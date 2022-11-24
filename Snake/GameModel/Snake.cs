using Newtonsoft.Json;
using SnakeGame;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

[JsonObject(MemberSerialization.OptIn)]
public class Snake
{
    [JsonProperty(PropertyName = "snake")]
    public int id { get; private set; }      // Unique id
    [JsonProperty(PropertyName = "body")]
    public List<Vector2D> body { get; private set; } = new();    // represents the entire body; first index tail; last index head
    [JsonProperty(PropertyName = "dir")]
    public Vector2D direction { get; private set; } = new();  // Snake's orientation
    [JsonProperty(PropertyName = "name")]
    public string name { get; private set; } = "";  // Player's name
    [JsonProperty(PropertyName = "score")]
    public int score;
    [JsonProperty(PropertyName = "died")]
    public bool died;   // Did the snake die on this frame?
    [JsonProperty(PropertyName = "alive")]
    public bool alive;  // Is this snake alive right now?
    [JsonProperty(PropertyName = "dc")]
    public bool dc;     // Did the snake disconnect on this frame?
    [JsonProperty(PropertyName = "join")]
    public bool join;   // Did the snake join on this frame?

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
