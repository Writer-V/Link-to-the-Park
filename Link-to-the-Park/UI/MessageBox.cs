using System;
using System.Collections.Generic;
using LinkToThePark.Graphics;
using LinkToThePark.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.UI;
public class MessageBox
{
    private static readonly Rectangle frame = new(20, 380, 600, 80);
    private const int padding = 20;
    private List<string> lines = new();
    private Action? onClose;
    public bool IsOpen {get; private set;}
    public void Show(string text, Action? onClose = null)
    {
        lines = Wrap(text, frame.Width - 2*padding);
        this.onClose = onClose;
        IsOpen = true;
    }
    private static List<string> Wrap(string text, int maxWidth)
    {
        List<string> restult = new();
        string line = "";
        foreach(string word in text.Split(" "))
        {
            string attempt = line == "" ? word : line + " " + word;
            if(TextureManager.Font.MeasureString(attempt).X > maxWidth && line != "")
            {
                restult.Add(line);
                line = word;
            }
            else line = attempt;
        }
        if(line != "") restult.Add(line);
        return restult;
    }
    public void Update(PlayerInputs inputs)
    {
        if(IsOpen && inputs.IsActionPressed(InputAction.Confirm))
        {
            IsOpen = false;
            onClose?.Invoke();
        }
    }
    public void Draw(SpriteBatch spriteBatch)
    {
        if(!IsOpen) return;
        NineSlice.TextFrame(spriteBatch, frame);
        float y = frame.Y + padding;
        foreach(string line in lines)
        {
            spriteBatch.DrawString(
                TextureManager.Font, line,
                new Vector2(frame.X + padding, y),
                new Color(40,30,20));
            y += TextureManager.Font.LineSpacing;
        }
    }
}