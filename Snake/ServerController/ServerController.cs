using NetworkUtil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace SnakeGame
{
    /// <summary>
    /// A controller for handling updates to a Snake Game Server
    /// 
    /// Updates the model according to the game mechanics, keeps track 
    /// of clients, and broadcasts the world to them.
    /// </summary>
    public class ServerController
    {
        #region Model Params
        public World theWorld { get; private set; } = new();
        private int powerupId = 0;
        private int powerupSpawnDelay = 200;    // How long to wait before spawning a new powerup
        #endregion
        #region Directions
        private static Vector2D UP = new Vector2D(0, -1);
        private static Vector2D DOWN = new Vector2D(0, 1);
        private static Vector2D LEFT = new Vector2D(-1, 0);
        private static Vector2D RIGHT = new Vector2D(1, 0);
        #endregion
        public List<SocketState> Clients { get; private set; } = new();

        public ServerController() 
        {
            // Get the current settings
            XmlDocument settings = new();
            settings.Load("settings.xml");
            // Read the frame settings and world size
            XmlNode? fpshot = settings.SelectSingleNode("//GameSettings/FramesPerShot");
            if (fpshot != null)
            {
                theWorld.FramesPerShot = int.Parse(fpshot.InnerText);
            }
            XmlNode? mspframe = settings.SelectSingleNode("//GameSettings/MSPerFrame");
            if (mspframe != null)
            {
                theWorld.MSPerFrame = int.Parse(mspframe.InnerText);
            }
            XmlNode? respawnRate = settings.SelectSingleNode("//GameSettings/RespawnRate");
            if (respawnRate != null)
            {
                theWorld.RespawnRate = int.Parse(respawnRate.InnerText);
            }
            XmlNode? univereSize = settings.SelectSingleNode("//GameSettings/UniverseSize");
            if (univereSize != null)
            {
                theWorld.worldSize = int.Parse(univereSize.InnerText);
            }
            // Read the walls node
            XmlNode? wallsNode = settings.SelectSingleNode("//GameSettings/Walls");
            if (wallsNode != null)
            {   // Read each wall
                XmlNodeList walls = wallsNode.ChildNodes;
                foreach (XmlNode w in walls)
                {
                    XmlNode? id = w.SelectSingleNode("ID");
                    XmlNode? p1_x = w.SelectSingleNode("p1/x");
                    XmlNode? p1_y = w.SelectSingleNode("p1/y");
                    XmlNode? p2_x = w.SelectSingleNode("p2/x");
                    XmlNode? p2_y = w.SelectSingleNode("p2/y");
                    if (id != null && p1_x != null && p1_y != null && p2_x != null &&
                        p2_y != null)
                    {
                        Wall wall = new();
                        wall.id = int.Parse(id.InnerText);
                        wall.p1.X = double.Parse(p1_x.InnerText);
                        wall.p1.Y = double.Parse(p1_y.InnerText);
                        wall.p2.X = double.Parse(p2_x.InnerText);
                        wall.p2.Y = double.Parse(p2_y.InnerText);
                        theWorld.walls.Add(wall.id, wall);
                    }
                }
            }
        }




        #region Processing New Clients
        /// <summary>
        /// Waits for the newly connected client to send their name.
        /// </summary>
        /// <param name="state">the client</param>
        public void ClientConnection(SocketState state)
        {
            Console.WriteLine("Accepted new client.");
            state.OnNetworkAction = ClientNameReceived;
            Networking.GetData(state);
        }

        /// <summary>
        /// Processes a client who has sent their name by adding them to the list 
        /// of clients then sending them their player id, world size, and the walls.
        /// 
        /// 1. Send two strings representing integer numbers each terminated by a 
        ///     '\n'. The first number is the player's unique ID. The second is the 
        /// size of the world, representing both width and height.
        /// 2. Send the client all of the walls as JSON objects, each separated by 
        ///     a '\n'.
        /// 3. Continually send the current state of the rest of the game on every 
        ///     frame. Each object ends with a '\n' characer. There is no guarantee 
        ///     that all objects will be included in a single network send/receive 
        ///     operation. Objects can be sent in any order.
        /// </summary>
        /// <param name="state">the client being processed</param>
        private void ClientNameReceived(SocketState state)
        {
            // Wait for player name from client
            int clientId = (int)state.ID;
            string raw = state.GetData();
            string[] data = Regex.Split(raw, "\n");
            string playerName = data[0];
            playerName.Remove(playerName.Length - 3);
            state.RemoveData(0, playerName.Length);

            lock (theWorld)
            { // Add this client to the world as a newly spawned snake
                List<Vector2D> spawnLoc = SpawnSnake();
                // Calculate the snake's spawn Direction
                bool isVertical = spawnLoc[0].X == spawnLoc[1].X;
                Vector2D spawnDir;
                if (isVertical)
                {
                    if (spawnLoc[0].Y < spawnLoc[1].Y)
                    {   // Moving down
                        spawnDir = DOWN;
                    }
                    else
                    {   // Moving up
                        spawnDir = UP;
                    }
                }
                else
                {
                    if (spawnLoc[0].X < spawnLoc[1].X)
                    {   // Moving right
                        spawnDir = RIGHT;
                    }
                    else
                    {   // Moving left
                        spawnDir = LEFT;
                    }
                }
                Snake player = new Snake(clientId, spawnLoc, spawnDir, playerName);
                theWorld.snakes.Add(player.id, player);
            }

            // Send client ID and world size
            Networking.Send(state.TheSocket, clientId + "\n" + theWorld.worldSize + "\n");
            // Send client the walls
            StringBuilder wallsJSON = new();
            lock (theWorld)
            {
                foreach (Wall w in theWorld.walls.Values)
                {
                    wallsJSON.Append(JsonConvert.SerializeObject(w) + "\n");
                }
            }
            Networking.Send(state.TheSocket, wallsJSON.ToString());

            // Start sending client info on each frame
            Clients.Add(state);
            Console.WriteLine("Player(" + state.ID + ") \"" + playerName + "\" connected");
            // Allow the client to send move commands
            state.OnNetworkAction = ReceiveMoveCommand;
            Networking.GetData(state);
        }

        /// <summary>
        /// Randomly finds an empty spot in the world and returns a newly spawned 
        /// snake segment there. 
        /// 
        /// Snakes are 120 units upon respawn.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private List<Vector2D> SpawnSnake()
        {
            // Prepare a list of Snake segments to return
            List<Vector2D> body;

            // Choose a random axis-aligned orientation for snake
            Random rng = new();
            Vector2D spawnDir = UP; // assume vertical
            if (rng.Next(2) == 0) // 50% chance to swap to horizontal
            {
                spawnDir.Rotate(90);
            }
            if (rng.Next(2) == 0) // 50% chance to reverse direction
            {
                spawnDir *= -1;
            }

            // Find a valid spawn location for the snake's starting segment
            do
            {
                // Randomly place the head
                int xCor = rng.Next(-1 * theWorld.worldSize / 2, theWorld.worldSize / 2);
                int yCor = rng.Next(-1 * theWorld.worldSize / 2, theWorld.worldSize / 2);
                Vector2D head = new(xCor, yCor);
                // Calculate position of the rest of the body
                Vector2D tail = new Vector2D(head.X + (120 * spawnDir.X),
                    head.Y + (120 * spawnDir.Y));
                body = new List<Vector2D>();
                body.Add(tail);
                body.Add(head);
            } while (InvalidSpawn(body));

            // Then return the newly spawned snake's body
            return body;
        }

        #endregion

        #region Collision Checking
        /// <summary>
        /// Checks whether or not a given powerup location is an Invalid spawn location
        /// </summary>
        /// <param name="powerup"></param>
        /// <returns>Invalid spawn location?</returns>
        private bool InvalidSpawn(Vector2D powerup)
        {
            // Check for a collision between each snake-segment and every single collidable world object
            foreach (Wall w in theWorld.walls.Values)
            {   // walls
                List<Vector2D> wall = new();
                wall.Add(w.p1);
                wall.Add(w.p2);
                if (AreColliding(powerup, wall, 50))
                    return true;
            }
            foreach (Snake s in theWorld.snakes.Values)
            {   // other snakes
                if (AreColliding(powerup, s.body, 10))
                    return true;
            }

            // There were no collisions
            return false;
        }

        /// <summary>
        /// Checks whether or not a given snake body is an Invalid spawn location
        /// </summary>
        /// <param name="snake"></param>
        /// <returns>Invalid spawn location?</returns>
        private bool InvalidSpawn(List<Vector2D> snake)
        {
            // Check for a collision between each snake-segment and every single collidable world object
            foreach (Wall w in theWorld.walls.Values)
            {   // walls
                List<Vector2D> wall = new();
                wall.Add(w.p1);
                wall.Add(w.p2);
                if (AreColliding(snake, wall, 50))
                    return true;
            }
            foreach (Snake s in theWorld.snakes.Values)
            {   // other snakes
                if (AreColliding(snake, s.body, 10))
                    return true;
            }

            // There were no collisions
            return false;
        }

        /// <summary>
        /// Checks if two rectangles given by their topleft and botright corners 
        /// overlap. 
        /// </summary>
        /// <param name="rect1TL">rectangle one top left corner</param>
        /// <param name="rect1BR">rectangle one bottom right corner</param>
        /// <param name="rect2TL">rectangle two top left corner</param>
        /// <param name="rect2BR">rectangle two bottom right corner</param>
        /// <returns>whether or not the rectangles intersect</returns>
        private bool IsIntersectingRectangles(Vector2D rect1TL, Vector2D rect1BR,
                              Vector2D rect2TL, Vector2D rect2BR)
        {
            // if rectangle has area 0, no overlap
            if (rect1TL.X == rect1BR.X || rect1TL.Y == rect1BR.Y || rect2BR.X == rect2TL.X || rect2TL.Y == rect2BR.Y)
            {
                return false;
            }

            // If one rectangle is on left side of other
            if (rect1TL.X > rect2BR.X || rect2TL.X > rect1BR.X)
            {
                return false;
            }

            // If one rectangle is above other
            if (rect1BR.Y > rect2TL.Y || rect2BR.Y > rect1TL.Y)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks whether or not a body of a newly spawned snake is colliding with 
        /// an object
        /// 
        /// Newly spawned snakes are 120 units long with one straight axis-aligned 
        /// segment.
        /// </summary>
        /// <param name="body">newly spawned snake's body</param>
        /// <param name="obj">obj being collided with</param>
        /// <param name="width">width of the object being collided with</param>
        /// <returns>Whether or not the body and object have collided</returns>
        private bool AreColliding(List<Vector2D> body, List<Vector2D> obj, int width)
        {
            // Check the collision barrier one segment at a time
            Vector2D bodyTopLeft, bodyBottomRight, objTopLeft, objBottomRight;
            CalculateCollisionBarrier(body[0], body[1], 10, out bodyTopLeft, out bodyBottomRight);
            for (int i = 0; i < obj.Count - 1; i++)
            {
                Vector2D p1 = obj[i], p2 = obj[i + 1];
                CalculateCollisionBarrier(p1, p2, width, out objTopLeft, out objBottomRight);
                // Check for collision
                if (IsIntersectingRectangles(bodyTopLeft, bodyBottomRight, objTopLeft, objBottomRight))
                {
                    return true;
                }
            }

            // No collisions found
            return false;
        }

        /// <summary>
        /// Checks whether or not a given snake is colliding with itself
        /// </summary>
        /// <param name="s">snake</param>
        /// <returns>If the snake collided with itself</returns>
        private bool AreColliding(Snake s)
        {
            List<Vector2D> collidableSegments = s.body.GetRange(0, s.OppositeTurnJointIndex - 1);
            return AreColliding(s.body.Last(), collidableSegments, 10);
        }

        /// <summary>
        /// Checks whether or not a given vector is colliding with an object
        /// </summary>
        /// <param name="v">vector</param>
        /// <param name="obj">object being collided with</param>
        /// <param name="width">width of the object being collided with</param>
        /// <returns>Whether or not the vector and object have collided</returns>
        private bool AreColliding(Vector2D v, List<Vector2D> obj, int width)
        {
            // Check the collision barrier one segment at a time
            Vector2D topLeft, bottomRight;
            for (int i = 0; i < obj.Count - 1; i++)
            {
                Vector2D p1 = obj[i], p2 = obj[i + 1];
                CalculateCollisionBarrier(p1, p2, width, out topLeft, out bottomRight);
                // Check for collision
                if ((v.X > topLeft.X && v.X < bottomRight.X) &&
                    (v.Y > topLeft.Y && v.Y < bottomRight.Y))
                {
                    return true;
                }
            }

            // No collisions found
            return false;
        }

        /// <summary>
        /// Checks whether or not a given head is colliding with a powerup
        /// 
        /// Powerups have are 16 by 16 units
        /// </summary>
        /// <param name="head">Vector2D representing a head</param>
        /// <param name="p">powerup being collided with</param>
        /// <returns>Whether or not the head and powerup have collided</returns>
        private bool AreColliding(Vector2D head, PowerUp p)
        {
            // Calculate Collision Barrier
            Vector2D topLeft = new Vector2D(p.loc.X - 16, p.loc.Y - 16);
            Vector2D bottomRight = new Vector2D(p.loc.X + 16, p.loc.Y + 16);
            // Check for collision
            return (head.X > topLeft.X && head.X < bottomRight.X) &&
                    (head.Y > topLeft.Y && head.Y < bottomRight.Y);
        }

        /// <summary>
        /// Calculates the top left and bottom right corners of a collision barrier
        /// </summary>
        /// <param name="p1">start of segment</param>
        /// <param name="p2">end of segment</param>
        /// <param name="width">width of segment</param>
        /// <param name="topLeft"></param>
        /// <param name="bottomRight"></param>
        private void CalculateCollisionBarrier(Vector2D p1, Vector2D p2, int width,
            out Vector2D topLeft, out Vector2D bottomRight)
        {
            // Calcualte direction
            Vector2D segmentDir;
            bool isVertical = p1.X == p2.X;
            if (isVertical)
            {   // Vertical segment
                if (p1.Y < p2.Y)
                {
                    segmentDir = DOWN;
                }
                else
                {
                    segmentDir = UP;
                }
            }
            else
            {   // Horizontal segment
                if (p1.X < p2.X)
                {
                    segmentDir = RIGHT;
                }
                else
                {
                    segmentDir = LEFT;
                }
            }
            // Calculate corners of this segment of the collision barrier
            if (isVertical)
            {
                if (segmentDir == DOWN)
                {
                    topLeft = new Vector2D(p1.X - (width / 2) - 10, p1.Y - 10);
                    bottomRight = new Vector2D(p2.X + (width / 2) + 10, p2.Y + 10);
                }
                else
                {
                    topLeft = new Vector2D(p2.X - (width / 2) - 10, p2.Y - 10);
                    bottomRight = new Vector2D(p1.X + (width / 2) + 10, p2.Y + 10);
                }
            }
            else
            {
                if (segmentDir == RIGHT)
                {
                    topLeft = new Vector2D(p1.X - 10, p1.Y - (width / 2) - 10);
                    bottomRight = new Vector2D(p2.X + 10, p2.Y + (width / 2) + 10);
                }
                else
                {
                    topLeft = new Vector2D(p2.X - 10, p2.Y - (width / 2) - 10);
                    bottomRight = new Vector2D(p1.X + 10, p1.Y + (width / 2) + 10);
                }
            }
        }
        #endregion

        #region Receiving From Clients
        /// <summary>
        /// Receive a movement command from the client and update the client's 
        /// snake's direction.
        /// </summary>
        /// <param name="state">the client</param>
        private void ReceiveMoveCommand(SocketState state)
        {
            // Get data from the client
            string raw = state.GetData();
            state.RemoveData(0, raw.Length - 1);
            string[] data = Regex.Split(raw, "\n");

            // Check if they sent a move command
            bool foundCmd = false;
            int count = 0;
            ControlCommand cmd = new();
            while (!foundCmd && count < data.Length)
            {
                // Skip non-JSON strings
                if (!(data[count].StartsWith("{") && data[count].EndsWith("}")))
                {
                    continue;
                }

                // Parse the string into a JSON object
                JObject obj = JObject.Parse(data[count]);
                JToken? token = obj["moving"];
                if (token != null)
                {
                    cmd = JsonConvert.DeserializeObject<ControlCommand>(data[0]);
                    foundCmd = true;
                }
            }

            // Process the client's move command
            lock (theWorld)
            {
                Snake clientSnake = theWorld.snakes[(int)state.ID];
                if (cmd.moving == "up")
                {
                    clientSnake.direction = UP;
                }
                else if (cmd.moving == "right")
                {
                    clientSnake.direction = RIGHT;
                }
                else if (cmd.moving == "down")
                {
                    clientSnake.direction = DOWN;
                }
                else if (cmd.moving == "left")
                {
                    clientSnake.direction = LEFT;
                }
                // Update the client's snake's opposite direction for self-collision checking
                if (clientSnake.direction == (clientSnake.OppositeDirection * -1))
                {
                    clientSnake.OppositeDirection = clientSnake.direction;
                    clientSnake.OppositeTurnJointIndex = clientSnake.body.Count() - 2;
                }
            }
        }

        /// <summary>
        /// A JSON compatible structure for representing control commands from 
        /// a client.
        /// </summary>
        [JsonObject(MemberSerialization.OptIn)]
        internal struct ControlCommand
        {
            [JsonProperty]
            public string moving;

            /// <summary>
            /// Constructs a control command with a moving value of none
            /// </summary>
            [JsonConstructor]
            public ControlCommand()
            {
                this.moving = "none";
            }
        }

        #endregion

        #region Broadcasting To Clients
        /// <summary>
        /// Updates the state of each object in the world (movement, position, booleans) using the 
        /// Server Controller.
        /// </summary>
        public void UpdateWorld()
        {
            // Update the current state of the world
            lock (theWorld)
            {
                // Spawn powerups
                if (theWorld.powerups.Count() < theWorld.MaxPowerups)
                {
                    if (powerupSpawnDelay == 0)
                    {
                        // Find a valid spawn location for the snake's starting segment
                        Random rng = new();
                        Vector2D loc = new();   // tmp value
                        do
                        {
                            // Randomly place the powerup
                            int xCor = rng.Next(-1 * theWorld.worldSize / 2, theWorld.worldSize / 2);
                            int yCor = rng.Next(-1 * theWorld.worldSize / 2, theWorld.worldSize / 2);
                            loc = new(xCor, yCor);
                        } while (InvalidSpawn(loc));
                        // Reset the spawn timer for the next powerup
                        powerupSpawnDelay = rng.Next(0, 201);
                        // Spawn the powerup into the world
                        PowerUp p = new PowerUp(powerupId++, loc);
                        theWorld.powerups.Add(p.id, p);
                    }
                    else
                    {   // Lower the spawn timer by one frame
                        powerupSpawnDelay--;
                    }
                }

                // Update the snakes
                foreach (Snake s in theWorld.snakes.Values)
                {
                    s.died = false; // Snakes should only be set to died for one frame max
                    if (!s.alive)
                    {   // Lower the Snake's respawn timer
                        s.alive = ++s.FramesSpentDead >= theWorld.RespawnRate;
                        if (s.alive)
                        {
                            s.body = SpawnSnake();
                        }
                    }
                    else
                    {   // Move the snake
                        // Move the head
                        Vector2D head = s.body.Last() + (s.direction * 3);
                        s.body.Last().X = head.X;
                        s.body.Last().Y = head.Y;
                        // Check for Collision
                        foreach (Snake snakeBeingHit in theWorld.snakes.Values)
                        {
                            if (snakeBeingHit.id == s.id)
                            {   // Check for self collisions
                                if (AreColliding(s))
                                {
                                    s.died = true;
                                    break;
                                }
                            }
                            else
                            {   // Check for collisions with other snakes
                                if (AreColliding(s.body.Last(), snakeBeingHit.body, 10))
                                {
                                    s.died = true;
                                    s.alive = false;
                                    break;
                                }
                            }
                        }
                        if (!s.died)
                        {   // See if it died by hitting a wall
                            foreach (Wall w in theWorld.walls.Values)
                            {
                                List<Vector2D> wallSeg = new();
                                wallSeg.Add(w.p1);
                                wallSeg.Add(w.p2);
                                if (AreColliding(s.body.Last(), wallSeg, 50))
                                {
                                    s.died = true;
                                    s.alive = false;
                                    break;
                                }
                            }
                        }
                        else
                        {   // The snake died hitting a snake
                            break;
                        }
                        if (!s.died)
                        {   // See if the snake grabbed any powerups
                            foreach (PowerUp p in theWorld.powerups.Values)
                            {
                                if (AreColliding(s.body.Last(), p))
                                {   // The snake collided with a powerup
                                    s.score++;
                                    s.FoodInBelly += 12;
                                    break;
                                }
                            }
                        }
                        else
                        {   // The snake died hitting a wall
                            break;
                        }

                        // Tail-end movement
                        if (s.FoodInBelly > 0)
                        {   // The snake grows one frame worth of movement
                            s.FoodInBelly -= 1;
                        }
                        else
                        {   // Move the rest of the body starting from the tail
                            int movement = 3;
                            // Clean up tail-end joints
                            Vector2D dist = s.body[1] - s.body[0];
                            while (dist.Length() < movement)
                            {
                                movement -= (int)dist.Length();
                                s.body.RemoveAt(0);
                                dist = s.body[1] - s.body[0];
                            }
                            // Move the tail of the snake
                            Vector2D newTail = (s.direction * movement) + s.body[0];
                            s.body[0] = newTail;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Broadcasts the current state of the world to each client
        /// </summary>
        public void BroadcastWorld()
        {
            lock (theWorld)
            {
                // Serialize each object in the world
                StringBuilder jsonSerialization = new();
                foreach (Snake s in theWorld.snakes.Values)
                {   // Snakes
                    jsonSerialization.Append(JsonConvert.SerializeObject(s) + "\n");
                    s.join = false;
                }
                foreach (PowerUp p in theWorld.powerups.Values)
                {   // Powerups
                    jsonSerialization.Append(JsonConvert.SerializeObject(p) + "\n");
                }
                // Send each client the new state of the world
                foreach (SocketState c in Clients)
                {
                    Networking.Send(c.TheSocket, jsonSerialization.ToString());
                }
            }
        }

        #endregion
    }
}