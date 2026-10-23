using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.Graphics;
public class AnimationSet
{
    public Texture2D Texture {get;}
    private readonly Dictionary<AnimSequenceType,AnimationClip> clips = new();
    public bool HasAnimType(AnimSequenceType type) => clips.ContainsKey(type);
    public AnimSequenceType DefaultType {get;}
    public readonly int FrameWidth;
    public int FrameHeight {get;}
    public AnimationSet(
        Texture2D texture, 
        int frameWidth, int frameHeight, 
        AnimSequenceType defaultType, 
        params (
            AnimSequenceType type, 
            Point startPos, 
            int frameCount, 
            int fps, 
            bool loop)[] rows)
    {
        Texture = texture;
        FrameWidth = frameWidth;
        FrameHeight = frameHeight;
        DefaultType = defaultType;
        for(int i = 0; i < rows.Length; i++)
        {
            clips.Add(
                rows[i].type, 
                new AnimationClip(
                    texture,
                    rows[i].frameCount,
                    frameWidth, frameHeight,
                    rows[i].startPos,
                    rows[i].loop,
                    rows[i].fps));
        }
        //Using with to reflect existing ones to create bonus sequences.
        if (clips.ContainsKey(AnimSequenceType.MoveLeft) && !clips.ContainsKey(AnimSequenceType.MoveRight))
            clips.Add(AnimSequenceType.MoveRight, clips[AnimSequenceType.MoveLeft] with
                {ApplyEffect = clips[AnimSequenceType.MoveLeft].ApplyEffect ^ SpriteEffects.FlipHorizontally});
        else if (!clips.ContainsKey(AnimSequenceType.MoveLeft) && clips.ContainsKey(AnimSequenceType.MoveRight))
            clips.Add(AnimSequenceType.MoveLeft, clips[AnimSequenceType.MoveRight] with 
                {ApplyEffect = clips[AnimSequenceType.MoveRight].ApplyEffect ^ SpriteEffects.FlipHorizontally});
        if(!clips.ContainsKey(defaultType)) throw new ArgumentException("AnimationSet doesn't include default sequence");
    }
    public AnimationClip BestClipFor(AnimSequenceType type)
    {
        AnimationClip clipToReturn;

        do // Sometimes a specific fallback makes sense
        {
            if(clips.TryGetValue(type,out clipToReturn!)) return clipToReturn; //The easy way...
            // or... Starting by collapsing directions if needed.
            else if(type is AnimSequenceType.IdleLeft or AnimSequenceType.IdleRight or AnimSequenceType.IdleUp)
                type = AnimSequenceType.Idle;
            else if(type is AnimSequenceType.MoveLeft or AnimSequenceType.MoveRight or AnimSequenceType.MoveUp)
                type = AnimSequenceType.MoveDown;
            else if(type == AnimSequenceType.AttackLeft)
            {
                if(clips.TryGetValue(AnimSequenceType.MoveLeft, out clipToReturn!))
                    return clipToReturn; // Right direction is more important
                else type = AnimSequenceType.Attack;
            }
            else if(type == AnimSequenceType.AttackRight)
            {
                if(clips.TryGetValue(AnimSequenceType.MoveRight, out clipToReturn!))
                    return clipToReturn; // Right direction is more important
                else type = AnimSequenceType.Attack;
            }
            else if(type == AnimSequenceType.AttackUp)
            {
                if(clips.TryGetValue(AnimSequenceType.MoveUp, out clipToReturn!))
                    return clipToReturn; // Right direction is more important
                else type = AnimSequenceType.Attack;
            }
            else if(type is AnimSequenceType.PoseLeft or AnimSequenceType.PoseRight or AnimSequenceType.PoseUp)
                type = AnimSequenceType.Pose;
            else if(type is AnimSequenceType.HitLeft or AnimSequenceType.HitRight or AnimSequenceType.HitUp)
                type = AnimSequenceType.Hit;
            else if(type == AnimSequenceType.Death)
                type = AnimSequenceType.Hit; // Deaths can use Hits
            else if(type == AnimSequenceType.DeathLeft)
            {
                if(clips.TryGetValue(AnimSequenceType.HitLeft ,out clipToReturn!))
                    return clipToReturn;
                else type = AnimSequenceType.Death;
            }
            else if(type == AnimSequenceType.DeathRight)
            {
            if(clips.TryGetValue(AnimSequenceType.HitRight ,out clipToReturn!)) 
                return clipToReturn;
            else type = AnimSequenceType.Death;
            }
            else if(type == AnimSequenceType.DeathUp)
            {
                if(clips.TryGetValue(AnimSequenceType.HitUp ,out clipToReturn!))
                    return clipToReturn;
                else type = AnimSequenceType.Death;
            }
            else if(type == AnimSequenceType.Pose)
                type = AnimSequenceType.Idle;
            else type = DefaultType;
        } while(!clips.TryGetValue(type, out clipToReturn!));
        return clipToReturn;
    }
}