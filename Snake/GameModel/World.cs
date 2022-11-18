using NetworkUtil;
using Newtonsoft.Json;
using SnakeGame;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

[JsonObject(MemberSerialization.OptIn)]
/// <summary>
/// Represents all of the objects in the world and what direction the player is moving
/// </summary>
public static class World
{
    public static List<Snake> snakes { get; private set; }      // All snakes to be drawn each frame
    public static List<Wall> walls { get; private set; }        // All walls to be drawn on initialization
    public static List<PowerUp> powerups { get; private set; }  // All powerups to be drawn each frame
    [JsonProperty]
    public static string moving { private get; set; }           // What direction the player is moving in


    // Construct an empty world
    static World()
    {
        // Initialize params
        snakes = new List<Snake>();
        powerups = new List<PowerUp>();
        walls = new List<Wall>();
        moving = "none";

        // TODO: set the walls in the world
    }

    /// <summary>
    /// Updates the state of the world by adding and removing snakes or powerups
    /// 
    /// Should only be called by the game controller
    /// </summary>
    /// <param name="data"></param>
    public static void UpdateWorld(SocketState state)
    {
        // Get the new position of objects in the world
        Networking.GetData(state);
        JsonArray jsonObjs = new JsonArray(state.GetData());

        // Update the positions of objects in the world
        foreach (JsonNode? node in jsonObjs)
        {
            if (node != null)
            {
                char typeIdentifier = node.ToString().ElementAt(2);
                if (typeIdentifier == 's')
                {   // Update the snakes
                    SnakeStruct s = node.Deserialize<SnakeStruct>();
                    snakes.Add(new Snake(s.snake, s.name, s.body, s.dir, s.score, 
                        s.died, s.alive, s.dc, s.join));
                }
                else if (typeIdentifier == 'p')
                {   // Update the powerups
                    PowerUpStruct p = node.Deserialize<PowerUpStruct>();
                    powerups.Add(new PowerUp(p.power, p.loc, p.died));
                }
            }
        }
    }

    /// <summary>
    /// Structure of a Snake object 
    /// 
    /// This structure helps with null checks
    /// </summary>
    private struct SnakeStruct
    {
        public int snake { get; private set; }      // Unique id
        public string name { get; private set; }    // Player's name
        public List<Vector2D> body; // represents the entire body; first index tail; last index head
        public Vector2D dir { get; private set; }   // Snake's orientation
        public int score;
        public bool died;   // Did the snake die on this frame?
        public bool alive;  // Is this snake alive right now?
        public bool dc;     // Did the snake disconnect on this frame?
        public bool join;   // Did the snake join on this frame?
    }

    /// <summary>
    /// Structure of a PowerUp object
    /// 
    /// This structure helps iwth null checks
    /// </summary>
    private struct PowerUpStruct
    {
        public int power { get; private set; }      // unique ID
        public Vector2D loc { get; private set; }   // location in the world
        public bool died;   // Did the power-up die on this frame?
    }
}
