using System.Collections.Generic;

namespace LinkToThePark.World.Tiled;

public class TiledTileset
{
    public int FirstGid { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Columns { get; set; }
    public string? Image { get; set; }
    public int TileWidth { get; set; }
    public int TileHeight { get; set; }
    public List<TiledTile> Tiles { get; set; } = new();
}