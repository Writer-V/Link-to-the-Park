
using System;
using System.Collections.Generic;
using LinkToThePark.Graphics;
using LinkToThePark.World;
using Microsoft.Xna.Framework;

namespace LinkToThePark.Actors;
public class Npc : Actor
{
    public int NpcId {get;}
    private readonly Queue<Point> steps = new();
    public bool IsDoneMoving => steps.Count == 0 && !IsMoving;
    public Npc(Point position, int character) : base(position, 96f)
    {
        if(character == 0)
            NpcId = 3 - GameSession.Character;
        else NpcId = character;
        
        (float scale, Vector2 origin) = NpcId switch
        {
            1 => (1f,    new Vector2(64, 95)),
            2 => (1f,    new Vector2(64, 95)),
            3 => (0.5f, new Vector2(64,121)),
            4 => (0.5f, new Vector2(64, 121)),
            5 => (1f,    new Vector2(55,57)),
            _ => throw new ArgumentOutOfRangeException()
        };

        animator = new(
            CreateAnimations(NpcId), 
            initialAnimType: AnimSequenceType.Idle, 
            scale: scale, origin: origin);
        Name = DialogBook.GetName(NpcId);
    }
    public void WalkPath(params Point[] stepDirections)
    {
        foreach(var d in stepDirections) steps.Enqueue(d);
    }
    public string GetDialog(int interaction)
    {
        string line = DialogBook.GetLine(NpcId, interaction);
        return line == "" ? "" : Name + ": " + line;
    }
    public void Update(GameTime gameTime, Map map)
    {
        if(!IsMoving && steps.Count > 0)
            Move(steps.Dequeue(), map);
        base.Update(gameTime);
    }
}