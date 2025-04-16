using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using painting;

public class MenuComponent : DrawableGameComponent
{
    private SpriteBatch spriteBatch;
    private Game1 game;
    private Texture2D colorSelectTexture;

    public Tool selectedTool;

    const int BORDER_THICKNESS = 2;
    public const int MENU_WIDTH = 200;
    public const int GAP = 10;

    public const int MIN_THICKNESS = 1;
    public const int MAX_THICKNESS = 100;

    static readonly Rectangle colorSelector = new Rectangle(GAP, GAP, 180, 180);
    static readonly Rectangle firstColorSelect = new Rectangle(GAP, colorSelector.Bottom + GAP, 25, 25);
    static readonly Rectangle secondColorSelect = new Rectangle(firstColorSelect.Right + GAP, colorSelector.Bottom + GAP, 25, 25);

    static readonly Rectangle saturationSelector = new Rectangle(GAP, secondColorSelect.Bottom + GAP, 180, 25);
    static readonly Rectangle opacitySelector = new Rectangle(GAP, saturationSelector.Bottom + GAP, 180, 25);
    static readonly Rectangle thicknessSelector = new Rectangle(GAP, opacitySelector.Bottom + GAP, 180, 25);

    bool holdingColorSelect = false;
    bool holdingSaturationSelect = false;
    bool holdingOpacitySelect = false;
    bool holdingThicknessSelect = false;

    bool isSelectedFirstColor = true;

    float saturation = 1f;

    public MenuComponent(Game1 game) : base(game)
    {
        this.game = game;
        game.Components.Add(this);
    }

    public override void Initialize()
    {
        selectedTool = new Tool(ToolType.Brush);

        int sizeX = 400;
        int sizeY = 400;

        Color[] data = new Color[sizeX * sizeY];
        colorSelectTexture = new Texture2D(game.GraphicsDevice, sizeX, sizeY);

        for (int x = 0; x < sizeX; x++)
            for (int y = 0; y < sizeY; y++)
                data[y * sizeX + x] = Helper.ColorFromHSV((float)x / sizeX, (float)y / sizeY, saturation);

        colorSelectTexture.SetData(data);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        base.LoadContent();
    }

