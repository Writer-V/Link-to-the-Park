using System.Collections.Generic;
using System.Text.Json;

namespace LinkToThePark.World.Tiled;
public class TiledObject
{
    public int Id {get; set;}
    public string Name {get; set;} = string.Empty;
    public string Type {get; set;} = string.Empty;
    public float X {get; set;}
    public float Y {get; set;}
    public List<TiledProperty> Properties {get; set;} = new();
    public JsonElement? GetProperty(string name)
    {
        foreach(var property in Properties)
            if (property.Name == name) 
                return property.Value;
        return null;
    }
}