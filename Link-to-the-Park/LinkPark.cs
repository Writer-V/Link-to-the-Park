using LinkToThePark.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LinkToThePark.World.Tiled;
using LinkToThePark.Graphics;
using LinkToThePark.World;
using LinkToThePark.Input;

namespace LinkToThePark;

public class LinkPark : Game
{
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch = null!;
    private Texture2D whitePixel = null!;
    private ScreenManager screenManager = new ScreenManager();
    private PlayerInputs inputs = new();
    private Map map = null!;
    private bool viewDebug = false;
    public LinkPark()
    {
        graphics = new GraphicsDeviceManager(this);
        graphics.PreferredBackBufferWidth = 640;
        graphics.PreferredBackBufferHeight = 480;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        TextureManager.Initialize(Content);
        whitePixel = new(GraphicsDevice, 1, 1);
        whitePixel.SetData([Color.White]);
        map = new("maps/SE.tmj");
    }

    protected override void Update(GameTime gameTime)
    {
        inputs.UpdateState(true);
        if(inputs.IsActionPressed(InputAction.Quit))
            Exit();
        if(inputs.IsActionPressed(InputAction.Debug))
            viewDebug = !viewDebug;
        // TODO: Add your update logic here
        screenManager.Update(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        spriteBatch.Begin();
        map.DrawBelow(spriteBatch);
        map.DrawAbove(spriteBatch);
        if(viewDebug)
            map.DrawDebug(spriteBatch, whitePixel);
        spriteBatch.End();
        base.Draw(gameTime);
    }
    protected override void UnloadContent()
    {
        whitePixel.Dispose();
        base.UnloadContent();
    }
}
