using System;
using System.Collections.Generic;
using LinkToThePark.Graphics;
using LinkToThePark.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.Actors;
public class Enemy : Actor
{
    private static (int HitPoints, float Speed, int Points) StatsFor(EnemyType type) => type switch
    {
        EnemyType.Skeleton => (1, 128f, 3),
        EnemyType.Zombie => (2, 48f, 2),
        EnemyType.Ghost => (1, 92f, 1),
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
    public EnemyType Type {get;}
    public int HitPoints {get; private set;}
    public int PointValue {get;} // Variable point values are out of scope
    private readonly List<Point> waypoints;
    private int nextWaypoint;
    public bool IsDead => HitPoints <= 0;
    public Enemy(Patrol patrol) : base(patrol.Waypoints[0], StatsFor(patrol.Type).Speed)
    {
        Type = patrol.Type;
        HitPoints = StatsFor(Type).HitPoints;
        PointValue = StatsFor(Type).Points;
        waypoints = patrol.Waypoints;
        nextWaypoint = 1 % waypoints.Count;
    }
    public void Update(GameTime gameTime, Map map)
    {
        if(!IsMoving)
        {
            Point target = waypoints[nextWaypoint];
            if(Position == target)
                nextWaypoint = (nextWaypoint + 1) % waypoints.Count;
            else // A step in the right direction
                Move(new Point(
                    Math.Sign(target.X - Position.X),
                    Math.Sign(target.Y - Position.Y)), map);
        }
        base.Update(gameTime);
    }
    public void TakeHit(int damage = 1)
    {
        HitPoints -= damage;
    }
    public void Draw(SpriteBatch spriteBatch)
    {
        Color color = Type switch
        {
            EnemyType.Ghost => Color.LightBlue,
            EnemyType.Zombie => Color.Green,
            _ => Color.White
        };
        spriteBatch.Draw(TextureManager.Pixel, 
            new Rectangle(
                (int)drawPosition.X, (int)drawPosition.Y, 
                Map.TileSize, Map.TileSize),
            color);
    }
}