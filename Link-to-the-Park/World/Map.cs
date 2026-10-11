using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LinkToThePark.World.Tiled;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.World;
public class Map
{
    public List<Tile?[,]> LayersAbove { get; private set; } = new();
    public List<Tile?[,]> LayersBelow { get; private set; } = new();
    private bool[,] solidMap = null!;
    public const int TileSize = 32;
    public bool IsSolid(int x, int y) => solidMap[x,y];
    public Map(string filepath)
    {
        TiledMap tiledMap = TiledLoader.Load(filepath);
        bool passedPlayerLayer = false;
        solidMap = new bool[tiledMap.Width, tiledMap.Height];

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
        // Build the blocking solid areas.
        for (int x = 0; x < tiledMap.Width; x++)
        {
            for (int y = 0; y < tiledMap.Height; y++)
            {
                bool anySolid = false;
                bool anyBridge = false;
                foreach (var layer in LayersAbove.Concat(LayersBelow))
                {
                    if (layer[x,y] != null && layer[x, y]!.IsSolid)
                        anySolid = true;
                    if (layer[x,y] != null && layer[x,y]!.IsBridge)
                        anyBridge = true;
                }
                solidMap[x, y] = anySolid && !anyBridge;
            }
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
                        new Rectangle(x * TileSize, (y + 1) * TileSize - tile.Source.Height, tile.Source.Width, tile.Source.Height), 
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
    public void DrawDebug(SpriteBatch spriteBatch, Texture2D pixel)
    {
        for(int x = 0; x < solidMap.GetLength(0); x++)
        {
            for(int y = 0; y < solidMap.GetLength(1); y++)
            {
                if(solidMap[x,y])
                    spriteBatch.Draw(
                        pixel, new Rectangle(x * TileSize, y * TileSize, TileSize, TileSize),
                        Color.Red * 0.4f);
            }
        }
    }
}