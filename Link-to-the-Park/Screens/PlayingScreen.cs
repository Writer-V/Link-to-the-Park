using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LinkToThePark.Actors;
using LinkToThePark.Graphics;
using LinkToThePark.Input;
using LinkToThePark.UI;
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
    // NPC id index for NPC and visibility
    private Dictionary<int, (Npc npc, bool visible)> npcs = new();
    private int score = 0;
    private Hud hud = new();
    private MessageBox messageBox = new();
    private bool StoryMoment => player.Progression >= StoryProgressState.GateOpens;
    private float dramaTimer; // When something needs to hold tension
    private bool bridgeUnlocked;
    public PlayingScreen(Action<GameState> requestGameState) : base(requestGameState)
    {
    }
    public override void Enter()
    {
        player = new(new Point(10,12), GameSession.Character);
        SetCurrentMap(worldMap[currentArea.X, currentArea.Y]);
        messageBox.Show("You: Locked out... The key is south... but I need an offering to the altar.", player.ProgressStory);
    }

    public override void Exit()
    {
    }
    private void SetCurrentMap(string? areaName)
    {
        if(areaName == null) throw new InvalidOperationException("Attempted to load null map file.");
        Map? returnMap;
        if(maps.TryGetValue(areaName, out returnMap)) 
            currentMap = returnMap;
        else
        {
            returnMap = new(areaName);
            maps.Add(areaName, returnMap);
            currentMap = returnMap;
        }
        enemies = currentMap.Patrols.Select(p => new Enemy(p)).ToList();
        foreach(var spawn in currentMap.NpcSpawns)
        {
            if(spawn.Id == 0) // Other character at the end
                npcs[spawn.Id] = (new Npc(spawn.Tile, 3 - player.Character), false);
        }
    }
    private bool TryChangeArea(Point direction)
    {
        if(currentMap.Name != worldMap[currentArea.X, currentArea.Y])
            return false;
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
            SetCurrentMap(worldMap[target.X, target.Y]);
            currentArea = target;
            canChange = true;
        }
        return canChange;
    }

    public override void Update(GameTime gameTime, PlayerInputs inputs)
    {
        #if DEBUG
        if(inputs.IsActionPressed(InputAction.CheatSkip))
            player.ProgressStory();
        #endif
        currentMap.Update(gameTime);
        if(messageBox.IsOpen)
        {
            player.Animate(gameTime);
            foreach(var enemy in enemies)
                enemy.Animate(gameTime);
            foreach(var npc in npcs)
                if(npc.Value.visible) npc.Value.npc.Animate(gameTime);
            messageBox.Update(inputs);
            return;
        }
        StoryMomentUpdates(gameTime);

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
        if(direction != Point.Zero && 
            dramaTimer <= 0 && 
            !StoryMoment &&
            !player.IsHoldingItem)
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

        // Player encuonters door:
        if(!player.IsMoving && currentMap.Doors.TryGetValue(player.Position, out string? target))
        {
            Point faceDirection = player.FacingTile - player.Position;
            string from = currentMap.Name;
            SetCurrentMap(target);
            Point otherSide = currentMap.Doors.First(d => d.Value == from).Key;
            player.PlaceAt(otherSide + faceDirection);
        }
        
        //Player hits something
        if(inputs.IsActionPressed(InputAction.Attack) &&
            !player.IsMoving &&
            !player.IsAttacking)
        {
            player.Attack();
            foreach(var enemy in enemies)
            {
                // Was an enemy hit?
                if(enemy.Position == player.FacingTile)
                {
                    enemy.TakeHit();
                    if(enemy.IsDead) score += enemy.PointValue;
                    Debug.WriteLine($"Score: {score} (added {enemy.PointValue})");
                }
            }
            foreach(var treasure in currentMap.Treasures)
            {
                // Was a treasure hit?
                if(treasure.Tile == player.FacingTile && 
                    !treasure.Opened &&
                    treasure.Type is TreasureType.Chest or 
                    TreasureType.BreakableVase)
                {
                    treasure.Open();
                    score += treasure.Points;
                    Debug.WriteLine($"Score: {score} (added {treasure.Points}) — Solidity is {treasure.BlocksMovement}");
                }
                // But, like, an important one?
                else if(treasure.Tile == player.Position &&
                    !player.IsMoving && !treasure.Opened &&
                    player.TryPickUp(treasure.Type))
                {
                    treasure.Open();
                    if(treasure.Type == TreasureType.Item_Key)
                    {
                        // Bridge really should appear one step at a time.
                        currentMap.SetLayerVisible("HiddenBridge1", true);
                        dramaTimer = 0.5f;
                    }
                    Debug.WriteLine($"Picked up {treasure.Type}");
                }
            }
            
            /* Player encounters winning castle gates
            This should be the character backing away as the
            porteculis rises and the other PC comes out, followed
            by end message to reveal the twist. */ 
            if (player.Progression == StoryProgressState.HasKey && currentMap.CastleGates.Contains(player.FacingTile))
            {
                currentMap.SetLayerVisible("Porteculis1", false);
                currentMap.SetLayerVisible("Porteculis2", true);
                player.ProgressStory();
                dramaTimer = 0.2f;
            }
            // Remove dead enemies
            enemies.RemoveAll(e => e.IsDead);
        }

        foreach (var enemy in enemies)
            enemy.Update(gameTime, currentMap);
        foreach(var npc in npcs)
            if(npc.Value.visible)
                npc.Value.npc.Update(gameTime, currentMap);
        foreach(var enemy in enemies)
            if(enemy.Position == player.Position && 
                dramaTimer <= 0 && 
                !StoryMoment &&
                !player.IsHoldingItem)
                player.TakeHit();
        if(player.CurrentLives <= 0)
            requestGameState(GameState.GameOver);
    }
    private void StoryMomentUpdates(GameTime gameTime)
    {
        if(dramaTimer > 0) // Every tension must end...
            dramaTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        // Oh, and also every story.
        if(dramaTimer <= 0 && player.Progression == StoryProgressState.GateOpens)
        {
            player.ProgressStory();
            player.Move(new Point(0,1), currentMap);
            dramaTimer = 0.4f;
        }
        else if(dramaTimer <= 0 && player.Progression == StoryProgressState.BackAway)
        {
            npcs[0] = (npcs[0].npc, true);
            npcs[0].npc.WalkPath(new(0,1), new(0,1), new(0,1), new(0,1));
            if(player.Position == new Point(15,10)) //Player might be in the way.
                player.Move(new Point(-1,0), currentMap);
            player.ProgressStory();
        }
        else if(player.Progression == StoryProgressState.OtherWalksOut &&
            npcs[0].npc.IsDoneMoving)
            {
                player.Face(new(1,0));
                npcs[0].npc.Face(new(-1,0));
                messageBox.Show("You: Finally. Now get out - the castle is mine!", player.ProgressStory);
            }
        else if(player.Progression == StoryProgressState.CharMessage)
            messageBox.Show("Them: You suck... (I'll have to get an offering for my own key...)", () => requestGameState(GameState.Victory));
        // But so, must bridges... come..back..?
        else if(dramaTimer <= 0 && !bridgeUnlocked &&
            player.Progression == StoryProgressState.HasKey)
        {
            currentMap.SetLayerVisible("HiddenBridge2", true);
            bridgeUnlocked = true;
        }
    }
    public override void Draw(SpriteBatch spriteBatch, bool viewDebug)
    {
        currentMap.DrawBelow(spriteBatch);
        currentMap.DrawTreasures(spriteBatch);
        foreach (var enemy in enemies)
            enemy.Draw(spriteBatch, player.DrawPosition);
        foreach(var npc in npcs)
            if(npc.Value.visible) npc.Value.npc.Draw(spriteBatch);
        player.Draw(spriteBatch);

        if(player.IsHoldingItem && player.Progression == StoryProgressState.HasOffering)
            spriteBatch.Draw(
                TextureManager.Get("items/Offering"),
                new Rectangle(player.Position.X * Map.TileSize,
                    (player.Position.Y - 1) * Map.TileSize - 16, 23, 23),
                Color.White);
        if(player.IsHoldingItem && player.Progression == StoryProgressState.HasKey)
            spriteBatch.Draw(
                TextureManager.Get("items/FinalKey"),
                new Rectangle(player.Position.X * Map.TileSize,
                    (player.Position.Y - 1) * Map.TileSize - 16, 23, 23),
                new Rectangle(9, 0, 32, 32),
                Color.White);
        
        currentMap.DrawAbove(spriteBatch);
        if(viewDebug) currentMap.DrawDebug(spriteBatch);
        hud.Draw(spriteBatch, player, score);
        messageBox.Draw(spriteBatch);
    }
}