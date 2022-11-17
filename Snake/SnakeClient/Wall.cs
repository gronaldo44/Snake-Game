using SnakeGame;
using System;

/// <summary>
/// Walls will always be axis-aligned.
/// 
/// One sector of a wall has width fifty.
/// 
/// Walls can overlap and intersect each other.
/// </summary>
public class Wall
{   // TODO: JSON compatability
    public int wall { get; private set; }           // Wall's unique ID
    public Vector2D p1 { get; private set; }        // endpoint
    public Vector2D p2 { get; private set; }        // endpoint

    // TODO: constructor
}
