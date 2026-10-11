using System;
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
        Draw(spriteBatch);
    }
    public override void Draw(SpriteBatch spriteBatch)
    {
        
    }
}