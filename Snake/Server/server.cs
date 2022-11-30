namespace SnakeGame;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



/// <summary>
/// TODO: Header comments.
/// </summary>
internal class Server
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
    }

    /// <summary>
    /// TODO: update the state (movement, position, booleans) of each object
    /// </summary>
    private void OnFrame()
    {

        foreach (Snake snake in theWorld.snakes)
        {
            // if(dead)
            //      theWorld.snakes.remove(snake);
            // else while(there's food in the snake's belly) {
            //      snake.moveButHoldOff() // grow at the head but the tail should stay where it's at.
            //      snake.score++;
            // }
            // snake.move(); again
        }
        foreach (PowerUp pUp in theWorld.powerups)
        {
            // if(dead) {
            //      snake.food += what the powerup is worth.
            //      theWorld.powerups.remove(pUp);
            // }

            // else continue;
        }
    }

}

