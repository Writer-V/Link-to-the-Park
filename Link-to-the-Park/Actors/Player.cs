using System;
using LinkToThePark.Graphics;
using LinkToThePark.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.Actors;
public class Player
{
    public Point Position {get; private set;}
    public Point Facing { get; private set;}
    private Texture2D texture; //Really needs an animator class...
    public Player(Point pos)
    {
        Position = pos;
        texture = TextureManager.Get("characters/PlayerCharacter1");
    }
    public bool TryMove(Point direction, Map map)
    {
        Facing = direction;
        Point target = Position + direction;
        System.Diagnostics.Debug.WriteLine($"Attempted direction {direction.X},{direction.Y}, to {target.X},{target.Y} from {Position.X},{Position.Y}");
        if(!map.IsInside(target))
            // Do the map shifting, but for now...
            return false;
        if(map.IsSolid(target))
            return false;
        Position = target;
        return true;
    }
    public void Draw(SpriteBatch spriteBatch)
    {
        // TODO: Replace with proper anim and soure selection.
        spriteBatch.Draw(
    texture,
    new Vector2(Position.X * Map.TileSize + 16, Position.Y * Map.TileSize + 31),
    new Rectangle(0, 0, 128, 128),
    Color.White,
    0f,                     // rotation
    new Vector2(64, 95),    // origin: Feet
    1f,                     // scale
    SpriteEffects.None,
    0f);
    }
}