using System;
using LinkToThePark.Audio;
using LinkToThePark.Graphics;
using LinkToThePark.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.Screens;

public class VictoryScreen : BaseScreen
{
    private Vector2 victoryTextSize;
    private string storyText = "You got the castle back!";
    private Vector2 storyTextSize;
    private Vector2 scoreTextSize;
    public VictoryScreen(Action<GameState> requestGameState) : base(requestGameState)
    {
    }
    public override void Enter()
    {
        AudioManager.Music("songEnd");
        victoryTextSize = TextureManager.FontBig.MeasureString("SUCCESS");
        storyTextSize = TextureManager.Font.MeasureString(storyText);
        scoreTextSize = TextureManager.FontBig.MeasureString($"Score: {GameSession.Score}");
    }

    public override void Exit()
    {
    }

    public override void Update(GameTime gameTime, PlayerInputs input)
    {
        if(input.IsActionPressed(InputAction.Confirm))
            requestGameState(GameState.MainMenu);
    } 
    public override void Draw(SpriteBatch spriteBatch, bool viewDebug)
    {
        TextureManager.DrawBackground(spriteBatch, "UI/mainMenuBg", new(640,480));
        spriteBatch.DrawString(TextureManager.FontBig, "SUCCESS", 
            new Vector2(320, 140) - victoryTextSize / 2, Color.White);
        spriteBatch.DrawString(TextureManager.Font, storyText,
        new Vector2(320 - storyTextSize.X / 2, 140 + TextureManager.Font.LineSpacing),
        Color.White);
        spriteBatch.DrawString(TextureManager.FontBig, $"Score: {GameSession.Score}",
            new Vector2(320, 400) - scoreTextSize, Color.Red);
    }
}
