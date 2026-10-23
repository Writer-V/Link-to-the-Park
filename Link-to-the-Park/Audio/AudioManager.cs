using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace LinkToThePark.Audio;
public static class AudioManager
{
    private static ContentManager? content;
    private readonly static Dictionary<string, Song> music = new();
    private readonly static Dictionary<string, SoundEffect> soundEffects = new();
    private static string? currentSong;
    public static void Initialize(ContentManager contentManager)
    {
        content = contentManager;
        MediaPlayer.IsRepeating = true;
    }
    public static void Play(string name, float volume = 1, float pitch = 0, float pan = 0)
    {
        SoundEffect? effect;
        if (soundEffects.TryGetValue(name, out effect))
            effect.Play(volume, pitch, pan);
        else
        {
            if(content == null) throw new System.InvalidOperationException("No content manager");
            effect = content.Load<SoundEffect>($"SoundEffects/{name}");
            soundEffects[name] = effect;
            effect.Play(volume, pitch, pan);
        }
    }
    public static void Music(string name)
    {
        if(name == currentSong) return;
        Song? song;
        if (music.TryGetValue(name, out song));
        else
        {
            if(content == null) throw new System.InvalidOperationException("No content manager");
            song = content.Load<Song>($"Songs/{name}");
            music[name] = song;
        }
        MediaPlayer.Play(song);
        currentSong = name;
    }
}