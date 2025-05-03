using System;
using Microsoft.Xna.Framework;

public static class Helper
{
    public static float Distance(Point p1) => (float)Math.Sqrt(p1.X * p1.X + p1.Y * p1.Y);

    public static Point SnapEndTo45Degrees(Point start, Point end)
    {
        Point size = end - start;
        double angle = Math.Atan2(size.Y, size.X);
        double snappedAngle = Math.Round(angle / (Math.PI / 4)) * (Math.PI / 4);

        double length = Math.Sqrt(size.X * size.X + size.Y * size.Y);
        size.X = (int)(Math.Cos(snappedAngle) * length);
        size.Y = (int)(Math.Sin(snappedAngle) * length);

        return start + size;
    }

    public static Color ColorFromHSV(float x, float y, float saturation)
    {
        float hue = x * 360f;
        float brightness = 1f - y;

        int hi = (int)(hue / 60f) % 6;
        float f = hue / 60f % 1f;

        float v = brightness;
        float p = v * (1f - saturation);
        float q = v * (1f - f * saturation);
        float t = v * (1f - (1f - f) * saturation);

        float r = 0f, g = 0f, b = 0f;

        switch (hi)
        {
            case 0: r = v; g = t; b = p; break;
            case 1: r = q; g = v; b = p; break;
            case 2: r = p; g = v; b = t; break;
            case 3: r = p; g = q; b = v; break;
            case 4: r = t; g = p; b = v; break;
            case 5: r = v; g = p; b = q; break;
        }

        return new Color(r, g, b);
    }

    public static Color BlendColors(Color fg, Color bg)
    {
        float fgA = fg.A / 255f;
        float bgA = bg.A / 255f;

        float outA = fgA + bgA * (1 - fgA);

        if (outA == 0)
            return Color.Transparent;

        float r = (fg.R * fgA + bg.R * bgA * (1 - fgA)) / outA;
        float g = (fg.G * fgA + bg.G * bgA * (1 - fgA)) / outA;
        float b = (fg.B * fgA + bg.B * bgA * (1 - fgA)) / outA;

        return new Color((int)r, (int)g, (int)b, (int)(outA * 255));
    }

    public static bool CheckCollision(Rectangle rectangle, Point point) => rectangle.Contains(point);

    public static bool CheckCollision(Rectangle rectangle, Rectangle rectangle2) => rectangle.Intersects(rectangle2);

    public static Rectangle RemoveNegativeSize(Rectangle rectangle)
    {
        if (rectangle.Width < 0)
        {
            rectangle.X += rectangle.Width;
            rectangle.Width *= -1;
        }
        if (rectangle.Height < 0)
        {
            rectangle.Y += rectangle.Height;
            rectangle.Height *= -1;
        }
        return rectangle;
    }
}