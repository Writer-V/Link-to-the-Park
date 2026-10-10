using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.Screens;
public abstract class BaseScreen
{
    public abstract void Enter();
    protected abstract void Exit();
    public readonly Action<GameState> requestGameState;
    protected BaseScreen(Action<GameState> requestGameState)
    {
        this.requestGameState = requestGameState;
    }
    public abstract void Update(GameTime gameTime);
    public abstract void Draw(SpriteBatch spriteBatch);
}