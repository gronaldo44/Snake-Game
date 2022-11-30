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
        XmlReader settings = XmlReader.Create("settings.xml");
        // TODO: Read the frame settings and world size
        // TODO: Read the walls
        while (settings.Read())
        {
            //Console.WriteLine(settings.Value);
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
