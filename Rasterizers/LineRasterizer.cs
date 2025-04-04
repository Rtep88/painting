using System;
using Microsoft.Xna.Framework;

public static class LineRasterizer
{
    public static void Rasterize(Line line, Rasterizer rasterizer)
    {
        int radius = line.width / 2;
        int dx = Math.Abs(line.end.X - line.start.X), sx = line.start.X < line.end.X ? 1 : -1;
        int dy = -Math.Abs(line.end.Y - line.start.Y), sy = line.start.Y < line.end.Y ? 1 : -1;
        int err = dx + dy, e2;

        while (line.start.X != line.end.X || line.start.Y != line.end.Y)
        {
            e2 = 2 * err;
            if (e2 >= dy) { err += dy; line.start.X += sx; }
            if (e2 <= dx) { err += dx; line.start.Y += sy; }
            rasterizer.Rasterize(new Circle(line.start, radius, line.color, line.color, 0, true, 1));
        }
    }
}