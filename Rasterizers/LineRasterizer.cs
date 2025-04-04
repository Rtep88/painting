using System;
using Microsoft.Xna.Framework;

public static class LineRasterizer
{
    public static void Rasterize(Line line, Rasterizer rasterizer)
    {
        int radius = line.width / 2;
        int dx = Math.Abs(line.end.X - line.start.X);
        int sx = line.start.X < line.end.X ? 1 : -1;
        int dy = -Math.Abs(line.end.Y - line.start.Y);
        int sy = line.start.Y < line.end.Y ? 1 : -1;
        int err = dx + dy, e2;

        int countOfYes = 1;
        int countOfNo = -1;

        switch (line.lineType)
        {
            case LineType.Dashed:
                countOfYes = 3;
                countOfNo = 2;
                break;
            case LineType.Dotted:
                countOfYes = 1;
                countOfNo = 1;
                break;
        }

        int realCountOfYes = (countOfYes - 1) * (radius * 2 + 1);
        int realCountOfNo = (countOfNo + 1) * (radius * 2 + 1);
        int sum = Math.Max(1, realCountOfYes + realCountOfNo);
        Point distance = new Point(0);

        while (line.start.X != line.end.X || line.start.Y != line.end.Y)
        {
            if ((int)Helper.Distance(distance) % sum <= realCountOfYes)
                rasterizer.Rasterize(new Circle(line.start, radius, line.color, line.color, 0, true, 1));

            e2 = 2 * err;
            if (e2 >= dy && e2 <= dx)
            {
                err += dy;
                line.start.X += sx;
                distance.X += sx;
            }
            else if (e2 >= dy)
            {
                err += dy;
                line.start.X += sx;
                distance.X += sx;
            }
            else if (e2 <= dx)
            {
                err += dx;
                line.start.Y += sy;
                distance.Y += sy;
            }
        }

        if ((int)Helper.Distance(distance) % sum <= realCountOfYes)
            rasterizer.Rasterize(new Circle(line.start, radius, line.color, line.color, 0, true, 1));
    }
}