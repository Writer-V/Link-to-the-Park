using System;
using LinkToThePark.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.Screens;
public class ScreenManager
{
    private GameState? pendingGameState = GameState.MainMenu;
    private GameState gameState = GameState.MainMenu;
    private PlayerInputs inputs;
    private bool viewDebug = false;
    private BaseScreen? currentScreen;
    public ScreenManager(PlayerInputs inputs)
    {
        this.inputs = inputs;
    }

    public void RequestGameState(GameState newState)
    {
        if (pendingGameState != null) return;
        pendingGameState = newState;
    }

    private void ChangeGameState()
    {
        if (pendingGameState == null) return;
        currentScreen?.Exit();
        gameState = pendingGameState.Value;
        pendingGameState = null;
        switch (gameState)
        {
            case GameState.MainMenu:
                currentScreen = new MainMenuScreen(RequestGameState);
                break;
            case GameState.Playing:
                currentScreen = new PlayingScreen(RequestGameState);
                break;
            case GameState.Victory:
                currentScreen = new VictoryScreen(RequestGameState);
                break;
            case GameState.GameOver:
                currentScreen = new GameOverScreen(RequestGameState);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        currentScreen?.Enter();
    }

    public void Update(GameTime gameTime)
    {
        ChangeGameState();
        if(inputs.IsActionPressed(InputAction.Debug))
            viewDebug = !viewDebug;
        currentScreen?.Update(gameTime, inputs);
    }
    public void Draw(SpriteBatch spriteBatch)
    {
        currentScreen?.Draw(spriteBatch, viewDebug);
    }
}