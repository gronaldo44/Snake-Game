namespace SnakeGame;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;



/// <summary>
/// TODO: Header comments.
/// </summary>
public class Server
{

    // Need access to the world.
    World theWorld;

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
        theWorld = new();

        // Get the current settings
        XmlDocument settings = new();
        settings.Load("settings.xml");
        // Read the frame settings and world size
        XmlNode? fpshot = settings.SelectSingleNode("//GameSettings/FramesPerShot");
        if (fpshot != null)
        {
            Console.WriteLine(fpshot.InnerText);
        }
        XmlNode? mspframe = settings.SelectSingleNode("//GameSettings/MSPerFrame");
        if (mspframe != null)
        {
            Console.WriteLine(mspframe.InnerText);
        }
        XmlNode? respawnRate = settings.SelectSingleNode("//GameSettings/RespawnRate");
        if (respawnRate != null)
        {
            Console.WriteLine(respawnRate.InnerText);
        }
        XmlNode? worldSize = settings.SelectSingleNode("//GameSettings/UniverseSize");
        if (worldSize != null)
        {
            Console.WriteLine(worldSize.InnerText);
        }
        // Read the walls
        XmlNodeList? walls = settings.SelectNodes("//GameSettings/Wall");
        if (walls != null)
        {   // TODO: debug starting here
            foreach (XmlNode w in walls)
            {
                XmlNode? id = w.SelectSingleNode("ID");
                XmlNode? p1_x = w.SelectSingleNode("//p1/x");
                XmlNode? p1_y = w.SelectSingleNode("//p1/y");
                XmlNode? p2_x = w.SelectSingleNode("//p2/x");
                XmlNode? p2_y = w.SelectSingleNode("//p2/y");
                if (id != null && p1_x != null && p1_y != null && p2_x != null && 
                    p2_y != null)
                {
                    Console.WriteLine("Wall: \n\tid: " + id.InnerText);
                    Console.WriteLine("\tP1:\n\t\tx: " + p1_x.InnerText);
                    Console.WriteLine("\t\ty: " + p1_y.InnerText);
                    Console.WriteLine("\tP2:\n\t\tx: " + p2_x.InnerText);
                    Console.WriteLine("\t\ty: " + p2_y.InnerText);
                }
            }
        }
    }

    /// <summary>
    /// TODO: update the state (movement, position, booleans) of each object
    /// </summary>
    private void OnFrame()
    {
        // TODO: update the state (movement, position, booleans) of each object
        //foreach (object obj in theWorld)
        //{
        //    // if(dead) remove
        //    // else obj.move()  or obj.update()
        //}
    }

    /// <summary>
    /// For unit testing
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
        Server s = new();
    }
}
