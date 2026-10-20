using System;
using System.Diagnostics;
using LinkToThePark.Graphics;
using LinkToThePark.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.Actors;
public class Player : Actor
{
    public int CurrentLives {get; private set;} = 3;
    public const int MaxLives = 3; // Might change.
    public StoryProgressState Progression {get; private set;}
    private float invulnerableTimer = 0;
    private bool IsInvulnerable => invulnerableTimer > 0;
    private const float invulnerableBase = 1.5f; // set to...
    private float attackTimer = 0;
    public bool IsAttacking => attackTimer >= 0;
    private const float attackTimerBase = 0.15f; // set to..
    public Point FacingTile => Position + facing;
    public Player(Point position) : base(position, 128f)
    {
        texture = TextureManager.Get("characters/PlayerCharacter1");
        facing = new(0,1);
        Progression = StoryProgressState.LockedOut;
    }
    public void Attack()
    {
        attackTimer = attackTimerBase;
    }
    public void TakeHit(int damage = 1) // Prepared for something dealing more damage
    {
        if(IsInvulnerable) return;
        CurrentLives -= damage;
        invulnerableTimer = invulnerableBase;
        Debug.WriteLine($"Lives: {CurrentLives} / {MaxLives}");
    }
    public bool TryPickUp(TreasureType item)
    {
        if(item == TreasureType.Chest || item == TreasureType.BreakableVase)
            return true;
        if(Progression == StoryProgressState.LockedOut &&
            item == TreasureType.Item_Offering)
        {
            Progression = StoryProgressState.HasOffering;
            return true;
        }
        else if(Progression == StoryProgressState.HasOffering &&
            item == TreasureType.Item_Key)
        {
            Progression = StoryProgressState.HasKey;
            return true;
        }
        else return false;
    }
    public void EnterCastle()
    {
        Progression = StoryProgressState.EnteredCastle;
    }
    public override void Update(GameTime gameTime)
    {
        if(IsInvulnerable)
            invulnerableTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        if(IsAttacking)
            attackTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        base.Update(gameTime);
    }
    public void Draw(SpriteBatch spriteBatch)
    {
        Color color;
        if (IsInvulnerable && (int)(invulnerableTimer * 5) % 2 == 0)
            color = Color.Red;
        else color = Color.White;
        // TODO: Replace with proper anim and soure selection.
        spriteBatch.Draw(
            texture,
            DrawPosition + new Vector2(16, 31),
            new Rectangle(0, 0, 128, 128), color, 0f,
            new Vector2(64, 95), // origin: Feet
            1f, SpriteEffects.None, 0f);
        if(IsAttacking)
            spriteBatch.Draw(
                TextureManager.Pixel, 
                new Rectangle(
                    FacingTile * Map.TileSize, 
                    new Point(Map.TileSize,Map.TileSize)),
                Color.Yellow * 0.7f);
    }
}