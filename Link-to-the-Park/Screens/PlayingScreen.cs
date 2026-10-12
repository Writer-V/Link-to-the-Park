using System;
using System.Collections.Generic;
using LinkToThePark.Actors;
using LinkToThePark.Input;
using LinkToThePark.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.Screens;
public class PlayingScreen : BaseScreen
{
    private readonly string?[,] worldMap = {  // Goes column by column.
        { "NW", null }, { "N", "S" }, {"NE", "SE"}};
    private Point currentArea = new(2,0);
    private readonly Dictionary<string, Map> maps = new();
    private Map currentMap = null!;
    private Player player = null!;
    public PlayingScreen(Action<GameState> requestGameState) : base(requestGameState)
    {
    }
    public override void Enter()
    {
        SetCurrentMap(currentArea);
        player = new(new Point(10,12));
    }

    public override void Exit()
    {
    }
    private void SetCurrentMap(Point area)
    {
        string? areaName = worldMap[area.X, area.Y];
        if(areaName == null) throw new InvalidOperationException("Attempted to load null map file.");
        Map? returnMap;
        if(maps.TryGetValue(areaName, out returnMap)) 
            currentMap = returnMap;
        else
        {
            returnMap = new($"maps/{areaName}.tmj");
            maps.Add(areaName, returnMap);
            currentMap = returnMap;
        }
    }
    private bool TryChangeArea(Point direction)
    {
        // TODO: Handle character reaching edges.
        Point target = currentArea + direction;
        if(
            target.X < 0 || 
            target.X > (worldMap.GetLength(0) - 1) || 
            target.Y < 0 ||
            target.Y > (worldMap.GetLength(1) - 1))
            return false;
        if(worldMap[target.X,target.Y] == null) 
            return false;
        SetCurrentMap(target);
        currentArea = target;
        return true;
    }

    public override void Update(GameTime gameTime, PlayerInputs inputs)
    {
        //One Direction. Not a band, but a lack of diagonals.
        if(inputs.IsActionPressed(InputAction.MoveUp))
            player.TryMove(new Point(0,-1), currentMap);
        else if(inputs.IsActionPressed(InputAction.MoveDown))
            player.TryMove(new Point(0,1), currentMap);
        else if(inputs.IsActionPressed(InputAction.MoveLeft))
            player.TryMove(new Point(-1,0), currentMap);
        else if(inputs.IsActionPressed(InputAction.MoveRight))
            player.TryMove(new Point(1, 0), currentMap);
    }
    
    public override void Draw(SpriteBatch spriteBatch, bool viewDebug)
    {
        currentMap.DrawBelow(spriteBatch);
        player.Draw(spriteBatch);
        currentMap.DrawAbove(spriteBatch);
        if(viewDebug) currentMap.DrawDebug(spriteBatch);
    }
}