    public override void Update(GameTime gameTime)
    {
        Point currentMousePosition = Mouse.GetState().Position;

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

        if (game.mouseClicked)
        {
            if (Helper.CheckCollision(firstColorSelect, currentMousePosition))
                isSelectedFirstColor = true;
            else if (Helper.CheckCollision(secondColorSelect, currentMousePosition))
                isSelectedFirstColor = false;
            else if (Helper.CheckCollision(colorSelector, currentMousePosition))
                holdingColorSelect = true;
            else if (Helper.CheckCollision(saturationSelector, currentMousePosition))
                holdingSaturationSelect = true;
            else if (Helper.CheckCollision(opacitySelector, currentMousePosition))
                holdingOpacitySelect = true;
            else if (Helper.CheckCollision(thicknessSelector, currentMousePosition))
                holdingThicknessSelect = true;
        }
        if (Mouse.GetState().LeftButton == ButtonState.Pressed)
        {
            if (holdingColorSelect && Helper.CheckCollision(colorSelector, currentMousePosition))
            {
                (float x, float y) = ((currentMousePosition.X - colorSelector.Left) / (float)colorSelector.Width,
                    (currentMousePosition.Y - colorSelector.Top) / (float)colorSelector.Height);

                x = Math.Min(1, Math.Max(0, x));
                y = Math.Min(1, Math.Max(0, y));

                if (isSelectedFirstColor)
                    selectedTool.settedFirstColor = Helper.ColorFromHSV(x, y, saturation);
                else
                    selectedTool.settedSecondColor = Helper.ColorFromHSV(x, y, saturation);
            }

            if (holdingOpacitySelect)
            {
                float x = (currentMousePosition.X - opacitySelector.Left) / (float)opacitySelector.Width;
                if (isSelectedFirstColor)
                    selectedTool.settedFirstOpacity = Math.Min(1, Math.Max(0, x));
                else
                    selectedTool.settedSecondOpacity = Math.Min(1, Math.Max(0, x));
            }

            if (holdingSaturationSelect)
            {
                float x = (currentMousePosition.X - saturationSelector.Left) / (float)saturationSelector.Width;
                saturation = Math.Min(1, Math.Max(0, x));
            }

            if (holdingThicknessSelect)
            {
                float x = (currentMousePosition.X - thicknessSelector.Left) / (float)thicknessSelector.Width;
                selectedTool.thickness = (int)Math.Min(MAX_THICKNESS, Math.Max(MIN_THICKNESS, (x * (MAX_THICKNESS - MIN_THICKNESS)) + MIN_THICKNESS));
            }
        }
        else
        {
            holdingColorSelect = false;
            holdingOpacitySelect = false;
            holdingSaturationSelect = false;
            holdingThicknessSelect = false;
        }

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        spriteBatch.Begin();

        spriteBatch.Draw(game.pixel, new Rectangle(0, 0, MENU_WIDTH, Game1.HEIGHT), Color.LightGray);

        spriteBatch.Draw(game.pixel, colorSelector, Color.Gray);
        for (int x = 0; x < colorSelector.Width - BORDER_THICKNESS * 2; x++)
            for (int y = 0; y < colorSelector.Height - BORDER_THICKNESS * 2; y++)
            {
                spriteBatch.Draw(game.pixel, new Rectangle(colorSelector.Left + BORDER_THICKNESS + x, colorSelector.Top + BORDER_THICKNESS + y, 1, 1),
                    Helper.ColorFromHSV((float)x / (colorSelector.Width - BORDER_THICKNESS * 2), (float)y / (colorSelector.Height - BORDER_THICKNESS * 2), saturation));
            }

        DrawWithBorder(game.pixel, firstColorSelect, selectedTool.firstColor, isSelectedFirstColor ? Color.Red : Color.Gray);
        DrawWithBorder(game.pixel, secondColorSelect, selectedTool.secondColor, !isSelectedFirstColor ? Color.Red : Color.Gray);

        DrawSlider(saturationSelector, Color.Gray, saturation);
        DrawSlider(opacitySelector, Color.Gray, isSelectedFirstColor ? selectedTool.settedFirstOpacity : selectedTool.settedSecondOpacity);
        DrawSlider(thicknessSelector, Color.Gray, (float)((selectedTool.thickness - (float)MIN_THICKNESS) / (MAX_THICKNESS - MIN_THICKNESS)));

        selectedTool.Update();

        spriteBatch.End();

        base.Draw(gameTime);
    }

    public void DrawWithBorder(Texture2D texture, Rectangle rect, Color color, Color borderColor)
    {
        spriteBatch.Draw(game.pixel, rect, borderColor);
        spriteBatch.Draw(game.pixel, new Rectangle(rect.X + BORDER_THICKNESS, rect.Y + BORDER_THICKNESS,
            rect.Width - BORDER_THICKNESS * 2, rect.Height - BORDER_THICKNESS * 2), Color.White);
        spriteBatch.Draw(texture, new Rectangle(rect.X + BORDER_THICKNESS, rect.Y + BORDER_THICKNESS,
            rect.Width - BORDER_THICKNESS * 2, rect.Height - BORDER_THICKNESS * 2), color);
    }

    public void DrawSlider(Rectangle rect, Color color, float value)
    {
        int xOffset = (int)(value * (rect.Width - 10));
        spriteBatch.Draw(game.pixel, new Rectangle(rect.X, rect.Y + rect.Height / 2 - 1, rect.Width, 2), color);
        spriteBatch.Draw(game.pixel, new Rectangle(rect.X + xOffset, rect.Y, 10, rect.Height), color);
    }
}