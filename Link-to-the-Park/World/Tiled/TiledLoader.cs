using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.World.Tiled;
public static class TiledLoader
{
    public static TiledMap Load(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        return JsonSerializer.Deserialize<TiledMap>(json, options)!;
    }
    private static Tile? CreateTile(uint raw, TiledMap map)
    {
        int gid =(int)(raw & 0x0FFFFFFF);
        if (gid == 0) return null;

        TiledTileset? tileset = null;
        string textureName ="";
        Rectangle source = new();
        SpriteEffects effects = SpriteEffects.None;

        for (int i = map.Tilesets.Count - 1; i >= 0; i--)
        {
            if (map.Tilesets[i].FirstGid <= gid)
            {
                tileset = map.Tilesets[i];
                break;
            }
        }
        int localId = gid - tileset!.FirstGid;
        
        if((raw & 0x80000000) != 0) effects |= SpriteEffects.FlipHorizontally;
        if((raw & 0x40000000) != 0) effects |= SpriteEffects.FlipVertically;

        if (tileset.Image != null)
        {
            textureName = "tilesets/" + Path.GetFileNameWithoutExtension(tileset.Image);
            TiledTile? data = GetTileById(tileset, localId);
            Rectangle[]? frames = data?.Animation?.Select(f => SourceById(f.TileId, tileset)).ToArray();
            float duration = (data?.Animation?[0].Duration ?? 0) / 1000f;
            return new Tile(
                textureName, 
                SourceById(localId, tileset), 
                effects,
                frames: frames,
                frameDuration: duration,
                isSolid: HasTrueProperty(GetTileById(tileset, localId), "solid"),
                isBridge: HasTrueProperty(GetTileById(tileset, localId), "bridge"));
        }
        else
        {
            var tile = GetTileById(tileset, localId);
            if(tile == null) throw new InvalidDataException("No object texture file: " + textureName);
            textureName = "tilesets/objects_separately/" + Path.GetFileNameWithoutExtension(tile.Image);
            source = new Rectangle(0, 0, tile.ImageWidth, tile.ImageHeight);
            return new Tile(
                textureName, 
                source, 
                effects, // TODO: The anchor tile might not be what syhould be solid.
                isSolid: HasTrueProperty(tile, "solid"),
                isBridge: HasTrueProperty(tile, "bridge"));
        }
    }
    public static Tile?[,] LoadLayer(TiledLayer layer, TiledMap map)
    {
        // Not handling object layers yet, so throw a null exception.
        if (layer.Data == null) throw new ArgumentException("Not a tile layer", nameof(layer));
        var layerTileGrid = new Tile?[map.Width, map.Height];  
        for (int i = 0; i < layer.Data.Length; i++)
        {
            layerTileGrid[i % map.Width, i / map.Width] = CreateTile(layer.Data[i], map);
        }
        return layerTileGrid;
    }
    private static TiledTile? GetTileById(TiledTileset tileset, int localId)
    {
        foreach (var tile in tileset.Tiles)
        {
            if (tile.Id == localId)
                return tile;
        }
        return null;
    }
    private static Rectangle SourceById(int id, TiledTileset tileset)
    {
        int column = id % tileset.Columns;
        int row = id / tileset.Columns;
        return new Rectangle(
            column * tileset.TileWidth, 
            row * tileset.TileHeight, 
            tileset.TileWidth, 
            tileset.TileHeight);
    }
    public static bool HasTrueProperty(TiledTile? tile, string propertyName)
    {
        if (tile == null) return false;
        foreach (var property in tile.Properties)
        {
            if (property.Name == propertyName && property.Value.ValueKind == JsonValueKind.True)
                return true;
        }
        return false;
    }
}