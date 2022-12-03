namespace SnakeGame;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NetworkUtil;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;



/// <summary>
/// TODO: Header comments.
/// </summary>
public class Server
{

    #region Model Params
    private static World theWorld = new();
    private static int snakeId = 0;
    private static int powerupId = 0;
    #endregion
    private static List<SocketState> clients = new();


    /*
     * TODO: I think we need to add a project reference to the server controller
     * 
     * ServerController serverControl = new(Action updateArrived, Action<SocketState> errorOccurred, World w);
     */



    /**
     * TODO: Need a collection of all commands that need to be executed on this frame.
     * Or maybe we could just change an attribute (such as direction or boolean flag) when the 
     * command is received then update the objects' appearance at the start of each frame.
     */

    /// <summary>
    /// TODO: Default Constructor
    /// </summary>
    public Server()
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
            Console.WriteLine("break");
        }
    }

    /// <summary>
    /// Updates the state of each object in the world (movement, position, booleans) using the 
    /// Server Controller.
    /// </summary>
    private static void OnFrame()
    {
        /*
         * TODO: OnFrame/Update
         * 
         * foreach(SocketState client in clients) {
         * 
         *      -Networking.GetData(SocketState eachClient)
         *      
         *      -Update the positions of powerups
         *      -Receive move commands from clients
         *      -Update the positions of snakes and handle collisions
         *      -Send the data back to each client
         *      
         * }
         */
        // Update the current state of the world


        // Send each client the new state of the world

        // Wait for the frame to finish
        // Start the next frame
    }

    /// <summary>
    /// Processes a new client connection, performs the network protocol 
    /// handshake before, then sends the world's objects on each frame.
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
    private static void ProcessClientConnection(SocketState state)
    {
        // Wait for player name from client
        while (state.GetData() == "")
        { }
        int clientId = snakeId++;
        string playerName = state.GetData();
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
                    spawnDir = new Vector2D(0, 1);
                }
                else
                {   // Moving up
                    spawnDir = new Vector2D(0, -1);
                }
            }
            else
            {
                if (spawnLoc[0].X < spawnLoc[1].X)
                {   // Moving right
                    spawnDir = new Vector2D(1, 0);
                }
                else
                {   // Moving left
                    spawnDir = new Vector2D(-1, 0);
                }
            }
            Snake player = new Snake(clientId, spawnLoc, spawnDir, playerName);
            theWorld.snakes.Add(player.id, player);
        }

        // Send client ID and world size
        Networking.Send(state.TheSocket, clientId + "\n" + theWorld.worldSize + "\n");
        // Send client the walls
        Networking.Send(state.TheSocket, JsonConvert.SerializeObject(theWorld.walls.Values));
        // Allow the client to send move commands
        state.OnNetworkAction = ReceiveMoveCommand;

        // Start sending client info on each frame
        clients.Add(state);
    }

    /// <summary>
    /// Randomly finds an empty spot in the world and returns a newly spawned 
    /// snake segment there. 
    /// 
    /// Snakes are 120 units upon respawn.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private static List<Vector2D> SpawnSnake()
    {
        // Prepare a list of Snake segments to return
        List<Vector2D> segments;

        // Choose a random axis-aligned orientation for snake
        Random rng = new();
        Vector2D spawnDir = new(1, 0); // assume vertical
        if (rng.Next(2) == 0) // 50% chance to swap to horizontal
        {
            spawnDir.Rotate(90);
        }
        if (rng.Next(2) == 0) // 50% chance to reverse direction
        {
            spawnDir *= -1;
        }

        // Find a straight, empty list of vectors that don't collide with anything.
        do
        {
            // Starting at a random (head) location
            int xCor = rng.Next(-1 * theWorld.worldSize / 2, theWorld.worldSize / 2);
            int yCor = rng.Next(-1 * theWorld.worldSize / 2, theWorld.worldSize / 2);
            Vector2D head = new(xCor, yCor);

            // (Re)set the segment-list to contain a single (head) segment
            segments = new();
            segments.Add(head);

            // Calculate the distance between each snake segment.
            int framesWorth = 12; // The default is 12 frames worth of movement
            Vector2D increment = new Vector2D(spawnDir) * framesWorth;

            // Add the trailing segments to the list
            int snakeSize = 10; // TODO: I think "snakeSize" and "framesworth" can be defined globally by the constructor after reading the settings file.
            for (int i = 0; i < snakeSize - 1; i++)
                segments.Add(segments.Last() + increment);

        // Repeat this process until a straight, empty, non-colliding list of vectors is found
        } while (AreColliding(segments)); 

        // Then return that list of vectors
        return segments;
    }

    /// <summary>
    /// Overloaded method of collision detection for a list of snake segments.
    /// This method just calls the primary "AreColliding" method to check for collisions
    /// between each snake segment and each world object.
    /// </summary>
    /// <param name="snake"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private static bool AreColliding(List<Vector2D> snake)
    {
        // Check for a collision between each snake-segment and every single collidable world object
        foreach(Vector2D segment in snake)
        {
            // walls
            foreach(Wall w in theWorld.walls.Values)
                if(AreColliding(segment, w))
                    return true;
            // other snakes
            foreach (Snake s in theWorld.snakes.Values)
                if (AreColliding(segment, s))
                    return true;
            // power-ups
            foreach (PowerUp pUp in theWorld.powerups.Values)
                if (AreColliding(segment, pUp))
                    return true;
        }

        return false;
    }

    /// <summary>
    /// TODO: This method might need to be overloaded once for each possible type-pair of arguments.
    /// Returns a boolean indicating if the two argued world objects are colliding.
    /// </summary>
    /// <param name="obj1"></param>
    /// <param name="obj2"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private static bool AreColliding(Object obj1, Object obj2)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Receive a movement command from the client and update the client's 
    /// snake's direction.
    /// </summary>
    /// <param name="state">the client</param>
    private static void ReceiveMoveCommand(SocketState state)
    {
        // Get data from the client
        string raw = state.GetData();
        state.RemoveData(0, raw.Length - 1);
        string[] data = Regex.Split(raw, "\n");

        // Check if they sent a move command
        bool foundCmd = false;
        int count = 0;
        ControlCommand cmd;
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

        // TODO: Process the client's move command
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
    }



    /// <summary>
    /// For unit testing
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
        // Get the game settings
        Server s = new();

        // Start listening for and receiving snake-clients
        TcpListener listener = Networking.StartServer(ProcessClientConnection, 11000);

        // TODO: Finish connection with the client (send them their id, worldsize, and walls).... I think this is done in "OnConnect".


        // TODO: Start sending onFrame to the clients
        /*
         * while(there are no clients connected) {spin/do nothing}
         * 
         * while(there are clients connected) {OnFrame/Update()}
         */

        /*
         * TODO: Allow the client to send move commands ........ 
         * 
         * The assignment instructions say
         *      "The commands should be applied on the next frame after receiving them"
         *      
         * So I think we should handle this in OnFrame/Update()
         * 
         *      - maybe we could save each move command in a variable (Snakes already have a "dir") 
         *      then at the start of the for-each-client-loop in OnFrame:
         *          
         *          World.snakes[ID].position += velocity * dir;
         *      
         *          if(the new command is valid)
         *              World.snakes[ID].dir = new command;
         */

    }
}
