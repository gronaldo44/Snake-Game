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



/// <summary>
/// TODO: Header comments.
/// </summary>
public class Server
{
    private static World theWorld = new();
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
    /// For unit testing
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
        // Get the game settings
        Server s = new();

        // Start listening for and receiving snake-clients
        TcpListener listener = Networking.StartServer(controller.ProcessClientConnection, 11000);

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

    #endregion

}
