using Newtonsoft.Json;
using SnakeGame;
using System;

[JsonObject(MemberSerialization.OptIn)]
/// <summary>
/// Consumable item that does something to snakes when 
/// they collide with it.
/// 
/// Powerups are 16x16 pixels
/// </summary>
public class PowerUp
{
    [JsonProperty(PropertyName = "power")]
    public int id { get; private set; }      // unique ID
    [JsonProperty(PropertyName = "loc")]
    public Vector2D? loc { get; private set; }   // location in the world
    [JsonProperty(PropertyName = "died")]
    public bool died;   // Did the power-up die on this frame?

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
