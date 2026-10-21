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
        if(clips.TryGetValue(type,out clipToReturn!)) return clipToReturn; //The easy way...
        switch (type)
        {
            //Sometimes a specific fallback makes sense
            case AnimSequenceType.MoveLeft:
                if(clips.TryGetValue(AnimSequenceType.Idle, out clipToReturn!)) return clipToReturn;
                else return clips[DefaultType];
            case AnimSequenceType.Death:
                if(clips.TryGetValue(AnimSequenceType.Hit, out clipToReturn!)) return clipToReturn;
                else return clips[DefaultType];
            default:
                return clips[DefaultType];
        }
    }
}