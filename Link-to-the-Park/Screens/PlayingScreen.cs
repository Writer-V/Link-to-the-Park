using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LinkToThePark.Actors;
using LinkToThePark.Audio;
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
    private Dictionary<int, (Npc npc, int interactionNo, bool visible)> npcs = new();
    private int score = 0;
    private const int totalChests = 8;
    private int openedChests = 0;
    private int chestBonus = 60;
    private Hud hud = new();
    private MessageBox messageBox = new();
    private bool StoryMoment => player.Progression >= StoryProgressState.GateOpens;
    private float dramaTimer; // When something needs to hold tension
    private bool bridgeUnlocked;
    private bool IsNpcAt(Point tile) =>
        npcs.Values.Any(n => n.visible && n.npc.Position == tile);
    private bool IsOccupued(Point tile) =>
        player.Position == tile ||
        enemies.Any(e => e.Stats.IsSolid && e.Position == tile) ||
        IsNpcAt(tile);
    public PlayingScreen(Action<GameState> requestGameState) : base(requestGameState)
    {
    }
    public override void Enter()
    {
        GameSession.Score = 0;
        player = new(new Point(10,12), GameSession.Character);
        SetCurrentMap(worldMap[currentArea.X, currentArea.Y]);
        messageBox.Show("You: Locked out... The key is south... but I need an offering to the altar.", player.ProgressStory);
    }

    public override void Exit()
    {
        GameSession.Score = score;
        //Could add some sort of time bonus / penalty...
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

        //Spawn enemies on patrol
        enemies = currentMap.Patrols.Select(p => new Enemy(p)).ToList();

        //Set up relevant NPCs
        foreach (var npcId in npcs.Keys.ToList()) // Hide everyone
            npcs[npcId] = (npcs[npcId].npc, npcs[npcId].interactionNo, false);
        foreach(var spawn in currentMap.NpcSpawns)
        {
            if(!npcs.ContainsKey(spawn.Id)) // Create if they're not in there. Hide the end character.
                npcs[spawn.Id] = (new Npc(spawn.Tile, spawn.Id), 0, spawn.Id != 0);
            else if(spawn.Id != 0) // Show the relevant ones.
                npcs[spawn.Id] = (npcs[spawn.Id].npc, npcs[spawn.Id].interactionNo, true);
        }

        switch (currentMap.Name) // Set the music
        {
            case "N":
                AudioManager.Music("songAreaN-S");
                break;
            case "S":
                AudioManager.Music("songAreaN-S");
                break;
            case "NW":
                AudioManager.Music("songNW-cabin");
                break;
            case "Cabin":
                AudioManager.Music("songNW-cabin");
                break;
            default:
                AudioManager.Music("songGeneralGameplay");
                break;
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
        {
            canChange = false;
        }
        else if(worldMap[target.X,target.Y] == null) 
        {
            canChange = false;
        }
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
        if(messageBox.IsOpen) // Only animations
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
        
        // Check for all the reasons to ignore movement input
        if(direction != Point.Zero && 
            dramaTimer <= 0 && 
            !StoryMoment &&
            !player.IsHoldingItem)
        {
            MoveResult result = player.Move(direction, currentMap, IsOccupued);
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
        if(!player.IsMoving && 
            currentMap.Doors.TryGetValue(player.Position, out string? target))
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
                    if(enemy.IsDead) score += enemy.Stats.Points;
                    Debug.WriteLine($"Score: {score} (added {enemy.Stats.Points})");
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
                    if(treasure.Type == TreasureType.Chest)
                        openedChests++;
                    Debug.WriteLine($"Opened {openedChests} chests");
                    if(openedChests == totalChests)
                    {
                        score += chestBonus;
                        messageBox.Show(DialogBook.GetLine(10,0) + chestBonus.ToString());
                    }
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
                        AudioManager.Play("thunk");
                        dramaTimer = 0.5f;
                    }
                    Debug.WriteLine($"Picked up {treasure.Type}");
                }
            }
            foreach(var npc in npcs)
            {
                if(npc.Value.visible && 
                    npc.Value.npc.Position == player.FacingTile)
                {
                    int interaction = npc.Value.interactionNo;
                    string line = npc.Value.npc.GetDialog(interaction);
                    if(line != "")messageBox.Show(line);
                    npcs[npc.Key] = (npc.Value.npc, interaction += 1, npc.Value.visible);
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
                AudioManager.Play("thunk");
                player.ProgressStory();
                dramaTimer = 0.2f;
            }
            // Remove dead enemies
            enemies.RemoveAll(e => e.IsDead);
        }

        foreach (var enemy in enemies)
            enemy.Update(gameTime, currentMap, IsOccupued);
        foreach(var npc in npcs)
            if(npc.Value.visible)
                npc.Value.npc.Update(gameTime, currentMap);
        if(dramaTimer <= 0 && 
            !StoryMoment &&
            !player.IsHoldingItem)
        {
            foreach(var enemy in enemies)
                if(enemy.TryAttack(player.Position))
                    player.TakeHit();
        }
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
            npcs[0] = (npcs[0].npc, npcs[0].interactionNo, true);
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
        {
            if(npcs[0].interactionNo == 0)
            {
                messageBox.Show(npcs[0].npc.GetDialog(0));
                npcs[0] = (npcs[0].npc, 1, true);
            }
            else messageBox.Show(npcs[0].npc.GetDialog(npcs[0].interactionNo), () => requestGameState(GameState.Victory));
        }
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
                    (player.Position.Y - 1) * Map.TileSize - 16, 32, 32),
                Color.White);
        if(player.IsHoldingItem && player.Progression == StoryProgressState.HasKey)
            spriteBatch.Draw(
                TextureManager.Get("items/FinalKey"),
                new Rectangle(player.Position.X * Map.TileSize,
                    (player.Position.Y - 1) * Map.TileSize - 16, 32, 32),
                new Rectangle(9, 0, 32, 32),
                Color.White);
        
        currentMap.DrawAbove(spriteBatch);
        if(viewDebug) currentMap.DrawDebug(spriteBatch);
        hud.Draw(spriteBatch, player, score);
        messageBox.Draw(spriteBatch);
    }
}