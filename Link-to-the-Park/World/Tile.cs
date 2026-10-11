using LinkToThePark.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.World;
public class Tile
{
    public Texture2D Texture { get; private set; }
    public Rectangle Source { get; private set; }
    public SpriteEffects Effects { get; private set; }
    public bool IsSolid { get; private set; } = false;
    public bool IsBridge { get; private set; } = false;
    public Tile(
        string textureName, 
        Rectangle source, 
        SpriteEffects effects = SpriteEffects.None,
        bool isSolid = false,
        bool isBridge = false)
    {
        Texture = TextureManager.Get(textureName);
        Source = source;
        Effects = effects;
        IsSolid = isSolid;
        IsBridge = isBridge;
    }
}