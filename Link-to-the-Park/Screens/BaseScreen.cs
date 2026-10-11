using System;
using LinkToThePark.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.Screens;
public abstract class BaseScreen
{
    public abstract void Enter();
    public abstract void Exit();
    protected readonly Action<GameState> requestGameState;
    protected BaseScreen(Action<GameState> requestGameState)
    {
        this.requestGameState = requestGameState;
    }
    public abstract void Update(GameTime gameTime, PlayerInputs input);
    public abstract void Draw(SpriteBatch spriteBatch);
    public abstract void Draw(SpriteBatch spriteBatch, bool viewDebug = false);
}