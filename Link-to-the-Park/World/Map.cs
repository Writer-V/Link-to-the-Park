using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using LinkToThePark.Graphics;
using LinkToThePark.World.Tiled;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.World;
public class Map
{
    public string Name {get;}
    public List<Tile?[,]> LayersAbove { get; private set; } = new();
    public List<Tile?[,]> LayersBelow { get; private set; } = new();
    private readonly Dictionary<string, Tile?[,]> layersByName = new();
    private readonly HashSet<Tile?[,]> hiddenLayers = new();
    public readonly List<Point> CastleGates = new();
    public List<Treasure> Treasures {get;} = new();
    public List<Patrol> Patrols {get;} = new();
    public List<NpcSpawn> NpcSpawns {get;} = new();
    public Dictionary<Point, string> Doors {get;} = new();
    private bool[,] solidMap = null!;
    public const int TileSize = 32;
    public int Width {get;}
    public int Height {get;}
    private float clock = 0;
    public void Update(GameTime gameTime) =>
        clock += (float)gameTime.ElapsedGameTime.TotalSeconds;
    private Texture2D chestTexture;
    private Texture2D vaseTexture;
    private Texture2D offeringTexture;
    private Texture2D keyTexture;
    public bool IsSolid(Point position) => 
        solidMap[position.X,position.Y] ||
        Treasures.Any(t => t.Tile == position && t.BlocksMovement);
    public Map(string filename)
    {
        Name = filename;
        TiledMap tiledMap = TiledLoader.Load($"maps/{filename}.tmj");
        Height = tiledMap.Height;
        Width = tiledMap.Width;
        Debug.WriteLine($"Width: {Width}, Height: {Height}");
        bool passedPlayerLayer = false;
        Dictionary<int, List<(int Order, Point Tile, EnemyType Type)>> waypointsById = new();
        solidMap = new bool[tiledMap.Width, tiledMap.Height];

        // Go through the layers..
        for (int i = 0; i < tiledMap.Layers.Count; i++)
        {
            // The object layer is weird — deal with it.
            if (tiledMap.Layers[i].Type == "objectgroup")
            {
                passedPlayerLayer = true;
                if(tiledMap.Layers[i].Objects != null)
                {
                    foreach (TiledObject tiledObject in tiledMap.Layers[i].Objects!)
                    {
                        Point tileLocation = new((int)(tiledObject.X / TileSize), (int)(tiledObject.Y / TileSize));
                        if(tiledObject.Type == "Treasure")
                        {
                            Treasures.Add(new(
                                Enum.Parse<TreasureType>(
                                    tiledObject.GetProperty("Type")?.GetString() ?? "Chest"),
                                    tileLocation, tiledObject.GetProperty("PointValue")?.GetInt32() ?? 0));
                        }
                        else if (tiledObject.Type == "EnemyLocation")
                        {
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
                                    waypointsById[id].Add((order, tileLocation, type));
                                else 
                                    waypointsById.Add(id, new List<(int Order, Point Tile, EnemyType Type)> {(order, tileLocation, type)});
                            }
                        }
                        else if(tiledObject.GetProperty("Target") is JsonElement target)
                        {
                            Doors[tileLocation] = target.GetString()!;
                        }
                        else if(tiledObject.GetProperty("CastleGate") is JsonElement gate && gate.GetBoolean())
                            CastleGates.Add(tileLocation);
                        else if(tiledObject.Type == "NpcSpawn")
                            NpcSpawns.Add(new(
                                tiledObject.GetProperty("ID")?.GetInt32() ?? 0, 
                                tileLocation));
                    }
                }
                continue;
            }

            // Add tile layers to places
            Tile?[,] layer = TiledLoader.LoadLayer(tiledMap.Layers[i], tiledMap);
            if(layer == null) continue;
            if(!tiledMap.Layers[i].Visible)
                hiddenLayers.Add(layer);
            layersByName.Add(tiledMap.Layers[i].Name, layer);
            if (passedPlayerLayer)
                LayersAbove.Add(layer);
            else
                LayersBelow.Add(layer);
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
        BuildSolidMap();

        // Load treasure texturse
        chestTexture = TextureManager.Get("items/Chest");
        vaseTexture = TextureManager.Get("items/BreakablePots");
        offeringTexture = TextureManager.Get("items/Offering");
        keyTexture = TextureManager.Get("items/FinalKey");
    }
    private void BuildSolidMap()
    {
        bool[,] isAnySolid = new bool[Width, Height]; // Bool default is false
        bool[,] isAnyBridge = new bool[Width,Height];
        foreach(var layer in layersByName)
        {
            if(hiddenLayers.Contains(layer.Value!) || layer.Value == null) continue;
            for(int x = 0; x < Width; x++)
            {
                for(int y = 0; y < Height; y++)
                {
                    if(layer.Value[x,y] != null && layer.Value[x,y]!.IsSolid)
                        isAnySolid[x,y] = true;
                    if(layer.Value[x,y] != null && layer.Value[x,y]!.IsBridge)
                        isAnyBridge[x,y] = true;
                }
            }
        }
        // And now to combine...
        for(int x = 0; x < Width; x++)
            for(int y = 0; y < Height; y++)
                solidMap[x,y] = isAnySolid[x,y] && !isAnyBridge[x,y];
    }
    public void SetLayerVisible(string name, bool visibility)
    {
        Tile?[,] layer = new Tile[Width,Height];
        if(layersByName.TryGetValue(name, out layer!))
        {
            if(visibility) hiddenLayers.Remove(layer);
            else hiddenLayers.Add(layer);
            BuildSolidMap();
        }
        else throw new KeyNotFoundException();
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
        if(hiddenLayers.Contains(layer)) return;
        for (int x = 0; x < layer.GetLength(0); x++)
        {
            for (int y = 0; y < layer.GetLength(1); y++)
            {
                Tile? tile = layer[x, y];
                if (tile != null)
                {
                    spriteBatch.Draw(
                        tile.Texture, 
                        new Rectangle(
                            x * TileSize, 
                            (y + 1) * TileSize - tile.Source.Height, 
                            tile.Source.Width, 
                            tile.Source.Height), 
                        tile.SourceAt(clock), 
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
        foreach(var door in Doors)
        {
            spriteBatch.Draw(
                TextureManager.Pixel,
                new Rectangle(door.Key * new Point(TileSize), new(TileSize)),
                Color.Blue * 0.5f);
        }
    }
    public void DrawTreasures(SpriteBatch spriteBatch)
    {
        // Compensate for the sprites being too small inside a tile
        Point compensation = new((int)(TileSize * 0.2));
        foreach (var treasure in Treasures)
        {
            if(treasure.Opened && 
                treasure.Type is TreasureType.Item_Key or 
                TreasureType.Item_Offering)
                    continue;
            
            (Texture2D? texture, int sourceX) = treasure.Type switch
            {
                TreasureType.Chest         => (chestTexture, treasure.Opened ? 64 : 0),
                TreasureType.BreakableVase => (vaseTexture,  treasure.Opened ? 96 : 0),
                TreasureType.Item_Offering => (offeringTexture, 0),
                TreasureType.Item_Key      => (keyTexture, 9),
                _ => (null, 0)
            };
            if (texture == null) continue; // TODO: Other stuff
            
            spriteBatch.Draw(texture,
                new Rectangle(
                    treasure.Tile * new Point(TileSize) - compensation, 
                    new Point(TileSize) + (compensation * 2)),
                new Rectangle(sourceX, 0, TileSize, TileSize), Color.White);
        }
    }
}