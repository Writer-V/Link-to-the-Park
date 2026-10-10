using System;
using System.IO;
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
        string textureName;
        int column;
        int row;
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

        if (tileset.Image != null)
        {
            textureName = "tilesets/" + Path.GetFileNameWithoutExtension(tileset.Image);
            column = localId % tileset.Columns;
            row = localId / tileset.Columns;
            var source = new Rectangle(
                column * tileset.TileWidth, 
                row * tileset.TileHeight, 
                tileset.TileWidth, 
                tileset.TileHeight);
            if((raw & 0x80000000) != 0) effects |= SpriteEffects.FlipHorizontally;
            if((raw & 0x40000000) != 0) effects |= SpriteEffects.FlipVertically;
            return new Tile(textureName, source, effects);
        }
        else
        {
            foreach (var tile in tileset.Tiles)
            {
                if (tile.Id == localId)
                {
                    textureName = "tilesets/" + Path.GetFileNameWithoutExtension(tile.Image);
                    var source = new Rectangle(0, 0, tileset.TileWidth, tileset.TileHeight);
                    if((raw & 0x80000000) != 0) effects |= SpriteEffects.FlipHorizontally;
                    if((raw & 0x40000000) != 0) effects |= SpriteEffects.FlipVertically;
                    return new Tile(textureName, source, effects);
                }
            }
        }
        return null; //Shouldn't happen, but just in case.
    }
    public static Tile?[,] LoadLayer(TiledLayer layer, TiledMap map)
    {
        // Not handling object layers yet, so throw a null exception.
        if (layer.Data == null) throw new ArgumentNullException(nameof(layer));
        var layerTileGrid = new Tile?[map.Width, map.Height];  
        for (int i = 0; i < layer.Data.Length; i++)
        {
            layerTileGrid[i % map.Width, i / map.Width] = CreateTile(layer.Data[i], map);
        }
        return layerTileGrid;
    }
}