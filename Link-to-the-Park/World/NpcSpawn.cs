using Microsoft.Xna.Framework;

namespace LinkToThePark.World;
public record NpcSpawn
{
    public int Id {get; init;}
    public Point Tile {get; init;}
    public NpcSpawn(int id, Point tile)
    {
        Id = id;
        Tile = tile;
    }
}