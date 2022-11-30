namespace SnakeGame;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



/// <summary>
/// TODO: Header comments.
/// </summary>
internal class server
{

    // TODO: Need a collection of moveable objects. Or just access to the world.
    World theWorld;

    /**
     * TODO: Need a collection of all commands that need to be executed on this frame.
     * Or maybe we could just change an attribute (such as direction or boolean flag) when the 
     * command is received then update the objects' appearance at the start of each frame.
     */

    /// <summary>
    /// TODO: Default Constructor
    /// </summary>
    public server()
    {
        theWorld = new();
    }

    /// <summary>
    /// TODO: contract
    /// </summary>
    private void OnFrame()
    {
        // TODO: update the state (movement, position, booleans) of each object
        foreach (object obj in theWorld)
        {
            // if(dead) remove
            // else obj.move()  or obj.update()
        }
    }

}

