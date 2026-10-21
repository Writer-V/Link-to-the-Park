using System;
using System.Collections.Generic;
using LinkToThePark.Actors;
using LinkToThePark.Graphics;
using LinkToThePark.Input;
using LinkToThePark.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinkToThePark.Screens;
public class MainMenuScreen : BaseScreen
{
    private SpriteAnimator character1Animator;
    private SpriteAnimator character2Animator;
    private bool character1Selected;
    private List<string> expose = new();
    private Vector2 exposeSize = new();
    private Vector2 labelSize;
    public MainMenuScreen(Action<GameState> requestGameState) : base(requestGameState)
    {
    }
    public override void Enter()
    {
        character1Animator = new(Player.CreateAnimations(1), scale: 2f, initialAnimType: AnimSequenceType.Pose);
        character2Animator = new(Player.CreateAnimations(2), scale: 2f, initialAnimType: AnimSequenceType.Pose);
        character1Selected = GameSession.Character == 2;
        expose = MessageBox.Wrap(
            "They barred the castle... The shrine has a key but will demand an offering from those in the west.", 
            600, TextureManager.FontBig);
        foreach(var line in expose)
        {
            Vector2 size = TextureManager.FontBig.MeasureString(line);
            exposeSize.X = size.X > exposeSize.X ? size.X : exposeSize.X;
            exposeSize.Y += TextureManager.FontBig.LineSpacing;
        }
        labelSize = TextureManager.FontBig.MeasureString("HER");
    }

    public override void Exit()
    {
        
    }

    public override void Update(GameTime gameTime, PlayerInputs inputs)
    {
        character1Animator.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        character2Animator.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        if(inputs.IsActionPressed(InputAction.Confirm))
        {
            GameSession.Character = character1Selected ? 1 : 2;
            requestGameState(GameState.Playing);
        }
        else if (inputs.IsActionPressed(InputAction.MoveLeft) || inputs.IsActionPressed(InputAction.MoveRight))
            character1Selected = !character1Selected;
    }
    
    public override void Draw(SpriteBatch spriteBatch, bool viewDebug)
    {
        TextureManager.DrawBackground(spriteBatch, "UI/mainMenuBg", new Point(640,480));
        character1Animator.Draw(spriteBatch, new(0,0));
        character2Animator.Draw(spriteBatch,new(376,0));
        if(character1Selected)
            spriteBatch.Draw(TextureManager.Pixel,
                new Rectangle(80, 214, 64, 10), Color.MediumPurple);
        else spriteBatch.Draw(TextureManager.Pixel,
                new Rectangle(456, 214, 64, 10), Color.MediumPurple);
        spriteBatch.DrawString(
            TextureManager.FontBig, "HER",
            new Vector2(113 - labelSize.X/2, 220),
            Color.White);
        spriteBatch.DrawString(
            TextureManager.FontBig, "HIM",
            new Vector2(488 - labelSize.X/2, 220),
            Color.White);
        for(int i = 0; i < expose.Count; i++)
        {
            spriteBatch.DrawString(TextureManager.FontBig, expose[i], 
                new Vector2(
                    320 - exposeSize.X/2, 
                    440 - exposeSize.Y + TextureManager.FontBig.LineSpacing * i),
                    Color.White);
        }
    }
}