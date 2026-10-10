using System.Collections.Generic;
using LinkToThePark.World.Tiled;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.World;
public class Map
{
    public List<Tile?[,]> LayersAbove { get; private set; } = new();
    public List<Tile?[,]> LayersBelow { get; private set; } = new();
    public Map(string filepath)
    {
        TiledMap tiledMap = TiledLoader.Load(filepath);
        bool passedPlayerLayer = false;

        for (int i = 0; i < tiledMap.Layers.Count; i++)
        {
            if (tiledMap.Layers[i].Type == "objectgroup")
            {
                passedPlayerLayer = true;
                continue;
            }

            if (passedPlayerLayer)
                LayersAbove.Add(TiledLoader.LoadLayer(tiledMap.Layers[i], tiledMap));
            else
                LayersBelow.Add(TiledLoader.LoadLayer(tiledMap.Layers[i], tiledMap));
        }
    }
    private void DrawLayer(SpriteBatch spriteBatch, Tile?[,] layer)
    {
        for (int x = 0; x < layer.GetLength(0); x++)
        {
            for (int y = 0; y < layer.GetLength(1); y++)
            {
                Tile? tile = layer[x, y];
                if (tile != null)
                {
                    spriteBatch.Draw(
                        tile.Texture, 
                        new Rectangle(x * 32, (y + 1) * 32 - tile.Source.Height, tile.Source.Width, tile.Source.Height), 
                        tile.Source, 
                        Color.White, 0f, Vector2.Zero, 
                        tile.Effects, 0f);
                }
            }
        }
    }
    public void DrawBelow(SpriteBatch spriteBatch)
    {
        foreach (var layer in LayersBelow)
            DrawLayer(spriteBatch, layer);
    }
    public void DrawAbove(SpriteBatch spriteBatch)
    {
        foreach (var layer in LayersAbove)
            DrawLayer(spriteBatch, layer);
    }
}