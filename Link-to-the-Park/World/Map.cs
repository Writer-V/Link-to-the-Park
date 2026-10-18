using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using LinkToThePark.Graphics;
using LinkToThePark.World.Tiled;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.World;
public class Map
{
    public List<Tile?[,]> LayersAbove { get; private set; } = new();
    public List<Tile?[,]> LayersBelow { get; private set; } = new();
    public List<Treasure> Treasures {get;} = new();
    public List<Patrol> Patrols {get;} = new();
    private bool[,] solidMap = null!;
    public const int TileSize = 32;
    public int Width => solidMap.GetLength(0);
    public int Height => solidMap.GetLength(1);
    private Texture2D chestTexture;
    private Texture2D vaseTexture;
    public bool IsSolid(Point position) => 
        solidMap[position.X,position.Y] ||
        Treasures.Any(t => t.Tile == position && t.BlocksMovement);
    public Map(string filepath)
    {
        TiledMap tiledMap = TiledLoader.Load(filepath);
        bool passedPlayerLayer = false;
        Dictionary<int, List<(int Order, Point Tile, EnemyType Type)>> waypointsById = new();
        solidMap = new bool[tiledMap.Width, tiledMap.Height];

        for (int i = 0; i < tiledMap.Layers.Count; i++)
        {
            if (tiledMap.Layers[i].Type == "objectgroup")
            {
                passedPlayerLayer = true;
                if(tiledMap.Layers[i].Objects != null)
                {
                    foreach (TiledObject tiledObject in tiledMap.Layers[i].Objects!)
                    {
                        if(tiledObject.Type == "Treasure")
                        {
                            Treasures.Add(new(
                                Enum.Parse<TreasureType>(
                                    tiledObject.GetProperty("Type")?.GetString() ?? "Chest"),
                                new((int)(tiledObject.X / TileSize), (int)(tiledObject.Y / TileSize)),
                                tiledObject.GetProperty("PointValue")?.GetInt32() ?? 0));
                        }
                        else if (tiledObject.Type == "EnemyLocation")
                        {
                            Point tile = new((int)(tiledObject.X / TileSize), (int)(tiledObject.Y / TileSize));
                            foreach (JsonElement entry in tiledObject.GetProperty("Enemies")!.Value.EnumerateArray())
                            {
                                JsonElement fields = entry.GetProperty("value");
                                int id = fields.TryGetProperty("EnemyId", out JsonElement idElement) 
                                    ? idElement.GetInt32() : 0;
                                int order = fields.TryGetProperty("Order", out JsonElement orderElement) 
                                    ? orderElement.GetInt32() : 0;
                                EnemyType type = fields.TryGetProperty("Type", out JsonElement typeElement) 
                                    ? Enum.Parse<EnemyType>(typeElement.ToString()) : EnemyType.Skeleton;
                                if(waypointsById.ContainsKey(id))
                                    waypointsById[id].Add((order, tile, type));
                                else 
                                    waypointsById.Add(id, new List<(int Order, Point Tile, EnemyType Type)> {(order, tile, type)});
                            }
                        }
                    }
                }
                continue;
            }

            if (passedPlayerLayer)
                LayersAbove.Add(TiledLoader.LoadLayer(tiledMap.Layers[i], tiledMap));
            else
                LayersBelow.Add(TiledLoader.LoadLayer(tiledMap.Layers[i], tiledMap));
        }
        // Create patrols from the temp dictinary
        foreach(var patrolEntry in waypointsById)
        {
            patrolEntry.Value.Sort((a, b) => a.Order.CompareTo(b.Order));
            Patrols.Add(new(
                patrolEntry.Key, 
                patrolEntry.Value[0].Type, 
                patrolEntry.Value.Select(wp => wp.Tile).ToList()));
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

        // Load treasure texturse
        chestTexture = TextureManager.Get("items/Chest");
        vaseTexture = TextureManager.Get("items/BreakablePots");
    }
    public bool IsInside(Point tile)
    {
        return (
            tile.X >= 0 && 
            tile.X < solidMap.GetLength(0) && 
            tile.Y >= 0 &&
            tile.Y < solidMap.GetLength(1));
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
    public void DrawDebug(SpriteBatch spriteBatch)
    {
        for(int x = 0; x < solidMap.GetLength(0); x++)
        {
            for(int y = 0; y < solidMap.GetLength(1); y++)
            {
                if(solidMap[x,y])
                    spriteBatch.Draw(
                        TextureManager.Pixel, new Rectangle(x * TileSize, y * TileSize, TileSize, TileSize),
                        Color.Red * 0.4f);
            }
        }
    }
    public void DrawTreasures(SpriteBatch spriteBatch)
    {
        foreach (var treasure in Treasures)
        {
            (Texture2D? texture, int sourceX) = treasure.Type switch
            {
                TreasureType.Chest         => (chestTexture, treasure.Opened ? 64 : 0),
                TreasureType.BreakableVase => (vaseTexture,  treasure.Opened ? 96 : 0),
                _ => (null, 0)
            };
            if (texture == null) continue; // TODO: Other stuff
            spriteBatch.Draw(texture,
                new Rectangle(treasure.Tile * new Point(TileSize), new Point(TileSize)),
                new Rectangle(sourceX, 0, TileSize, TileSize), Color.White);
        }
    }
}