using LinkToThePark.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.World;
public class Tile
{
    public Texture2D Texture { get; private set; }
    public Rectangle Source { get; private set; }
    public SpriteEffects Effects { get; private set; }
    public Rectangle[]? Frames {get;}
    public float FrameDuration {get;}
    public Rectangle SourceAt(float clock) =>
        Frames == null ? Source : Frames[(int)(clock / FrameDuration % Frames.Length)];
    public bool IsSolid { get; private set; } = false;
    public bool IsBridge { get; private set; } = false;
    public Tile(
        string textureName, 
        Rectangle source, 
        SpriteEffects effects = SpriteEffects.None,
        Rectangle[]? frames = null,
        float frameDuration = 0,
        bool isSolid = false,
        bool isBridge = false)
    {
        Texture = TextureManager.Get(textureName);
        Source = source;
        Effects = effects;
        Frames = frames;
        FrameDuration = frameDuration;
        IsSolid = isSolid;
        IsBridge = isBridge;
    }
}