using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace painting;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    public const int CANVAS_WIDTH = 1280;
    public const int CANVAS_HEIGHT = 720;

    public const int WIDTH = MenuComponent.MENU_WIDTH + CANVAS_WIDTH;
    public const int HEIGHT = CANVAS_HEIGHT;
    public const int CANVAS_OFFSET_X = MenuComponent.MENU_WIDTH;
    public const int CANVAS_OFFSET_Y = 0;

    private const int FPS_INTERVAL = 10;

    private Canvas canvas;
    private Canvas previewCanvas;
    private SpriteFont font;
    public Texture2D pixel;

    private KeyboardState previousKeyboardState;
    private MouseState previousMouseState;
    private Point lastMousePositon;

    private MenuComponent menuComponent;

    private bool isLeftButtonDown;
    private bool isDrawing;
    private bool renderIt;
    public bool mouseClicked;
    private Point point1;

    private float savedNotification = 0;

    private bool drawingPolygon;
    private List<Point> polygonPoints = new List<Point>();

    private Queue<int> fpsValues = new Queue<int>();

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferWidth = WIDTH;
        _graphics.PreferredBackBufferHeight = HEIGHT;
        IsFixedTimeStep = false;

        _graphics.ApplyChanges();

        pixel = new Texture2D(GraphicsDevice, 1, 1);
        pixel.SetData([Color.White]);

        for (int i = 0; i < FPS_INTERVAL; i++)
            fpsValues.Enqueue(60);

        canvas = new Canvas(GraphicsDevice, CANVAS_WIDTH, CANVAS_HEIGHT);
        previewCanvas = new Canvas(GraphicsDevice, CANVAS_WIDTH, CANVAS_HEIGHT);

        menuComponent = new MenuComponent(this);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        font = Content.Load<SpriteFont>("fonts/Arial");
    }

    protected override void Update(GameTime gameTime)
    {
        Point currentMousePosition = Mouse.GetState().Position - new Point(1) - new Point(CANVAS_OFFSET_X, CANVAS_OFFSET_Y);
        mouseClicked = Mouse.GetState().LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released;
        Tool selectedTool = menuComponent.selectedTool;

        if (IsKeyPressed(Keys.C))
            canvas.Clear();

        if (IsKeyPressed(Keys.S))
        {
            savedNotification = 3;
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filePath = Path.Combine(desktopPath, "painting.png");

            Canvas saveCanvas = new Canvas(GraphicsDevice, CANVAS_WIDTH, CANVAS_HEIGHT);
            for (int x = 0; x < CANVAS_WIDTH; x++)
                for (int y = 0; y < CANVAS_HEIGHT; y++)
                {
                    Color color = canvas.GetPixel(new Point(x, y));
                    saveCanvas.SetPixel(new Point(x, y), color.A == 0 ? Color.White : color);
                }
            saveCanvas.RemakeTexture();

            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                saveCanvas.texture.SaveAsPng(stream, CANVAS_WIDTH, CANVAS_HEIGHT);
            }
        }

        if (IsKeyPressed(Keys.Escape) || IsKeyPressed(Keys.C))
        {
            isDrawing = false;
            drawingPolygon = false;
            polygonPoints.Clear();
            previewCanvas.Clear();
            previewCanvas.RemakeTexture();
        }

        if (Mouse.GetState().LeftButton == ButtonState.Pressed && !isLeftButtonDown && currentMousePosition.X > 0 && currentMousePosition.Y > 0)
        {
            isLeftButtonDown = true;
            isDrawing = true;
            point1 = currentMousePosition;

            if (!drawingPolygon && menuComponent.selectedTool.toolType == ToolType.Polygon)
            {
                drawingPolygon = true;
                polygonPoints.Clear();
            }
        }
        else if (Mouse.GetState().LeftButton == ButtonState.Released && isLeftButtonDown)
        {
            isLeftButtonDown = false;

            if (isDrawing && !drawingPolygon)
            {
                previewCanvas.MergeInto(canvas);
                previewCanvas.Clear();
                previewCanvas.RemakeTexture();
                renderIt = true;
            }

            isDrawing = false;

            if (drawingPolygon)
            {
                if (polygonPoints.Count > 0)
                {
                    Point distance = polygonPoints[0] - currentMousePosition;
                    if (Math.Sqrt(distance.X * distance.X + distance.Y * distance.Y) < 10)
                    {
                        drawingPolygon = false;
                        renderIt = true;
                    }
                    else
                    {
                        if (selectedTool.shiftPressed)
                            polygonPoints.Add(Helper.SnapEndTo45Degrees(polygonPoints.Last(), currentMousePosition));
                        else
                            polygonPoints.Add(currentMousePosition);
                    }
                }
                else
                    polygonPoints.Add(currentMousePosition);
            }
        }

        //Calculating fps
        if (gameTime.ElapsedGameTime.TotalSeconds != 0)
        {
            fpsValues.Dequeue();
            fpsValues.Enqueue((int)(1 / gameTime.ElapsedGameTime.TotalSeconds));
        }

        if (isDrawing || drawingPolygon)
        {
            if (selectedTool.toolType != ToolType.Brush)
                previewCanvas.Clear();

            Point size, point2;
            switch (selectedTool.toolType)
            {
                case ToolType.Circle:
                    size = lastMousePositon - point1;
                    int radius = selectedTool.shiftPressed ? Math.Min(Math.Abs(size.X) / 2, Math.Abs(size.Y) / 2) : Math.Max(Math.Abs(size.X) / 2, Math.Abs(size.Y) / 2);
                    Point offset = selectedTool.shiftPressed ? new Point(radius) * new Point(Math.Sign(size.X), Math.Sign(size.Y)) : size / new Point(2);
                    float xyRatio = selectedTool.shiftPressed ? 1 : Math.Abs((float)size.Y / size.X);
                    previewCanvas.rasterizer.Rasterize(new Circle(point1 + offset, radius, selectedTool.firstColor, selectedTool.secondColor, 
                        selectedTool.thickness, true, xyRatio));
                    break;
                case ToolType.Line:
                    point2 = currentMousePosition;
                    if (selectedTool.shiftPressed)
                        point2 = Helper.SnapEndTo45Degrees(point1, point2);
                    previewCanvas.rasterizer.Rasterize(new Line(point1, point2, selectedTool.thickness, selectedTool.firstColor, LineType.Dashed));
                    break;
                case ToolType.Brush:
                    previewCanvas.rasterizer.Rasterize(new Line(currentMousePosition, lastMousePositon, selectedTool.thickness, 
                        selectedTool.firstColor, LineType.Normal));
                    break;
                case ToolType.Rectangle:
                    point2 = currentMousePosition;
                    size = point2 - point1;
                    if (selectedTool.shiftPressed)
                    {
                        if (Math.Abs(size.X) < Math.Abs(size.Y))
                            point2 = point1 + new Point(size.X, Math.Abs(size.X) * Math.Sign(size.Y));
                        else
                            point2 = point1 + new Point(Math.Abs(size.Y) * Math.Sign(size.X), size.Y);
                    }
                    previewCanvas.rasterizer.Rasterize(new RectangleShape(point1, point2, selectedTool.firstColor, selectedTool.thickness, 
                        selectedTool.secondColor, true));
                    break;
                case ToolType.Polygon:
                    for (int i = 0; i < polygonPoints.Count; i++)
                    {
                        Point start = polygonPoints[i];
                        Point end;

                        if (i == polygonPoints.Count - 1)
                        {
                            if (!renderIt)
                            {
                                if (selectedTool.shiftPressed)
                                    end = Helper.SnapEndTo45Degrees(start, currentMousePosition);
                                else
                                    end = currentMousePosition;
                            }
                            else
                                end = polygonPoints[0];
                        }
                        else
                            end = polygonPoints[i + 1];

                        previewCanvas.rasterizer.Rasterize(new Line(start, end, selectedTool.thickness, 
                            selectedTool.firstColor, LineType.Normal));
                    }
                    break;
                case ToolType.Fill:
                    if (renderIt)
                        canvas.Fill(currentMousePosition, selectedTool.firstColor);
                    break;
            }

            renderIt = false;
            previewCanvas.RemakeTexture();
        }

        canvas.RemakeTexture();

        base.Update(gameTime);

        if (savedNotification > 0)
            savedNotification -= (float)gameTime.ElapsedGameTime.TotalSeconds;

        previousKeyboardState = Keyboard.GetState();
        lastMousePositon = currentMousePosition;
        isLeftButtonDown = Mouse.GetState().LeftButton == ButtonState.Pressed;
        previousMouseState = Mouse.GetState();
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.White);

        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
        _spriteBatch.Draw(canvas.texture, new Rectangle(CANVAS_OFFSET_X, CANVAS_OFFSET_Y, CANVAS_WIDTH, CANVAS_HEIGHT), Color.White);
        _spriteBatch.Draw(previewCanvas.texture, new Rectangle(CANVAS_OFFSET_X, CANVAS_OFFSET_Y, CANVAS_WIDTH, CANVAS_HEIGHT), Color.White);

        _spriteBatch.DrawString(font, "FPS: " + (fpsValues.Sum() / fpsValues.Count).ToString(), new Vector2(10 + CANVAS_OFFSET_X, CANVAS_OFFSET_Y), Color.Black);

        if (savedNotification > 0)
        {
            string text = "Saved";
            _spriteBatch.DrawString(font, text, new Vector2(WIDTH - font.MeasureString(text).X - 5, 0), Color.Green);
        }
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    public bool IsKeyPressed(Keys key)
    {
        return Keyboard.GetState().IsKeyDown(key) && !previousKeyboardState.IsKeyDown(key);
    }
}
