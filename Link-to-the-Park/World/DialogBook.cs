using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using LinkToThePark.World.Dialog;

namespace LinkToThePark.World;
public static class DialogBook
{
    private static readonly Dictionary<int, DialogEntry> dialog = new();
    public static void Initialize()
    {
        Dictionary<int, DialogEntry> tempBook = JsonSerializer.Deserialize<Dictionary<int, DialogEntry>>(
            File.ReadAllText("maps/dialog.json"), 
            new JsonSerializerOptions { 
                PropertyNameCaseInsensitive = true
                }
            )!;
        foreach(var entry in tempBook)
            dialog[entry.Key] = entry.Value;
    }
    public static string GetName(int npcId)
    {
        DialogEntry entry;
        if(dialog.TryGetValue(npcId, out entry!))
            return entry.Name;
        else return "";
    }
    public static string GetSpriteName(int npcId)
    {
        DialogEntry entry;
        if(dialog.TryGetValue(npcId, out entry!))
            return entry.Sprite;
        else return "";
    }
    public static string GetLine(int npcId, int step = 0)
    {
        DialogEntry dialogEntry;
        if(dialog.TryGetValue(npcId, out dialogEntry!))
        {
            List<string> lines = dialogEntry.Steps[Math.Min(step, dialogEntry.Steps.Count - 1)];
            return lines[Random.Shared.Next(lines.Count)];
        }
        else return "";
    }
}