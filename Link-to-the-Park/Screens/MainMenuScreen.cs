using System;
using LinkToThePark.Graphics;
using LinkToThePark.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.Screens;
public class MainMenuScreen : BaseScreen
{
    public MainMenuScreen(Action<GameState> requestGameState) : base(requestGameState)
    {
    }
    public override void Enter()
    {
        
    }

    public override void Exit()
    {
        
    }

    public override void Update(GameTime gameTime, PlayerInputs inputs)
    {
        if(inputs.IsActionPressed(InputAction.Confirm))
            requestGameState(GameState.Playing);
    }
    
    public override void Draw(SpriteBatch spriteBatch, bool viewDebug)
    {
        
        string winText = "They barred the castle... You need an offering to get the key.";
        Vector2 size = TextureManager.Font.MeasureString(winText);

        spriteBatch.Draw(TextureManager.Get("UI/mainMenuBg"), 
            new Rectangle(0, 0, 640, 480), Color.White);
        spriteBatch.DrawString(TextureManager.Font, winText, 
            new Vector2(320, 240) - size / 2, Color.White);
    }
}