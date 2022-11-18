using SnakeGame;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

public class Snake
{   // TODO: JSON Compatability
    public int snake { get; private set; }      // Unique id
    public string name { get; private set; }    // Player's name
    public List<Vector2D> body; // represents the entire body; first index tail; last index head
    public Vector2D dir { get; private set; }   // Snake's orientation
    public int score;
    public bool died;   // Did the snake die on this frame?
    public bool alive;  // Is this snake alive right now?
    public bool dc;     // Did the snake disconnect on this frame?
    public bool join;   // Did the snake join on this frame?

    public Snake(int snake, string name, List<Vector2D> body, Vector2D dir,
        int score, bool died, bool alive, bool dc, bool join)
    {
        this.snake = snake;
        this.name = name;
        this.body = body;
        this.dir = dir;
        this.score = score;
        this.died = died;
        this.alive = alive;
        this.dc = dc;
        this.join = join;
    }
}
