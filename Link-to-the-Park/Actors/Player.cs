using System;
using LinkToThePark.Graphics;
using LinkToThePark.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.Actors;
public class Player
{
    public Point Position {get; private set;}
    private Vector2 drawPosition;
    public bool IsMoving {get; private set;}
    private const float speed = 128f;
    private Point facing;
    private Texture2D texture; //Really needs an animator class...
    public Point FacingTile => Position + facing;
    public Player(Point pos)
    {
        Position = pos;
        drawPosition = Position.ToVector2() * Map.TileSize;
        texture = TextureManager.Get("characters/PlayerCharacter1");
    }
    public MoveResult Move(Point direction, Map map)
    {
        if(IsMoving) return MoveResult.Blocked;
        facing = direction;
        Point target = Position + direction;
        System.Diagnostics.Debug.WriteLine($"Attempted direction {direction.X},{direction.Y}, to {target.X},{target.Y} from {Position.X},{Position.Y}");
        if(!map.IsInside(target))
            return MoveResult.MapExit;
        if(map.IsSolid(target))
            return MoveResult.Blocked;
        Position = target;
        IsMoving = true;
        return MoveResult.Walkable;
    }
    public void PlaceAt(Point position)
    {
        Position = position;
        drawPosition = position.ToVector2() * Map.TileSize;
        IsMoving = false;
    }
    public void Update(GameTime gameTime)
    {
        if(!IsMoving) return;
        Vector2 target = Position.ToVector2() * Map.TileSize;
        float step = speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        if(Vector2.Distance(drawPosition, target) <= step)
        { //Snap in place if close enough
            drawPosition = target;
            IsMoving = false;
        }
        else
        {
            drawPosition += Vector2.Normalize(target - drawPosition) * step;
        }
    }
    public void Draw(SpriteBatch spriteBatch)
    {
        // TODO: Replace with proper anim and soure selection.
        spriteBatch.Draw(
            texture,
            drawPosition + new Vector2(16, 31),
            new Rectangle(0, 0, 128, 128), Color.White, 0f,
            new Vector2(64, 95), // origin: Feet
            1f, SpriteEffects.None, 0f);
    }
}