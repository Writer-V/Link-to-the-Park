using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace LinkToThePark.World;
public class Patrol
{
    public int Id {get;}
    public EnemyType Type {get;}
    public List<Point> Waypoints {get;}
    public Patrol(int id, EnemyType type, List<Point> waypoints)
    {
        Id = id;
        Type = type;
        Waypoints = waypoints;
    }
}