using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class Canvas
{
    public Texture2D texture;
    public Color[] pixels;
    public Rasterizer rasterizer;
    public int width;
    public int height;

    public Canvas(GraphicsDevice graphicsDevice, int width, int height)
    {
        this.width = width;
        this.height = height;
        texture = new Texture2D(graphicsDevice, width, height);
        pixels = new Color[width * height];
        rasterizer = new Rasterizer(this);

        Clear();
    }

    public void SetPixel(Point point, Color color)
    {
        if (CheckIfPixelIsWithinBorders(point))
            pixels[point.X + point.Y * width] = color;
    }

    public Color GetPixel(Point point)
    {
        if (CheckIfPixelIsWithinBorders(point))
            return pixels[point.X + point.Y * width];
        else
            return Color.Black;
    }

    public bool CheckIfPixelIsWithinBorders(Point point) => !(point.X < 0 || point.X >= width || point.Y < 0 || point.Y >= height);

    public void Clear() => Array.Fill(pixels, new Color(0, 0, 0, 0));

    public void RemakeTexture() => texture.SetData(pixels, 0, pixels.Length);

    public void DrawCircle(Point point, int radius, Color color) => DrawRing(point, radius, radius, color);

    public void DrawRing(Point point, int radius, int width, Color color)
    {
        for (int x = -radius; x <= radius; x++)
            for (int y = -radius; y <= radius; y++)
                if (x * x + y * y <= radius * radius && x * x + y * y >= (radius - width) * (radius - width))
                    SetPixel(new Point(point.X + x, point.Y + y), color);
    }
    
    public void DrawLine(Point start, Point end, int width, Color color)
    {
        int radius = width / 2;
        int dx = Math.Abs(end.X - start.X), sx = start.X < end.X ? 1 : -1;
        int dy = -Math.Abs(end.Y - start.Y), sy = start.Y < end.Y ? 1 : -1;
        int err = dx + dy, e2;

        while (start.X != end.X || start.Y != end.Y)
        {
            e2 = 2 * err;
            if (e2 >= dy) { err += dy; start.X += sx; }
            if (e2 <= dx) { err += dx; start.Y += sy; }
            DrawCircle(start, radius, color);
        }
    }

    // Pouzije BFS k vyplneni oblasti barvou
    public void Fill(Point point, Color color)
    {
        if (CheckIfPixelIsWithinBorders(point))
        {
            Queue<Point> queue = new Queue<Point>();
            queue.Enqueue(point);

            Color colorToFill = GetPixel(point);
            if (colorToFill == color)
                return;

            while (queue.Count > 0)
            {
                Point current = queue.Dequeue();

                for (int x = -1; x <= 1; x++)
                    for (int y = -1; y <= 1; y++)
                        if (CheckIfPixelIsWithinBorders(new Point(current.X + x, current.Y + y)) && GetPixel(new Point(current.X + x, current.Y + y)) == colorToFill)
                        {
                            SetPixel(new Point(current.X + x, current.Y + y), color);
                            queue.Enqueue(new Point(current.X + x, current.Y + y));
                        }
            }
        }
    }

    // Pripise canvas do tohoto canvasu
    public void MergeInto(Canvas canvas)
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                Color color = Helper.BlendColors(GetPixel(new Point(x, y)), canvas.GetPixel(new Point(x, y)));
                canvas.SetPixel(new Point(x, y), color);
            }
    }

    // Pripise canvas do tohoto canvasu
    public void MergeInto(Canvas canvas, Point destinationOffset)
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                Color color = Helper.BlendColors(GetPixel(new Point(x, y)), canvas.GetPixel(new Point(x, y) + destinationOffset));
                canvas.SetPixel(new Point(x, y) + destinationOffset, color);
            }
    }

    // Vystrihne cast canvasu a vrati ji jako novy canvas
    public Canvas CutIntoNewCanvas(Rectangle rectangle)
    {
        Canvas newCanvas = new Canvas(texture.GraphicsDevice, rectangle.Width, rectangle.Height);
        for (int x = 0; x < rectangle.Width; x++)
            for (int y = 0; y < rectangle.Height; y++)
            {
                newCanvas.SetPixel(new Point(x, y), GetPixel(new Point(x + rectangle.X, y + rectangle.Y)));
                SetPixel(new Point(x + rectangle.X, y + rectangle.Y), Color.Transparent);
            }
        return newCanvas;
    }

    // Zmeni velikost tohoto canvasu do noveho canvasu
    public Canvas ResizeToNewCanvas(Rectangle newRectangle)
    {
        int newWidth = newRectangle.Width;
        int newHeight = newRectangle.Height;
        Canvas newCanvas;

        if (newWidth > 0 && newHeight > 0)
        {
            newCanvas = new Canvas(texture.GraphicsDevice, newWidth, newHeight);

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    float srcX = (float)x * width / newWidth;
                    float srcY = (float)y * height / newHeight;

                    int sampleX = (int)Math.Min(width - 1, Math.Round(srcX));
                    int sampleY = (int)Math.Min(height - 1, Math.Round(srcY));

                    Color color = GetPixel(new Point(sampleX, sampleY));
                    newCanvas.SetPixel(new Point(x, y), color);
                }
            }

            newCanvas.RemakeTexture();
        }
        else
            newCanvas = new Canvas(texture.GraphicsDevice, 1, 1);

        return newCanvas;
    }

}