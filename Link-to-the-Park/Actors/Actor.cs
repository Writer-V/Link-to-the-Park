using LinkToThePark.Graphics;
using LinkToThePark.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.Actors;
public abstract class Actor
{
    public Point Position {get; protected set;}
    public Vector2 DrawPosition {get; private set;}
    protected static readonly Vector2 TileAnchor = 
        new(Map.TileSize / 2, Map.TileSize - 1); // (16, 31)
    protected Point facing;
    public void Face(Point direction) => facing = direction;
    public bool IsMoving {get; protected set;}
    public virtual bool IsHoldingItem {get; private set;}
    protected float speed;
    protected SpriteAnimator animator = null!;
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
    protected AnimSequenceType AnimationSelection()
    {
        if(IsHoldingItem)
            return AnimSequenceType.Pose;
        else if(IsMoving && facing == new Point(0,1))
            return AnimSequenceType.MoveDown;
        else if(IsMoving && facing == new Point(-1,0))
            return AnimSequenceType.MoveLeft;
        else if(IsMoving && facing == new Point(1,0))
            return AnimSequenceType.MoveRight;
        else if(IsMoving && facing == new Point(0,-1))
            return AnimSequenceType.MoveUp;
        else if(facing == new Point(-1,0))
            return AnimSequenceType.IdleLeft;
        else if(facing == new Point(1,0))
            return AnimSequenceType.IdleRight;
        else if(facing == new Point(0,-1))
            return AnimSequenceType.IdleUp;
        else return AnimSequenceType.Idle;
    }
    public virtual void Update(GameTime gameTime)
    {
        animator?.Play(AnimationSelection());
        animator?.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
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
    public virtual void Draw(SpriteBatch spriteBatch, Color? color = null)
    {
        animator?.Draw(spriteBatch, DrawPosition + TileAnchor, color);
    }
}