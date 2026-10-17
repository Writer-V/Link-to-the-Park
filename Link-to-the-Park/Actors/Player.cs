using System;
using LinkToThePark.Graphics;
using LinkToThePark.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.Actors;
public class Player : Actor
{
    public Point FacingTile => Position + facing;
    public Player(Point position) : base(position, 128f)
    {
        texture = TextureManager.Get("characters/PlayerCharacter1");
    }
    public void Draw(SpriteBatch spriteBatch)
    {
        // TODO: Replace with proper anim and soure selection.
        spriteBatch.Draw(
            texture,
            drawPosition + new Vector2(16, 31),
            new Rectangle(0, 0, 128, 128), Color.White, 0f,
            new Vector2(64, 95), // origin: Feet
            1f, SpriteEffects.None, 0f);
    }
}