using System.Collections.Generic;

namespace LinkToThePark.World.Tiled;

public class TiledLayer
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool Visible {get; set;} = true;
    public uint[]? Data { get; set; }
    public List<TiledObject>? Objects {get; set;}
}