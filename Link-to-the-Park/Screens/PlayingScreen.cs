using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    private List<Enemy> enemies = new();
    private int score = 0;
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
        enemies = currentMap.Patrols.Select(p => new Enemy(p)).ToList();
    }
    private bool TryChangeArea(Point direction)
    {
        Point target = currentArea + direction;
        bool canChange;
        if(
            target.X < 0 || 
            target.X > (worldMap.GetLength(0) - 1) || 
            target.Y < 0 ||
            target.Y > (worldMap.GetLength(1) - 1))
            canChange = false;
        else if(worldMap[target.X,target.Y] == null) 
            canChange = false;
        else
        {
            canChange = true;
            SetCurrentMap(target);
            currentArea = target;
        }
        return canChange;
    }

    public override void Update(GameTime gameTime, PlayerInputs inputs)
    {
        //One Direction. Not a band, but a lack of diagonals.
        Point direction = new();
        if(inputs.IsActionHeld(InputAction.MoveUp))
            direction = new Point(0,-1);
        else if(inputs.IsActionHeld(InputAction.MoveDown))
            direction = new Point(0,1);
        else if(inputs.IsActionHeld(InputAction.MoveLeft))
            direction = new Point(-1,0);
        else if(inputs.IsActionHeld(InputAction.MoveRight))
            direction = new Point(1,0);
        if(direction != Point.Zero)
        {
            MoveResult result = player.Move(direction, currentMap);
            if(result == MoveResult.MapExit && TryChangeArea(direction))
            {
                Point p = player.Position + direction;
                // C# uses % weird. -1 % 20 = -1, not 19.
                p.X = (p.X + currentMap.Width) % currentMap.Width;
                p.Y = (p.Y + currentMap.Height) % currentMap.Height;
                player.PlaceAt(p);
            }
        }
        player.Update(gameTime);

        if(inputs.IsActionPressed(InputAction.Attack) &&
            !player.IsMoving &&
            !player.IsAttacking)
        {
            player.Attack();
            foreach(var enemy in enemies)
            {
                if(enemy.Position == player.FacingTile)
                {
                    enemy.TakeHit();
                    if(enemy.IsDead) score += enemy.PointValue;
                    Debug.WriteLine($"Score: {score} (added {enemy.PointValue})");
                }
            }
            enemies.RemoveAll(e => e.IsDead);
        }

        foreach (var enemy in enemies)
            enemy.Update(gameTime, currentMap);
        foreach(var enemy in enemies)
            if(enemy.Position == player.Position)
                player.TakeHit();
        if(player.CurrentLives <= 0)
            requestGameState(GameState.GameOver);
    }
    
    public override void Draw(SpriteBatch spriteBatch, bool viewDebug)
    {
        currentMap.DrawBelow(spriteBatch);
        foreach (var enemy in enemies)
            enemy.Draw(spriteBatch);
        player.Draw(spriteBatch);
        currentMap.DrawAbove(spriteBatch);
        if(viewDebug) currentMap.DrawDebug(spriteBatch);
    }
}