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
using System.Data;



/// <summary>
/// TODO: Header comments.
/// </summary>
public class Server
{
    private static ServerController controller = new();

    /*
     * TODO: Abstract logic such that,
     *      Server is a landing point that doesn't touch the model
     *      ServerController handles communications between Server and the model
     */

    #region Server Boot
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
        TcpListener listener = Networking.StartServer(controller.ClientConnection, 11000);

        // Start updating and broadcasting the state of the world to each client
        Stopwatch serverFPS = new(), frameTimer = new();
        frameTimer.Start();
        serverFPS.Start();
        while (true)
        {   // Loop for each frame until the server is stopped
            controller.UpdateWorld();
            controller.BroadcastWorld();
            
            // Print the current framerate the server is running at
            if (serverFPS.ElapsedMilliseconds >= 1000)
            {
                Console.WriteLine("FPS: " + (1000 / controller.theWorld.MSPerFrame));
                serverFPS.Restart();
            }

            while (frameTimer.ElapsedMilliseconds < controller.theWorld.MSPerFrame)
            {
                // Wait for the frame to finish
            }
            frameTimer.Restart();
        } 
    }

    #endregion

}
