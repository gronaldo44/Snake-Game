﻿using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using NetworkUtil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Controller for a SnakeClient game
/// </summary>
public class GameController
{
    private Action DrawWorld;                       // Draws the world based on the model
    private SocketState connection;                 // This client's connection to the server  
    private string playerName;
    private World theWorld;
    #region JSON Properties
    private string moving;           // What direction the player is moving in
    #endregion

    /// <summary>
    /// Creates a game controller with the argued method for drawing the world
    /// </summary>
    /// <param name="draw"></param>
    public GameController(Action draw, World w)
    {
        DrawWorld = draw;
        connection = new SocketState((s) => { }, "no connection made");
        moving = "none";
        playerName = "";    // temporary value
        theWorld = w;
    }


    /// <summary>
    /// Connects to the argued server's host name on port 11000
    /// </summary>
    /// <param name="hostName"></param>
    public void Connect(string hostName, string playerName)
    {
        this.playerName = playerName;
        Networking.ConnectToServer(OnConnection, hostName, 11000);
    }

    /// <summary>
    /// Updates the direction the player is currently moving in
    /// </summary>
    /// <param name="dir"></param>
    public void MoveCommand(string dir)
    {
        moving = dir;
    }

    /// <summary>
    /// Handles connections to the server.
    /// 
    /// 1. Documents and draws the walls 
    /// 2. Starts event-looping for control commands
    /// 3. Begins updating the client each frame
    /// </summary>
    /// <param name="state"></param>
    private void OnConnection(SocketState state)
    {
        if (Networking.Send(state.TheSocket, playerName))
        {
            state.OnNetworkAction = InitializeWorld;
            Networking.GetData(state);
        }
    }

    private void InitializeWorld(SocketState state)
    {
        // Document the player ID and world size
        string raw = state.GetData();      // TODO: delete
        string[] data = Regex.Split(raw, "\n");
        theWorld.playerID = int.Parse(data[0]);
        theWorld.worldSize = int.Parse(data[1]);

        // Document walls
        lock (theWorld)
        {
            foreach (string str in data)
            {
                // Skip non-json strings
                if (!str.StartsWith('{') && !str.EndsWith('}'))
                {
                    continue;
                }

                // Parse the wall as a Json object
                JObject obj = JObject.Parse(str);
                JToken? token = obj["wall"];
                if (token != null)
                {   // Document the wall in the world
                    Wall w = JsonConvert.DeserializeObject<Wall>(str)!;
                    theWorld.walls.Add(w.wall, w);
                }
            }
        }
        // Draw the walls
        DrawWorld();

        // Update on each frame
        state.OnNetworkAction = OnFrame;
        Networking.GetData(state);
    }

    /// <summary>
    /// Method to be called by the server on each frame
    /// </summary>
    /// <param name="state"></param>
    private void OnFrame(SocketState state)
    {
        // Only one command may be received each frame
        string controlCommand = "{\"moving\":\"" + moving + "\"}\n";
        Networking.Send(state.TheSocket, controlCommand);

        // Update the values in the world
        theWorld.UpdateWorld(state);
        // Draw the world
        DrawWorld();
    }

}
