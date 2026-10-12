using System;
using LinkToThePark.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.Screens;
public class GameOverScreen : BaseScreen
{
    public GameOverScreen(Action<GameState> requestGameState) : base(requestGameState)
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
    }
    
    public override void Draw(SpriteBatch spriteBatch, bool viewDebug = false)
    {
        Draw(spriteBatch);
    }
}