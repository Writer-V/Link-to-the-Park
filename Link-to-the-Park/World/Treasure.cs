using Microsoft.Xna.Framework;

namespace LinkToThePark.World;
public class Treasure
{
    public TreasureType Type {get; private set;}
    public Point Tile {get; private set;}
    public int Points {get; private set;}
    public bool Opened {get; set;} = false;
    public Treasure(TreasureType type, Point position, int points = 0)
    {
        Type = type;
        Tile = position;
        Points = points;
    }
}