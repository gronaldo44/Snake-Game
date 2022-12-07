﻿namespace SnakeGame;

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
/// This Class acts as a Server for a multiplayer Snake game. It maintains the state of the world, 
/// computes all game mechanics, and communicates to the clients (each player of the game).
/// 
/// This project was written using the Model, View, Controller design pattern. The solution contains
/// projects for each one. "SnakeClient" is the project for the view.
/// 
/// Authors: Ronald Foster & Shem Snow
/// Created 11/25/2022
/// Last Updated: 12/08/2022
/// </summary>
public class Server
{
    private static ServerController controller = new();

    /// TODO: do we even need a default Constructor?
    //public Server()
    //{
    //}

    /// <summary>
    /// Creates and runs a server using the settings read from a "settings.xml" file.
    /// A TCP Listener is used to pass control of the server to the "ServerControler" project/class 
    /// then an infinite loop to update the game's world runs until the server application is closed.
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
        // Get the game settings
        Server s = new();
        // Start listening for and receiving snake-clients
        TcpListener listener = Networking.StartServer(controller.ClientConnection, 11000);
        Console.WriteLine("Server is now accepting clients.");

        // Start updating and broadcasting the state of the world to each client
        Stopwatch serverFPS = new(), frameTimer = new();
        frameTimer.Start();
        serverFPS.Start();
        while (true)
        {   // Loop for each frame until the server is stopped
            controller.GetClientData();
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

}
