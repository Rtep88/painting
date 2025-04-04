using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using painting;

public class MenuComponent : DrawableGameComponent
{
    private SpriteBatch spriteBatch;
    private Game1 game;

    public Tool selectedTool;

    public MenuComponent(Game1 game) : base(game)
    {
        this.game = game;
        game.Components.Add(this);
    }

    public override void Initialize()
    {
        selectedTool = new Tool(ToolType.Brush);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        base.LoadContent();
    }

    public override void Update(GameTime gameTime)
    {
        if (game.IsKeyPressed(Keys.Q))
            selectedTool.toolType = ToolType.Brush;
        if (game.IsKeyPressed(Keys.W))
            selectedTool.toolType = ToolType.Line;
        if (game.IsKeyPressed(Keys.E))
            selectedTool.toolType = ToolType.Circle;
        if (game.IsKeyPressed(Keys.R))
            selectedTool.toolType = ToolType.Rectangle;
        if (game.IsKeyPressed(Keys.T))
            selectedTool.toolType = ToolType.Fill;
        if (game.IsKeyPressed(Keys.Y))
            selectedTool.toolType = ToolType.Polygon;

        selectedTool.shiftPressed = Keyboard.GetState().IsKeyDown(Keys.LeftShift);

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
    }
}