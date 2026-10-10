namespace LinkToThePark.World.Tiled;

public class TiledLayer
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public uint[]? Data { get; set; }
}