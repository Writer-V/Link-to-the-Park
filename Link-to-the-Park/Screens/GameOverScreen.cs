using System;
using LinkToThePark.Audio;
using LinkToThePark.Graphics;
using LinkToThePark.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.Screens;
public class GameOverScreen : BaseScreen
{
    private Vector2 gameOverTextSize;
    private Vector2 scoreTextSize;
    public GameOverScreen(Action<GameState> requestGameState) : base(requestGameState)
    {
    }
    public override void Enter()
    {
        gameOverTextSize = TextureManager.FontBig.MeasureString("GAME OVER");
        scoreTextSize = TextureManager.Font.MeasureString($"Score: {GameSession.Score}");
        AudioManager.Music("songGameOver");
    }

    public override void Exit()
    {
    }

    public override void Update(GameTime gameTime, PlayerInputs inputs)
    {
        if(inputs.IsActionPressed(InputAction.Confirm))
        {
            AudioManager.Play("buttonClick");
            requestGameState(GameState.MainMenu);
        }
    }
    
    public override void Draw(SpriteBatch spriteBatch, bool viewDebug = false)
    {
        TextureManager.DrawBackground(spriteBatch, "UI/mainMenuBg", new(640, 480));
        spriteBatch.DrawString(TextureManager.Font, "GAME OVER", 
            new Vector2(320, 240) - gameOverTextSize / 2, Color.White);
        spriteBatch.DrawString(TextureManager.Font, $"Score: {GameSession.Score}",
            new Vector2(320, 300) - scoreTextSize/2, Color.Red);
    }
}