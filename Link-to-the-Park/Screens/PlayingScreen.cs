using System;
using LinkToThePark.Input;
using LinkToThePark.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.Screens;
public class PlayingScreen : BaseScreen
{
    private Map currentMap = null!;
    public PlayingScreen(Action<GameState> requestGameState) : base(requestGameState)
    {
    }
    public override void Enter()
    {
        currentMap = new("maps/NE.tmj");
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
        if(viewDebug) currentMap.DrawDebug(spriteBatch);
    }
    public override void Draw(SpriteBatch spriteBatch)
    {
        currentMap.DrawBelow(spriteBatch);
        currentMap.DrawAbove(spriteBatch);
    }
}