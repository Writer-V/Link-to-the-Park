using LinkToThePark.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LinkToThePark.Graphics;
using LinkToThePark.Input;
using LinkToThePark.Audio;
using LinkToThePark.World;

namespace LinkToThePark;

public class LinkPark : Game
{
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch = null!;
    private ScreenManager screenManager;
    private PlayerInputs inputs = new();
    public LinkPark()
    {
        graphics = new GraphicsDeviceManager(this);
        graphics.PreferredBackBufferWidth = 640;
        graphics.PreferredBackBufferHeight = 480;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        screenManager = new(inputs);
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        TextureManager.Initialize(Content, GraphicsDevice);
        AudioManager.Initialize(Content);
        DialogBook.Initialize();
    }

    protected override void Update(GameTime gameTime)
    {
        inputs.UpdateState(IsActive);
        if(inputs.IsActionPressed(InputAction.Quit))
            Exit();
        screenManager.Update(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        screenManager.Draw(spriteBatch);
        spriteBatch.End();
        base.Draw(gameTime);
    }
    protected override void UnloadContent()
    {
        TextureManager.Unload();
        base.UnloadContent();
    }
}
