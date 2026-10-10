using System.Collections.Generic;

namespace LinkToThePark.World.Tiled;

public class TiledMap
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int TileWidth { get; set; }
    public int TileHeight { get; set; }
    public List<TiledLayer> Layers { get; set; } = new();
    public List<TiledTileset> Tilesets { get; set; } = new();
}