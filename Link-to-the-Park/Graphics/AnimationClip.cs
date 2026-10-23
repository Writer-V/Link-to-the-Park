using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.Graphics;
public record class AnimationClip
{
    // The animation texture.
    public Texture2D Texture { get; init; }

    // Time to display each frame
    public float TimePerFrame => 1f / framesPerSec;

    // The frame width, height, and number of them.
    public int Frames { get; init; }
    public int FrameWidth { get; init; }
    public int FrameHeight { get; init; }
    private Point startPos { get; init;}
    private int framesPerSec { get; init; }
    public bool Loop { get; init; }
    public SpriteEffects ApplyEffect {get; init;} = SpriteEffects.None;

    public AnimationClip(
        Texture2D texture, 
        int frames, 
        int frameWidth, 
        int frameHeight, 
        Point startPos, 
        bool loop = true,
        int framesPerSec = 12, 
        bool flipX = false, 
        bool flipY = false)
    {
        Texture = texture; //For others to reference, if nothing else.
        Frames = frames;
        this.startPos = startPos;
        Loop = loop;
        this.framesPerSec = framesPerSec;
        if(flipX) ApplyEffect |= SpriteEffects.FlipHorizontally;
        if(flipY) ApplyEffect |= SpriteEffects.FlipVertically;
        FrameWidth = frameWidth;
        FrameHeight = frameHeight;

    }
    public Rectangle FrameRectangle(int frameNumber)
    {
        int columns = Texture.Width / FrameWidth;
        return new Rectangle(
            startPos.X + frameNumber % columns * FrameWidth, 
            startPos.Y + frameNumber / columns * FrameHeight, 
            FrameWidth, FrameHeight);
    }
}