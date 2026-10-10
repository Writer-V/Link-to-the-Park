using System.Collections.Generic;

namespace LinkToThePark.World.Tiled;
public class TiledTile
{
    public int Id { get; set; }
    public string? Image { get; set; }
    public int ImageWidth { get; set; }
    public int ImageHeight { get; set; }
    public List<TiledProperty> Properties { get; set; } = new();
}