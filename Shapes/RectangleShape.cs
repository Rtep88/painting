using System;
using Microsoft.Xna.Framework;

public class RectangleShape
{
    public Point start;
    public Point end;
    public Color color;
    public int borderWidth;
    public Color borderColor;
    public bool filled;

    public RectangleShape(Point start, Point end, Color color, int borderWidth, Color borderColor, bool filled)
    {
        int minX = Math.Min(start.X, end.X);
        int maxX = Math.Max(start.X, end.X);
        int minY = Math.Min(start.Y, end.Y);
        int maxY = Math.Max(start.Y, end.Y);

        this.start = new Point(minX, minY);
        this.end = new Point(maxX, maxY);
        this.color = color;
        this.borderWidth = borderWidth;
        this.borderColor = borderColor;
        this.filled = filled;
    }

    public RectangleShape(Point start, Point end, Color color)
    {
        int minX = Math.Min(start.X, end.X);
        int maxX = Math.Max(start.X, end.X);
        int minY = Math.Min(start.Y, end.Y);
        int maxY = Math.Max(start.Y, end.Y);

        this.start = new Point(minX, minY);
        this.end = new Point(maxX, maxY);
        this.color = color;
        borderWidth = 0;
        borderColor = color;
        filled = true;
    }
}