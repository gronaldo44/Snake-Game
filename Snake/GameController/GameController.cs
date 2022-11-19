using System;
using System.Text.RegularExpressions;
using NetworkUtil;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
/// <summary>
/// Controller for a SnakeClient game
/// </summary>
public class GameController
{
    private Action DrawWorld;                       // Draws the world based on the model
    private SocketState connection;                 // This client's connection to the server
    private bool readyToReceiveCommands = false;    // Is the server ready to receive commands
    public static int playerID { get; private set; }       // This client's snake ID
    private string playerName;
    #region JSON Properties
    [JsonProperty]
    private string moving;           // What direction the player is moving in
    #endregion

    /// <summary>
    /// Creates a game controller with the argued method for drawing the world
    /// </summary>
    /// <param name="draw"></param>
    public GameController(Action draw)
    {
        DrawWorld = draw;
        connection = new SocketState((s) => { }, "no connection made");
        moving = "none";
        playerName = "";    // temporary value
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
        lock (this)
        {
            if (readyToReceiveCommands)
            {
                moving = dir;
                Networking.Send(connection.TheSocket, JsonConvert.SerializeObject(this));
                readyToReceiveCommands = false;
            }
        }
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
            state.OnNetworkAction = InitializePlayerIDAndWorldSize;
            Networking.GetData(state);
        }
    }

    private void InitializePlayerIDAndWorldSize(SocketState state)
    {
        // Document the player ID and world size
        string[] data = Regex.Split(state.GetData(), "\n");
        playerID = int.Parse(data[0]);
        World.worldSize = int.Parse(data[1]);

        // Get the walls
        state.OnNetworkAction = InitializeWalls;
        Networking.GetData(state);
    }

    private void InitializeWalls(SocketState state)
    {
        // Get the walls
        string[] data = Regex.Split(state.GetData(), "\n");

        // Document walls
        for (int i = 2; i < data.Length; i++)
        {
            Wall? w = JsonConvert.DeserializeObject<Wall>(data[i]);
            if (w != null)
            {
                World.walls.TryAdd(w.wall, w);
            }
        }
        // Draw the walls
        DrawWorld();

        // Start event-looping for control commands
        readyToReceiveCommands = true;
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
        readyToReceiveCommands = true;

        // Update the values in the world
        World.UpdateWorld(state);
        // Draw the world
        DrawWorld();
    }

    /// <summary>
    /// Calculates the screen's location and instantiates the value to 
    /// the argued paramaters.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public static void CalculateScreenLoc(out float x, out float y)
    {
        if (World.snakes.TryGetValue(playerID, out Snake? player))
        {
            x = (float)player.body.Last().GetX();
            y = (float)player.body.Last().GetY();
        } else
        {   // The screen should be centered at the middle
            x = 0;
            y = 0;
        }
    }
}
