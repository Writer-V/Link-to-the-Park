using System;
using Microsoft.Xna.Framework;

namespace LinkToThePark.Screens;
public class ScreenManager
{
    private GameState? pendingGameState = null;
    private GameState gameState = GameState.MainMenu;

    public void RequestGameState(GameState newState)
    {
        if (pendingGameState != null) return;
        pendingGameState = newState;
    }

    public void ChangeGameState()
    {
        if (pendingGameState == null) return;
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
            case GameState.GameOver:
                currentScreen = new GameOverScreen(RequestGameState);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void Update(GameTime gameTime)
    {
        switch (gameState)
        {
            case GameState.MainMenu:
                break;
            case GameState.Playing:
                break;
            case GameState.GameOver:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}