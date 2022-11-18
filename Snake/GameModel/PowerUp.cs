using SnakeGame;
using System;

/// <summary>
/// Consumable item that does something to snakes when 
/// they collide with it.
/// 
/// Powerups are 16x16 pixels
/// </summary>
public class PowerUp
{   // TODO: JSON Compatability
    public int power { get; private set; }      // unique ID
    public Vector2D loc { get; private set; }   // location in the world
    public bool died;   // Did the power-up die on this frame?

    public PowerUp(int power, Vector2D loc, bool died)
    {
        this.power = power;
        this.loc = loc;
        this.died = died;
    }
}
