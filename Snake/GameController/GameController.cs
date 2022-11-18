using System;
using NetworkUtil;

/// <summary>
/// Controller for a SnakeClient game
/// </summary>
public class GameController
{
    private Action DrawWorld;

    /// <summary>
    /// Creates a game controller with the argued method for drawing the world
    /// </summary>
    /// <param name="draw"></param>
    public GameController(Action draw)
    {
        DrawWorld = draw;
    }


    /// <summary>
    /// Connects to the argued server's host name on port 11000
    /// </summary>
    /// <param name="hostName"></param>
    public void Connect(string hostName)
    {
        Networking.ConnectToServer(OnFrame, hostName, 11000);
    }

    /// <summary>
    /// Updates the direction the player is currently moving in
    /// </summary>
    /// <param name="dir"></param>
    public void MoveCommand(string dir)
    {
        World.moving = dir;
    }

    /// <summary>
    /// Method to be called by the server on each frame
    /// </summary>
    /// <param name="state"></param>
    private void OnFrame(SocketState state)
    {
        // Update the values in the world
        World.UpdateWorld(state);
        
        // Draw the world
        DrawWorld();
    }
}
