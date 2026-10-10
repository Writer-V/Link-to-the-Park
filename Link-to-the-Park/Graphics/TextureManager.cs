using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.Graphics;
public static class TextureManager
{
    private static ContentManager? content;
    private readonly static Dictionary<string, Texture2D> textures = new();
    public static void Initialize(ContentManager contentManager)
    {
        content = contentManager;
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
}