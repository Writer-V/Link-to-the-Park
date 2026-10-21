using System;
using System.Diagnostics;
using LinkToThePark.Graphics;
using LinkToThePark.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.Actors;
public class Player : Actor
{
    public int Character {get;} // Character 1 or 2
    public int CurrentLives {get; private set;} = 3;
    public const int MaxLives = 3; // Might change.
    public StoryProgressState Progression {get; private set;}
    private float invulnerableTimer = 0;
    private bool IsInvulnerable => invulnerableTimer > 0;
    private const float invulnerableBase = 1.5f; // set to...
    private float attackTimer = 0;
    public bool IsAttacking => attackTimer >= 0;
    private const float attackTimerBase = 0.15f; // set to..
    private float holdItemTimer = 0;
    private const float holdItemTimerBase = 1.5f; // set to...
    public override bool IsHoldingItem => holdItemTimer > 0;
    public Point FacingTile => Position + facing;
    public Player(Point position, int character = 1) : base(position, 128f)
    {
        Character = character;
        animator = new(CreateAnimations(character), initialAnimType: AnimSequenceType.Idle, origin: new(64,95));
        facing = new(0,1);
        Progression = StoryProgressState.Prologue;
    }
    public static AnimationSet CreateAnimations(int character)
    {
        return new(
            TextureManager.Get($"characters/PlayerCharacter{character}"),
            128, 128, 
            AnimSequenceType.Idle, (
                AnimSequenceType.Pose, new Point(0,128),
                4, 8, true),
            (
                AnimSequenceType.Idle, new Point(0, 256),
                4, 8, true),
            (
                AnimSequenceType.IdleLeft, new Point(0, 384),
                4, 8, true),
            (
                AnimSequenceType.IdleRight, new Point(0, 512),
                4, 8, true),
            (
                AnimSequenceType.IdleUp, new Point(0, 640),
                4, 8, true),
            (
                AnimSequenceType.MoveDown, new Point(0, 768),
                4, 8, true),
            (
                AnimSequenceType.MoveLeft, new Point(0, 896),
                4, 8, true),
            (
                AnimSequenceType.MoveRight, new Point(0, 1024),
                4, 8, true),
            (
                AnimSequenceType.MoveUp, new Point(0, 1152),
                4, 8, true),
            (
                AnimSequenceType.Attack, new Point(0, 1280),
                4, 24, true),
            (
                AnimSequenceType.AttackLeft, new Point(0, 1408),
                4, 24, true),
            (
                AnimSequenceType.AttackRight, new Point(0, 1536),
                4, 24, true),
            (
                AnimSequenceType.AttackUp, new Point(0, 1664),
                4, 24, true)
        );
    }
    public void Attack()
    {
        if(facing == new Point(0,1))
            animator.PlayOnce(AnimSequenceType.Attack, true);
        else if(facing == new Point(-1,0))
            animator.PlayOnce(AnimSequenceType.AttackLeft, true);
        else if(facing == new Point(1,0))
            animator.PlayOnce(AnimSequenceType.AttackRight, true);
        else if(facing == new Point(0,-1))
            animator.PlayOnce(AnimSequenceType.AttackUp, true);
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
            ProgressStory();
            holdItemTimer = holdItemTimerBase;
            return true;
        }
        else if(Progression == StoryProgressState.HasOffering &&
            item == TreasureType.Item_Key)
        {
            ProgressStory();
            holdItemTimer = holdItemTimerBase;
            return true;
        }
        else return false;
    }
    public void ProgressStory()
    {
        Progression = Progression switch
        {
            StoryProgressState.Prologue      => StoryProgressState.LockedOut,
            StoryProgressState.LockedOut     => StoryProgressState.HasOffering,
            StoryProgressState.HasOffering   => StoryProgressState.HasKey,
            StoryProgressState.HasKey        => StoryProgressState.GateOpens,
            StoryProgressState.GateOpens     => StoryProgressState.BackAway,
            StoryProgressState.BackAway      => StoryProgressState.OtherWalksOut,
            StoryProgressState.OtherWalksOut => StoryProgressState.CharMessage,
            StoryProgressState.CharMessage   => StoryProgressState.OtherMessage,
            _ => Progression
        };
    }
    public override void Update(GameTime gameTime)
    {
        if(IsInvulnerable)
            invulnerableTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        if(IsAttacking)
            attackTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        if(IsHoldingItem)
            holdItemTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        base.Update(gameTime);
    }
    public override void Draw(SpriteBatch spriteBatch, Color? colorTint = null)
    {
        if (IsInvulnerable && (int)(invulnerableTimer * 5) % 2 == 0)
            colorTint = Color.Red;
        base.Draw(spriteBatch, colorTint);
    }
}