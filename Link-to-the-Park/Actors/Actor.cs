using LinkToThePark.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.Actors;
public abstract class Actor
{
    public Point Position {get; protected set;}
    public Vector2 DrawPosition {get; private set;}
    protected Point facing;
    public bool IsMoving {get; protected set;}
    protected float speed;
    // Future animator goes here
    protected Texture2D? texture; //Shouldn't be nullable — fix with along animations
    public Actor(Point position, float speed)
    {
        Position = position;
        DrawPosition = Position.ToVector2() * Map.TileSize;
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
        DrawPosition = position.ToVector2() * Map.TileSize;
        IsMoving = false;
    }
    public virtual void Update(GameTime gameTime)
    {
        if(!IsMoving) return;
        Vector2 target = Position.ToVector2() * Map.TileSize;
        float step = speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        if(Vector2.Distance(DrawPosition, target) <= step)
        { //Snap in place if close enough
            DrawPosition = target;
            IsMoving = false;
        }
        else
        {
            DrawPosition += Vector2.Normalize(target - DrawPosition) * step;
        }
    }
    // TODO: Move general Actor Draw here once it can call an animator.
}