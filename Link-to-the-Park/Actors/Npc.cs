using System.Collections.Generic;
using LinkToThePark.World;
using Microsoft.Xna.Framework;

namespace LinkToThePark.Actors;
public class Npc : Actor
{
    private readonly Queue<Point> steps = new();
    public bool IsDoneMoving => steps.Count == 0 && !IsMoving;
    public Npc(Point position, int character) : base(position, 96f)
    {
        animator = new(
            Player.CreateAnimations(character),
            origin: new(64,95));
    }
    public void WalkPath(params Point[] stepDirections)
    {
        foreach(var d in stepDirections) steps.Enqueue(d);
    }
    public void Update(GameTime gameTime, Map map)
    {
        if(!IsMoving && steps.Count > 0)
            Move(steps.Dequeue(), map);
        base.Update(gameTime);
    }
}