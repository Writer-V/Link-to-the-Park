using System;
using LinkToThePark.Audio;
using LinkToThePark.Graphics;
using LinkToThePark.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.Actors;
public abstract class Actor
{
    public string Name {get; protected set;} = "";
    public Point Position {get; protected set;}
    public Vector2 DrawPosition {get; private set;}
    protected static readonly Vector2 TileAnchor = 
        new(Map.TileSize / 2, Map.TileSize - 1); // (16, 31)
    protected Point facing;
    public void Face(Point direction) => facing = direction;
    public bool IsMoving {get; protected set;}
    private float solidBumpSound;
    public virtual bool IsHoldingItem {get; private set;}
    protected float speed;
    protected SpriteAnimator animator = null!;
    public Actor(Point position, float speed)
    {
        Position = position;
        DrawPosition = Position.ToVector2() * Map.TileSize;
        this.speed = speed;
    }
    public static AnimationSet CreateAnimations(int character)
    {
        switch (character)
        {
            case 0:
            case 1:
            case 2:
                return new(
                    TextureManager.Get($"characters/PlayerCharacter{character}"),
                    128, 128, 
                    AnimSequenceType.Idle,
                    (AnimSequenceType.Pose, new Point(0,128),
                        4, 8, true),
                    (AnimSequenceType.Idle, new Point(0, 256),
                        4, 8, true),
                    (AnimSequenceType.IdleLeft, new Point(0, 384),
                        4, 8, true),
                    (AnimSequenceType.IdleRight, new Point(0, 512),
                        4, 8, true),
                    (AnimSequenceType.IdleUp, new Point(0, 640),
                        4, 8, true),
                    (AnimSequenceType.MoveDown, new Point(0, 768),
                        4, 8, true),
                    (AnimSequenceType.MoveLeft, new Point(0, 896),
                        4, 8, true),
                    (AnimSequenceType.MoveRight, new Point(0, 1024),
                        4, 8, true),
                    (AnimSequenceType.MoveUp, new Point(0, 1152),
                        4, 8, true),
                    (AnimSequenceType.Attack, new Point(0, 1280),
                        4, 24, true),
                    (AnimSequenceType.AttackLeft, new Point(0, 1408),
                        4, 24, true),
                    (AnimSequenceType.AttackRight, new Point(0, 1536),
                        4, 24, true),
                    (AnimSequenceType.AttackUp, new Point(0, 1664),
                        4, 24, true)
                );
            case 3:
                return new(
                    TextureManager.Get("characters/npc3"),
                    128, 128, AnimSequenceType.Idle,
                    (AnimSequenceType.Idle, new(0,0),
                    20, 12, true));
            case 4:
                return new(
                    TextureManager.Get("characters/npc4"),
                    128, 128, AnimSequenceType.Idle,
                    (AnimSequenceType.Idle, new(0,0),
                    19, 12, true));
            case 5:
                return new(
                    TextureManager.Get("characters/npc5"),
                    96, 96, AnimSequenceType.Idle,
                    (AnimSequenceType.Idle, new(0,0),
                    4, 8, true));
            default:
                throw new ArgumentOutOfRangeException($"Animation set: {character}");
        }
    }
    public MoveResult Move(Point direction, Map map, Func<Point, bool>? isOccupied = null)
    {
        if(IsMoving) return MoveResult.Blocked;
        facing = direction;
        Point target = Position + direction;
        if(!map.IsInside(target))
            return MoveResult.MapExit;
        if(map.IsSolid(target))
        {
            if(solidBumpSound <= 0)
            {
                solidBumpSound = 0.5f;
                AudioManager.Play("hitSolid");
            }
            return MoveResult.HitSolid;
        }
        if(isOccupied?.Invoke(target) == true)
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
    public void Animate(GameTime gameTime)
    {
        animator?.Play(AnimationSelection());
        animator?.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
    }
    public virtual void Update(GameTime gameTime)
    {
        Animate(gameTime);
        if(solidBumpSound > 0)
            solidBumpSound -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        if(!IsMoving) return;

        Vector2 target = Position.ToVector2() * Map.TileSize;
        float step = speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        if(Vector2.Distance(DrawPosition, target) <= step) // Snap in place if close enough
        { 
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
        animator.Draw(spriteBatch, DrawPosition + TileAnchor, color);
    }
}