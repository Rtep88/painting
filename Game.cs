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
    // Sprava grafickeho zarizeni a kreslici nastroj
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    // Konstanty pro velikost platna a celeho okna
    public const int CANVAS_WIDTH = 1280;
    public const int CANVAS_HEIGHT = 720;
    public const int WIDTH = MenuComponent.MENU_WIDTH + CANVAS_WIDTH;
    public const int HEIGHT = CANVAS_HEIGHT;
    public const int CANVAS_OFFSET_X = MenuComponent.MENU_WIDTH;
    public const int CANVAS_OFFSET_Y = 0;

    private const int FPS_INTERVAL = 10;

    // Platno a pomocne promenne
    private Canvas canvas;
    public Canvas previewCanvas;
    public SpriteFont font;
    public Texture2D pixel;

    // Stavy klavesnice a mysi
    private KeyboardState previousKeyboardState;
    private MouseState previousMouseState;
    private Point lastMousePositon;

    private MenuComponent menuComponent;

    // Priznaky pro kresleni a ovladani
    private bool isLeftButtonDown;
    private bool isDrawing;
    private bool renderIt;
    public bool mouseClicked;
    private Point point1;

    private float savedNotification = 0;

    // Priznaky pro kresleni polygonu
    private bool drawingPolygon;
    private List<Point> polygonPoints = new List<Point>();

    // Mozne smery pro zmenu velikosti
    private enum ResizeDirection { None, TopLeft, TopRight, BottomLeft, BottomRight, Left, Right, Top, Bottom }

    // Promenne pro vyber a manipulaci s objektem
    private Rectangle selectedRectangle = new Rectangle(-1, -1, -1, -1);
    private Rectangle originalSelectedRectangle = new Rectangle(-1, -1, -1, -1);
    private Canvas selectedCanvas;
    private Canvas originalCanvas;
    private bool selecting = false;
    private bool moving = false;
    private bool resizing = false;
    private ResizeDirection resizeDirection = ResizeDirection.None;

    // Fronta pro vypocet FPS
    private Queue<int> fpsValues = new Queue<int>();

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // Inicializace velikosti okna a platna
        _graphics.PreferredBackBufferWidth = WIDTH;
        _graphics.PreferredBackBufferHeight = HEIGHT;
        IsFixedTimeStep = false;
        _graphics.ApplyChanges();

        // Vytvoreni jednobarevne textury
        pixel = new Texture2D(GraphicsDevice, 1, 1);
        pixel.SetData([Color.White]);

        // Inicializace fronty FPS
        for (int i = 0; i < FPS_INTERVAL; i++)
            fpsValues.Enqueue(60);

        // Vytvoreni platna
        canvas = new Canvas(GraphicsDevice, CANVAS_WIDTH, CANVAS_HEIGHT);
        previewCanvas = new Canvas(GraphicsDevice, CANVAS_WIDTH, CANVAS_HEIGHT);

        // Inicializace menu
        menuComponent = new MenuComponent(this);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        // Nacteni fontu a inicializace spriteBatch
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        font = Content.Load<SpriteFont>("fonts/Arial");
    }

    protected override void Update(GameTime gameTime)
    {
        // Zpracovani vstupu od uzivatele
        Point currentMousePosition = Mouse.GetState().Position - new Point(1) - new Point(CANVAS_OFFSET_X, CANVAS_OFFSET_Y);
        mouseClicked = Mouse.GetState().LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released;
        Tool selectedTool = menuComponent.selectedTool;
        Mouse.SetCursor(MouseCursor.Arrow);

        // Vymazani platna
        if (IsKeyPressed(Keys.C))
            canvas.Clear();

        // Ulozeni obrazku
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

        // Reset nastroje
        if (IsKeyPressed(Keys.Escape) || IsKeyPressed(Keys.C))
            ResetTool();

        // Smazani vybrane oblasti
        if (Keyboard.GetState().IsKeyDown(Keys.Delete))
        {
            selectedCanvas = null;
            ResetTool();
        }

        // Zmena kurzoru podle nastroje
        if (currentMousePosition.X > 0 && currentMousePosition.Y > 0)
        {
            if (selectedTool.toolType == ToolType.Select)
            {
                Rectangle biggerRectangle = new Rectangle(selectedRectangle.Location - new Point(15), selectedRectangle.Size + new Point(30));
                if (biggerRectangle.Contains(currentMousePosition) && !selecting)
                {
                    if (selectedRectangle.Contains(currentMousePosition) && !resizing)
                        Mouse.SetCursor(MouseCursor.Hand);
                    else if (!moving)
                    {
                        ResizeDirection resizeDirection = GetResizeDirection(currentMousePosition, selectedRectangle);
                        if (resizing)
                            resizeDirection = this.resizeDirection;

                        switch (resizeDirection)
                        {
                            case ResizeDirection.TopLeft:
                                Mouse.SetCursor(MouseCursor.SizeNWSE);
                                break;
                            case ResizeDirection.BottomRight:
                                Mouse.SetCursor(MouseCursor.SizeNWSE);
                                break;
                            case ResizeDirection.TopRight:
                                Mouse.SetCursor(MouseCursor.SizeNESW);
                                break;
                            case ResizeDirection.BottomLeft:
                                Mouse.SetCursor(MouseCursor.SizeNESW);
                                break;
                            case ResizeDirection.Top:
                                Mouse.SetCursor(MouseCursor.SizeNS);
                                break;
                            case ResizeDirection.Bottom:
                                Mouse.SetCursor(MouseCursor.SizeNS);
                                break;
                            case ResizeDirection.Left:
                                Mouse.SetCursor(MouseCursor.SizeWE);
                                break;
                            case ResizeDirection.Right:
                                Mouse.SetCursor(MouseCursor.SizeWE);
                                break;
                        }
                    }
                }
            }
        }

        // Zacatek drzeni mysi
        if (Mouse.GetState().LeftButton == ButtonState.Pressed && !isLeftButtonDown && currentMousePosition.X > 0 && currentMousePosition.Y > 0)
        {
            isLeftButtonDown = true;
            isDrawing = true;
            point1 = currentMousePosition;

            // Zacatek vykresleni polygonu
            if (!drawingPolygon && menuComponent.selectedTool.toolType == ToolType.Polygon)
            {
                drawingPolygon = true;
                polygonPoints.Clear();
            }

            // Zacatek vyberu oblasti
            if (selectedTool.toolType == ToolType.Select)
            {
                Rectangle biggerRectangle = new Rectangle(selectedRectangle.Location - new Point(15), selectedRectangle.Size + new Point(30));
                if (!biggerRectangle.Contains(currentMousePosition))
                {
                    selecting = true;
                    if (selectedCanvas != null)
                    {
                        selectedCanvas.MergeInto(canvas, selectedRectangle.Location);
                        selectedCanvas = null;
                    }
                    originalCanvas = null;
                }
                else if (selectedRectangle.Contains(currentMousePosition))
                {
                    point1 = currentMousePosition;
                    moving = true;
                }
                else
                {
                    point1 = currentMousePosition;
                    resizeDirection = GetResizeDirection(currentMousePosition, selectedRectangle);
                    resizing = true;
                    originalSelectedRectangle = selectedRectangle;
                }
            }
        }
        // Konec drzeni mysi
        else if (Mouse.GetState().LeftButton == ButtonState.Released && isLeftButtonDown)
        {
            isLeftButtonDown = false;

            // Konec kresleni tvaru
            if (isDrawing && !drawingPolygon && selectedTool.toolType != ToolType.Fill && selectedTool.toolType != ToolType.Select)
            {
                previewCanvas.MergeInto(canvas);
                previewCanvas.Clear();
                previewCanvas.RemakeTexture();
            }

            // Vyplneni oblasti
            if (isDrawing && selectedTool.toolType == ToolType.Fill)
            {
                canvas.Fill(currentMousePosition, selectedTool.firstColor);
            }

            // Konec vyberu oblasti
            if (isDrawing && selectedTool.toolType == ToolType.Select)
            {
                if (selecting)
                {
                    selecting = false;
                    if (selectedRectangle.Size.X > 0 && selectedRectangle.Size.Y > 0)
                    {
                        selectedCanvas = canvas.CutIntoNewCanvas(selectedRectangle);
                        selectedCanvas.RemakeTexture();
                        originalCanvas = selectedCanvas;
                    }
                    else
                        ResetTool();
                }
                else if (resizing)
                {
                    resizing = false;
                    originalSelectedRectangle = new Rectangle(-1, -1, -1, -1);
                }
                else if (moving)
                    moving = false;
            }
            isDrawing = false;

            // Konec vykresleni polygonu
            if (drawingPolygon)
            {
                if (polygonPoints.Count > 0)
                {
                    Point distance = polygonPoints[0] - currentMousePosition;
                    if (Math.Sqrt(distance.X * distance.X + distance.Y * distance.Y) < 10)
                    {
                        drawingPolygon = false;

                        previewCanvas.Clear();
                        for (int i = 0; i < polygonPoints.Count - 1; i++)
                            previewCanvas.rasterizer.Rasterize(new Line(polygonPoints[i], polygonPoints[i + 1], selectedTool.thickness,
                                selectedTool.firstColor, LineType.Normal));
                        previewCanvas.rasterizer.Rasterize(new Line(polygonPoints[polygonPoints.Count - 1], polygonPoints[0], selectedTool.thickness,
                            selectedTool.firstColor, LineType.Normal));
                        previewCanvas.MergeInto(canvas);
                        previewCanvas.Clear();
                        previewCanvas.RemakeTexture();
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

        //Vypocet FPS
        if (gameTime.ElapsedGameTime.TotalSeconds != 0)
        {
            fpsValues.Dequeue();
            fpsValues.Enqueue((int)(1 / gameTime.ElapsedGameTime.TotalSeconds));
        }

        // Vykresleni tvaru podle pohybu mysi v aktualnim updatu
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
                    previewCanvas.rasterizer.Rasterize(new Line(point1, point2, selectedTool.thickness, selectedTool.firstColor, LineType.Normal));
                    break;
                case ToolType.DashedLine:
                    point2 = currentMousePosition;
                    if (selectedTool.shiftPressed)
                        point2 = Helper.SnapEndTo45Degrees(point1, point2);
                    previewCanvas.rasterizer.Rasterize(new Line(point1, point2, selectedTool.thickness, selectedTool.firstColor, LineType.Dashed));
                    break;
                case ToolType.DottedLine:
                    point2 = currentMousePosition;
                    if (selectedTool.shiftPressed)
                        point2 = Helper.SnapEndTo45Degrees(point1, point2);
                    previewCanvas.rasterizer.Rasterize(new Line(point1, point2, selectedTool.thickness, selectedTool.firstColor, LineType.Dotted));
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
                        LineType.Normal, selectedTool.secondColor, true));
                    break;
                case ToolType.Polygon:
                    for (int i = 0; i < polygonPoints.Count; i++)
                    {
                        Point start = polygonPoints[i];
                        Point end;

                        if (i == polygonPoints.Count - 1)
                        {
                            if (selectedTool.shiftPressed)
                                end = Helper.SnapEndTo45Degrees(start, currentMousePosition);
                            else
                                end = currentMousePosition;
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
                case ToolType.Eraser:
                    canvas.rasterizer.Rasterize(new Line(currentMousePosition, lastMousePositon, selectedTool.thickness,
                            Color.Transparent, LineType.Normal));
                    break;
                case ToolType.Select:
                    point2 = currentMousePosition;
                    if (selectedCanvas == null)
                    {
                        if (selecting)
                            selectedRectangle = Helper.RemoveNegativeSize(new Rectangle(point1, point2 - point1));
                    }
                    else
                    {
                        if (resizing)
                        {
                            Point resizeSize = point2 - point1;
                            selectedRectangle = new Rectangle(originalSelectedRectangle.Location, originalSelectedRectangle.Size);

                            switch (resizeDirection)
                            {
                                case ResizeDirection.TopLeft:
                                    selectedRectangle.X += resizeSize.X;
                                    selectedRectangle.Y += resizeSize.Y;
                                    selectedRectangle.Width -= resizeSize.X;
                                    selectedRectangle.Height -= resizeSize.Y;
                                    break;
                                case ResizeDirection.TopRight:
                                    selectedRectangle.Y += resizeSize.Y;
                                    selectedRectangle.Width += resizeSize.X;
                                    selectedRectangle.Height -= resizeSize.Y;
                                    break;
                                case ResizeDirection.BottomLeft:
                                    selectedRectangle.X += resizeSize.X;
                                    selectedRectangle.Width -= resizeSize.X;
                                    selectedRectangle.Height += resizeSize.Y;
                                    break;
                                case ResizeDirection.BottomRight:
                                    selectedRectangle.Width += resizeSize.X;
                                    selectedRectangle.Height += resizeSize.Y;
                                    break;
                                case ResizeDirection.Left:
                                    selectedRectangle.X += resizeSize.X;
                                    selectedRectangle.Width -= resizeSize.X;
                                    break;
                                case ResizeDirection.Right:
                                    selectedRectangle.Width += resizeSize.X;
                                    break;
                                case ResizeDirection.Top:
                                    selectedRectangle.Y += resizeSize.Y;
                                    selectedRectangle.Height -= resizeSize.Y;
                                    break;
                                case ResizeDirection.Bottom:
                                    selectedRectangle.Height += resizeSize.Y;
                                    break;
                            }

                            selectedRectangle = Helper.RemoveNegativeSize(selectedRectangle);
                            selectedCanvas = originalCanvas.ResizeToNewCanvas(selectedRectangle);
                        }
                        else if (moving)
                        {
                            selectedRectangle = new Rectangle(selectedRectangle.Location + (point2 - point1), selectedRectangle.Size);
                            point1 = currentMousePosition;
                        }

                    }

                    // Vykresleni vybrane oblasti
                    if (selectedRectangle.Size.X > 0 || selectedRectangle.Size.Y > 0)
                        previewCanvas.rasterizer.Rasterize(new RectangleShape(selectedRectangle.Location - new Point(2),
                            selectedRectangle.Location + selectedRectangle.Size + new Point(1), selectedTool.firstColor, 2,
                            LineType.Dashed, Color.Gray, false));
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
        // Vycisteni obrazovky
        GraphicsDevice.Clear(Color.White);

        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

        // Vykresleni hlavniho a pomocneho platna
        _spriteBatch.Draw(canvas.texture, new Rectangle(CANVAS_OFFSET_X, CANVAS_OFFSET_Y, CANVAS_WIDTH, CANVAS_HEIGHT), Color.White);
        _spriteBatch.Draw(previewCanvas.texture, new Rectangle(CANVAS_OFFSET_X, CANVAS_OFFSET_Y, CANVAS_WIDTH, CANVAS_HEIGHT), Color.White);

        // Vykresleni vybrane oblasti
        if (menuComponent.selectedTool.toolType == ToolType.Select)
        {
            if (selectedCanvas != null)
            {
                _spriteBatch.Draw(selectedCanvas.texture, new Rectangle(new Point(CANVAS_OFFSET_X, CANVAS_OFFSET_Y) +
                    selectedRectangle.Location, new Point(selectedCanvas.width, selectedCanvas.height)), Color.White);
            }
        }

        // Zobrazeni FPS
        _spriteBatch.DrawString(font, "FPS: " + (fpsValues.Sum() / fpsValues.Count).ToString(), new Vector2(10 + CANVAS_OFFSET_X, CANVAS_OFFSET_Y), Color.Black);

        // Zobrazeni notifikace o ulozeni
        if (savedNotification > 0)
        {
            string text = "Saved";
            _spriteBatch.DrawString(font, text, new Vector2(WIDTH - font.MeasureString(text).X - 5, 0), Color.Green);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    // Pomocna metoda pro zjisteni stisknute klavesy
    public bool IsKeyPressed(Keys key)
    {
        return Keyboard.GetState().IsKeyDown(key) && !previousKeyboardState.IsKeyDown(key);
    }

    // Vyresetovani promennych
    public void ResetTool()
    {
        isDrawing = false;
        drawingPolygon = false;
        polygonPoints.Clear();
        previewCanvas.Clear();
        previewCanvas.RemakeTexture();

        if (selectedCanvas != null)
        {
            selectedCanvas.MergeInto(canvas, selectedRectangle.Location);
            selectedCanvas = null;
        }

        selectedRectangle = new Rectangle(-1, -1, -1, -1);
        originalSelectedRectangle = new Rectangle(-1, -1, -1, -1);
        originalCanvas = null;
        selecting = false;
        moving = false;
        resizing = false;
        resizeDirection = ResizeDirection.None;
    }

    private ResizeDirection GetResizeDirection(Point mouse, Rectangle rect)
    {
        const int margin = 20;

        bool left = Math.Abs(mouse.X - rect.Left) <= margin;
        bool right = Math.Abs(mouse.X - rect.Right) <= margin;
        bool top = Math.Abs(mouse.Y - rect.Top) <= margin;
        bool bottom = Math.Abs(mouse.Y - rect.Bottom) <= margin;

        if (left && top) return ResizeDirection.TopLeft;
        if (right && top) return ResizeDirection.TopRight;
        if (left && bottom) return ResizeDirection.BottomLeft;
        if (right && bottom) return ResizeDirection.BottomRight;
        if (left) return ResizeDirection.Left;
        if (right) return ResizeDirection.Right;
        if (top) return ResizeDirection.Top;
        if (bottom) return ResizeDirection.Bottom;

        return ResizeDirection.None;
    }
}
