using NetworkUtil;
using System;
using System.Collections.Generic;

public static class World
{
    public static List<Snake> snakes { get; private set; }      // All snakes to be drawn each frame
    public static List<Wall> walls { get; private set; }       // All walls to be drawn on initialization
    public static List<PowerUp> powerups { get; private set; } // All powerups to be drawn each frame

    // Construct an empty world
    static World()
    {
        // TODO: initialize the walls
    }

    /// <summary>
    /// Updates the state of the world by adding and removing snakes or powerups
    /// 
    /// Should only be called by the game controller
    /// </summary>
    /// <param name="data"></param>
    public static void UpdateWorld(SocketState state)
    {
        // TODO: update the snakes
        // TODO: update the powerups
    }
}
