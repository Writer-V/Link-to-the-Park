using LinkToThePark.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.Actors;
public abstract class Actor
{
    public Point Position {get; protected set;}
    protected Vector2 drawPosition;
    protected Point facing;
    public bool IsMoving {get; protected set;}
    protected float speed;
    // Future animator goes here
    protected Texture2D? texture; //Shouldn't be nullable — fix with along animations
    public Actor(Point position, float speed)
    {
        Position = position;
        drawPosition = Position.ToVector2() * Map.TileSize;
        this.speed = speed;
    }
    public MoveResult Move(Point direction, Map map)
    {
        if(IsMoving) return MoveResult.Blocked;
        facing = direction;
        Point target = Position + direction;
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
    public virtual void Update(GameTime gameTime)
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
    // TODO: Move general Actor Draw here once it can call an animator.
}