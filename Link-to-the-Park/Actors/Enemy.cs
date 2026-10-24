using System;
using System.Collections.Generic;
using LinkToThePark.Audio;
using LinkToThePark.Graphics;
using LinkToThePark.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.Actors;
public class Enemy : Actor
{
    
    public record EnemyStats(
        int HitPoints, float Speed, int Points,
        bool IsSolid, // Not a ghost
        float AttackCooldownn,
        int DetectRange, // In tiles
        int AbandonRange // Give up beyond this
    );
    private record EnemySounds(
        string Attack,      string Hurt,
        string Death,       string Detection
    );
    private static EnemyStats StatsFor(EnemyType type) => type switch
    {
        EnemyType.Skeleton  => new EnemyStats(1, 128f, 3, true, 0.8f, 4, 6),
        EnemyType.Zombie    => new EnemyStats(2,  48f, 2, true, 1.1f, 3, 5),
        EnemyType.Ghost     => new EnemyStats(1,  92f, 1, false,  1f, 0, 0),
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
    public EnemyType Type {get;}
    public EnemyStats Stats {get; private set;}
    private EnemySounds soundFiles;
    public int HitPoints {get; private set;}
    private float attackTimer;
    public bool IsAttacking => attackTimer <= 0;
    private readonly List<Point> waypoints;
    private int nextWaypoint;
    public bool IsDead => Stats.HitPoints <= 0;
    public Enemy(Patrol patrol) : base(patrol.Waypoints[0], StatsFor(patrol.Type).Speed)
    {
        Type = patrol.Type;
        Stats = StatsFor(Type);
        HitPoints = Stats.HitPoints;
        waypoints = patrol.Waypoints;
        nextWaypoint = 1 % waypoints.Count;

        (AnimationSet animations, Vector2 origin) = patrol.Type switch
        {
            EnemyType.Skeleton => (CreateAnimatios(EnemyType.Skeleton), new Vector2(32, 44)),
            EnemyType.Zombie => (CreateAnimatios(EnemyType.Zombie), new Vector2(40, 61)),
            EnemyType.Ghost => (CreateAnimatios(EnemyType.Ghost), new Vector2(16, 31)),
            _ => throw new NotImplementedException()
        };
        animator = new(animations, origin: origin);
        if(Type == EnemyType.Skeleton) animator.Scale = 1.5f;

        soundFiles = Type switch
        {
            EnemyType.Skeleton  => new EnemySounds("playerHitTarget", "", "skeletonDeath", "skeletonDetection"),
            EnemyType.Ghost     => new EnemySounds("ghostAttack", "", "ghostDeath", ""),
            EnemyType.Zombie    => new EnemySounds("zombieAttack", "zombieHurt", "zombieDeath", "zombieDetection"),
            _ => new EnemySounds("","","","")
        };
    }
    public static AnimationSet CreateAnimatios(EnemyType type)
    {
        // Skeleton, 64×64, origin (32,44)
        if(type == EnemyType.Skeleton)
            return new(TextureManager.Get("characters/skeleton"),
                64, 64,AnimSequenceType.Idle,
                (AnimSequenceType.MoveLeft,     new Point(0,   0), 8, 8, true),
                (AnimSequenceType.MoveDown,     new Point(0,  64), 8, 8, true),
                (AnimSequenceType.MoveUp,       new Point(0, 128), 8, 8, true),
                (AnimSequenceType.MoveRight,    new Point(0, 192), 8, 8, true),
                (AnimSequenceType.IdleLeft,     new Point(0, 256), 8, 12, true),
                (AnimSequenceType.Idle,         new Point(0, 320), 8, 12, true),
                (AnimSequenceType.IdleUp,       new Point(0, 384), 8, 12, true),
                (AnimSequenceType.IdleRight,    new Point(0, 448), 8, 12, true),
                (AnimSequenceType.AttackLeft,   new Point(0, 512), 6, 12, false),
                (AnimSequenceType.Attack,       new Point(0, 576), 6, 12, false),
                (AnimSequenceType.AttackUp,     new Point(0, 640), 6, 12, false),
                (AnimSequenceType.AttackRight,  new Point(0, 704), 6, 12, false));
        // Zombie, 80×80, origin (40,61): slow, so low fps
        else if(type == EnemyType.Zombie)
            return new(TextureManager.Get("characters/zombie"),
                80, 80, AnimSequenceType.Idle,
                (AnimSequenceType.Idle,         new Point(0,    0), 6, 6, true),
                (AnimSequenceType.MoveDown,     new Point(0,   80), 6, 6, true),
                (AnimSequenceType.Attack,       new Point(0,  160), 5, 6, false),
                (AnimSequenceType.Hit,          new Point(0,  240), 2, 6, false),
                (AnimSequenceType.Death,        new Point(0,  320), 6, 6, false),
                (AnimSequenceType.IdleUp,       new Point(0,  400), 6, 6, true),
                (AnimSequenceType.MoveUp,       new Point(0,  480), 6, 6, true),
                (AnimSequenceType.AttackUp,     new Point(0,  560), 5, 6, false),
                (AnimSequenceType.HitUp,        new Point(0,  640), 2, 6, false),
                (AnimSequenceType.DeathUp,      new Point(0,  720), 6, 6, false),
                (AnimSequenceType.IdleRight,    new Point(0,  800), 6, 6, true),
                (AnimSequenceType.MoveRight,    new Point(0,  880), 6, 6, true),
                (AnimSequenceType.AttackRight,  new Point(0,  960), 5, 6, false),
                (AnimSequenceType.HitRight,     new Point(0, 1040), 2, 6, false),
                (AnimSequenceType.DeathRight,   new Point(0, 1120), 6, 6, false),
                (AnimSequenceType.IdleLeft,     new Point(0, 1200), 6, 6, true),
                (AnimSequenceType.MoveLeft,     new Point(0, 1280), 6, 6, true),
                (AnimSequenceType.AttackLeft,   new Point(0, 1360), 5, 6, false),
                (AnimSequenceType.HitLeft,      new Point(0, 1440), 2, 6, false),
                (AnimSequenceType.DeathLeft,    new Point(0, 1520), 6, 6, false));
        // Ghost, 32×32, origin (16,31)
        else return new(TextureManager.Get("characters/ghost"),
            32, 32, AnimSequenceType.MoveDown,
            (AnimSequenceType.MoveDown,  new Point(0,  0), 3, 6, true),
            (AnimSequenceType.MoveLeft,  new Point(0, 32), 3, 6, true),
            (AnimSequenceType.MoveRight, new Point(0, 64), 3, 6, true),
            (AnimSequenceType.MoveUp,    new Point(0, 96), 3, 6, true));
    }
    public bool TryAttack(Point playerTile)
    {
        if(IsMoving || IsAttacking || IsDead) return false;
        if(Type == EnemyType.Ghost && Position != playerTile) return false;
        if(Math.Abs(Position.X - playerTile.X) + 
            Math.Abs(Position.Y - playerTile.Y) == 1 &&
            Type != EnemyType.Ghost) return false;
        
        Face(new Point(Position.X - playerTile.X,
            Position.Y - playerTile.Y));
        animator.PlayOnce(AttackAnimDirection(), true);
        if(soundFiles.Attack != "")
            AudioManager.Play(soundFiles.Attack);

        attackTimer = Stats.AttackCooldownn;
        return true;
    }
    public void Update(GameTime gameTime, Map map, Func<Point, bool>? isOccupied = null)
    {
        if(attackTimer > 0) attackTimer -= 
            (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        if(!IsMoving)
        {
            Point target = waypoints[nextWaypoint];
            if(Type == EnemyType.Ghost) isOccupied = null; //Ghosts ignore that.
            if(Position == target)
                nextWaypoint = (nextWaypoint + 1) % waypoints.Count;
            else // A step in the right direction
                Move(new Point(
                    Math.Sign(target.X - Position.X),
                    Math.Sign(target.Y - Position.Y)), map, isOccupied);
        }
        base.Update(gameTime);
    }
    public void TakeHit(int damage = 1)
    {
        HitPoints -= damage;
        if(Type == EnemyType.Skeleton)
            AudioManager.Play("skeletonDeath");
        else if(Type == EnemyType.Ghost)
            AudioManager.Play("ghostDeath");
        else if(Type == EnemyType.Zombie && IsDead)
            AudioManager.Play("zombieDeath");
        else AudioManager.Play("zombieHurt");
    }
    public void Draw(SpriteBatch spriteBatch, Vector2 playerPosition)
    {
        float opacity = 1f;
        if (Type == EnemyType.Ghost)
        {
            float distance = Vector2.Distance(DrawPosition, playerPosition);
            if (distance <= 2 * Map.TileSize) opacity = 0f;
            else if (distance <= 3 * Map.TileSize) opacity = 0.5f;
        }
        base.Draw(spriteBatch, Color.White * opacity);
    }
}