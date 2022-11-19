using NetworkUtil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SnakeGame;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

/// <summary>
/// Represents all of the objects in the world and what direction the player is moving
/// </summary>
public class World
{
    public Dictionary<int, Snake> snakes { get; private set; }       // All snakes to be drawn each frame
    public Dictionary<int, Wall> walls;                              // All walls to be drawn on initialization
    public Dictionary<int, PowerUp> powerups { get; private set; }   // All powerups to be drawn each frame
    public int worldSize;                    // Size of each side of the world; the world is square
    public int playerID;                     // This client's snakes player ID

    // Construct an empty world
    public World()
    {
        // Initialize params
        snakes = new();
        powerups = new();
        walls = new();
    }

    /// <summary>
    /// Updates the state of the world by adding and removing snakes or powerups
    /// 
    /// Should only be called by the game controller
    /// </summary>
    /// <param name="data"></param>
    public void UpdateWorld(SocketState state)
    {
        // Get the new position of objects in the world
        Networking.GetData(state);
        string data = state.GetData();      // TODO: delete
        //Debug.WriteLine(data);
        string[] worldObjs = Regex.Split(data, "\n");

        // Update the positions of objects in the world
        lock (this)
        {
            foreach (string str in worldObjs)
            {
                // Skip non-json strings
                if (!str.StartsWith('{') && !str.EndsWith('}'))
                {
                    continue;
                }

                JObject obj = JObject.Parse(str);
                JToken? token;

                // Check if this object is a snake
                token = obj["snake"];
                if (token != null)
                {
                    Snake s = JsonConvert.DeserializeObject<Snake>(str)!;
                    // Document the snake in the world
                    if (snakes.ContainsKey(s.snake))
                    {
                        snakes[s.snake] = s;
                    } else
                    {
                        snakes.Add(s.snake, s);
                    }
                    continue;
                }
                // Check if this object is a powerup
                token = obj["power"];
                if (token != null)
                {
                    PowerUp p = JsonConvert.DeserializeObject<PowerUp>(str)!;
                    // Document the powerup in the world
                    if (powerups.ContainsKey(p.power))
                    {
                        powerups[p.power] = p;
                    } else
                    {
                        powerups.Add(p.power, p);
                    }
                    continue;
                }
            }
        }
    }
}
