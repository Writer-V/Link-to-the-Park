using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.Graphics;
public static class TextureManager
{
    private static ContentManager? content;
    private static GraphicsDevice? graphics;
    private readonly static Dictionary<string, Texture2D> textures = new();
    
    public static Texture2D Pixel { get; private set; } = null!;
    public static SpriteFont Font {get; private set;} = null!;
    public static SpriteFont FontBig {get; private set;} = null!;
    public static SpriteFont FontSmall {get; private set;} = null!;
    public static void Initialize(ContentManager contentManager, GraphicsDevice graphicsDevice)
    {
        content = contentManager;
        graphics = graphicsDevice;
        Pixel = new(graphics, 1, 1);
        Pixel.SetData([Color.White]);
        Font = content.Load<SpriteFont>("UI/PixelParchment-16");
        FontBig = content.Load<SpriteFont>("UI/PixelParchment-24");
        FontSmall = content.Load<SpriteFont>("UI/PixelParchment-14");
    }
    public static Texture2D Get(string asset)
    {
        Texture2D? texture;
        if (textures.TryGetValue(asset, out texture)) return texture;
        else
        {
            if (content == null) throw new System.InvalidOperationException("TextureManager not initialized");
            texture = content.Load<Texture2D>(asset);
            textures.Add(asset, texture);
        }
        return texture;
    }
    public static void DrawBackground(SpriteBatch spriteBatch, string background, Point windowSize)
    {
        Texture2D tex = Get(background);
        float scaleFactor = MathHelper.Max(windowSize.X/(float)tex.Width, windowSize.Y/(float)tex.Height);
        spriteBatch.Draw(tex, new Rectangle(Point.Zero, windowSize),
        new Rectangle((int)((tex.Width - (windowSize.X/scaleFactor))/2f), 0, 
            (int)(windowSize.X/scaleFactor), (int)(windowSize.Y/scaleFactor)),
        Color.White);
    }
    public static void Unload()
    {
        Pixel?.Dispose();
    }
}