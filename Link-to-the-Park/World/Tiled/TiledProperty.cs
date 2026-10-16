using System.Text.Json;

namespace LinkToThePark.World.Tiled;
public class TiledProperty
{
    public string Name { get; set; } = string.Empty;
    public JsonElement Value { get; set; }
}