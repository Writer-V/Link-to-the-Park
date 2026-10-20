using LinkToThePark.Actors;
using LinkToThePark.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.UI;
public class Hud
{
    private readonly Texture2D bar = TextureManager.Get("UI/hpBar");
    private readonly Texture2D heart = TextureManager.Get("UI/hpHeart");
    private readonly Texture2D heartEmpty = TextureManager.Get("UI/hpHeartEmpty");
    private readonly Texture2D itemBox = TextureManager.Get("UI/pickedUpQuestItemBox");
    private readonly Texture2D keyTexture = TextureManager.Get("items/FinalKey");
    private readonly Texture2D offeringTexture = TextureManager.Get("items/Offering");
    public void Draw(SpriteBatch spriteBatch, Player player, int score)
    {
        Point barPos = new(8,8);
        spriteBatch.Draw(bar, barPos.ToVector2(), Color.White);

        // Hearts
        for(int i = 0; i < Player.MaxLives; i++)
        {
            Texture2D heartToDraaw = i < player.CurrentLives ? heart : heartEmpty;
            spriteBatch.Draw(
                heartToDraaw, 
                new Vector2(barPos.X + 12 + i * 22, barPos.Y + 8), 
                Color.White);
        }

        // Score
        Vector2 scoreSize = TextureManager.FontSmall.MeasureString(score.ToString());
        spriteBatch.DrawString(
            TextureManager.FontSmall,
            score.ToString(),
            new Vector2(barPos.X + bar.Width - scoreSize.X - 24,
            barPos.Y + bar.Height/2 - scoreSize.Y/2), Color.White);

        // Item box
        spriteBatch.Draw(
            itemBox, 
            new Vector2(barPos.X + bar.Width + 12, barPos.Y), 
            Color.White);
        switch (player.Progression)
        {
            case StoryProgressState.HasOffering:
                spriteBatch.Draw(
                    offeringTexture,
                    new Vector2(
                        barPos.X + bar.Width + 12
                         + itemBox.Width/2
                         -offeringTexture.Width/2, 
                        barPos.Y 
                        + itemBox.Height/2 
                        - offeringTexture.Height/2),
                    Color.White);
                break;
            case StoryProgressState.HasKey:
                spriteBatch.Draw(
                    keyTexture,
                    new Rectangle(
                        barPos.X + bar.Width + 12
                         + keyTexture.Width/2 -8, 
                        barPos.Y 
                        + itemBox.Height/2 - 16,
                        32, 32),
                    new Rectangle(9, 0, 32, 32),
                    Color.White);
                break;
        } // The empty box is a clue for the player to get somethign.
    }
}