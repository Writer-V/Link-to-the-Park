using System.Diagnostics;
using LinkToThePark.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.Actors;
public class Player : Actor
{
    public int CurrentLives {get; private set;} = 3;
    public const int MaxLives = 3; // Might change.
    private float invulnerableTimer = 0;
    private bool IsInvulnerable => invulnerableTimer > 0;
    private const float invulnerableBase = 1.5f; // reset to...
    public Point FacingTile => Position + facing;
    public Player(Point position) : base(position, 128f)
    {
        texture = TextureManager.Get("characters/PlayerCharacter1");
    }
    public void TakeHit(int damage = 1) // Prepared for something dealing more damage
    {
        if(IsInvulnerable) return;
        CurrentLives -= damage;
        invulnerableTimer = invulnerableBase;
        Debug.WriteLine($"Lives: {CurrentLives} / {MaxLives}");
    }
    public override void Update(GameTime gameTime)
    {
        invulnerableTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
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
            drawPosition + new Vector2(16, 31),
            new Rectangle(0, 0, 128, 128), color, 0f,
            new Vector2(64, 95), // origin: Feet
            1f, SpriteEffects.None, 0f);
    }
}