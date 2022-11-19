using NetworkUtil;
using Newtonsoft.Json;
using SnakeGame;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

/// <summary>
/// Represents all of the objects in the world and what direction the player is moving
/// </summary>
public static class World
{
    public static Dictionary<int, Snake> snakes { get; private set; }       // All snakes to be drawn each frame
    public static Dictionary<int, Wall> walls;                              // All walls to be drawn on initialization
    public static Dictionary<int, PowerUp> powerups { get; private set; }   // All powerups to be drawn each frame
    public static int worldSize;                    // Size of each side of the world; the world is square


    // Construct an empty world
    static World()
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
    public static void UpdateWorld(SocketState state)
    {
        // Get the new position of objects in the world
        Networking.GetData(state);
        string[] worldObjs = Regex.Split(state.GetData(), "\n");

        // Update the positions of objects in the world
        foreach (string obj in worldObjs)
        {
            if (obj.Length > 1)
            {
                char typeIdentifier = obj.ElementAt(2);
                if (typeIdentifier == 's')
                {   // Update the snakes
                    Snake? snake = JsonConvert.DeserializeObject<Snake>(obj);
                    if (snake != null)
                    {
                        // only document the snake if it is still connected
                        if (!snake.dc)
                        {
                            if (snakes.TryGetValue(snake.snake, out Snake? old))
                            {   // Change to the new value
                                snakes[snake.snake] = snake;
                            } else
                            {   // Add the new snake
                                snakes.Add(snake.snake, snake);
                            }
                        }
                        else
                        {
                            snakes.Remove(snake.snake);
                        }
                    }
                }
                else if (typeIdentifier == 'p')
                {   // Update the powerups
                    PowerUp? powerup = JsonConvert.DeserializeObject<PowerUp>(obj);
                    if (powerup != null)
                    {
                        // only document the powerup if it is still alive
                        if (!powerup.died)
                        {
                            if (powerups.TryGetValue(powerup.power, out PowerUp? old))
                            {   // change to the new value
                                powerups[powerup.power] = powerup;
                            }
                            else
                            {   // add the new powerup
                                powerups.Add(powerup.power, powerup);
                            }
                        }
                        else
                        {
                            powerups.Remove(powerup.power);
                        }
                    }
                }
            }
        }
    }
}
