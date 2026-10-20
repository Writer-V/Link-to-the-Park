using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.Graphics;
public static class NineSlice
{
    public static void Draw(
        SpriteBatch spriteBatch, 
        Texture2D texture, 
        Rectangle dest, 
        int border, 
        int? topBorder = null, int? rightBorder = null, 
        int? bottomBorder = null, int? leftBorder = null)
    {
        // Null border-values use the general rule.
        int left   = leftBorder   ?? border;
        int right  = rightBorder  ?? border;
        int top    = topBorder    ?? border;
        int bottom = bottomBorder ?? border;

        int[] srcX = { 0, left, texture.Width - right};
        int[] srcY = { 0, top, texture.Height - bottom };
        int[] srcW = { left, texture.Width - left - right, right };
        int[] srcH = { top, texture.Height - top - bottom, bottom };
        int[] dstX = { dest.X, dest.X + left, dest.Right - right};
        int[] dstY = { dest.Y, dest.Y + top, dest.Bottom - bottom};
        int[] dstW = { left, dest.Width - left - right, right};
        int[] dstH = { top, dest.Height - top - bottom, bottom };

        for (int row = 0; row < 3; row++)
            for (int col = 0; col < 3; col++)
                spriteBatch.Draw(texture,
                    new Rectangle(dstX[col], dstY[row], dstW[col], dstH[row]),
                    new Rectangle(srcX[col], srcY[row], srcW[col], srcH[row]),
                    Color.White);
    }
    public static void TextFrame(SpriteBatch spriteBatch, Rectangle dest)
    {
        Draw(spriteBatch, TextureManager.Get("UI/textFrame_9-slice"), dest, 16);
    }
    public static void Button(SpriteBatch spriteBatch, Rectangle dest)
    {
        Draw(spriteBatch, TextureManager.Get("UI/button_9-slice"), dest, 8);
    }
}