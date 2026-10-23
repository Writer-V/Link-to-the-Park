using LinkToThePark.Audio;
using Microsoft.Xna.Framework;

namespace LinkToThePark.World;
public class Treasure
{
    public TreasureType Type {get;}
    public Point Tile {get;}
    public int Points {get;}
    public bool Opened {get; private set;} = false;
    public bool BlocksMovement => 
        Type == TreasureType.Chest ||
        (Type == TreasureType.BreakableVase && !Opened);
    public Treasure(TreasureType type, Point position, int points = 0)
    {
        Type = type;
        Tile = position;
        Points = points;
    }
    public void Open()
    {
        Opened = true;
        if(Type == TreasureType.Chest)
            AudioManager.Play("chestOpen");
        if(Type == TreasureType.BreakableVase)
            AudioManager.Play("vaseBreak");
    }
}