using System;
using Microsoft.Xna.Framework;

public static class LineRasterizer
{
    public static void Rasterize(Line line, Rasterizer rasterizer)
    {
        int lineRadius = line.width / 2;

        if (line.end == line.start)
        {
            rasterizer.Rasterize(new Circle(line.start, lineRadius, line.color, line.color, 0, true, 1));
            return;
        }

        Point start = line.start;
        Point end = line.end;

        int dashOnSegments = 1, dashOffSegments = -1;
        switch (line.lineType)
        {
            case LineType.Dashed: dashOnSegments = 3; dashOffSegments = 2; break;
            case LineType.Dotted: dashOnSegments = 1; dashOffSegments = 1; break;
        }
        int segmentLength = lineRadius * 2 + 1;
        int dashOnLength = (dashOnSegments - 1) * segmentLength;
        int dashOffLength = (dashOffSegments + 1) * segmentLength;
        int dashCycleLength = Math.Max(1, dashOnLength + dashOffLength);

        Point direction = new Point(Math.Abs(end.X - start.X), -Math.Abs(end.Y - start.Y));
        Vector2 perpendicular = Vector2.Normalize(new Vector2(-(end.Y - start.Y), end.X - start.X));
        Point sign = new Point(start.X < end.X ? 1 : -1, start.Y < end.Y ? 1 : -1);
        int error = direction.X + direction.Y;

        bool lastDrawn = false;

        Point current = start;
        while (true)
        {
            if ((int)Helper.Distance(start - current) % dashCycleLength <= dashOnLength)
            {
                for (int i = -lineRadius; i <= lineRadius; i++)
                {
                    rasterizer.canvas.SetPixel(current + new Point((int)Math.Round(perpendicular.X * i), (int)Math.Round(perpendicular.Y * i)), 
                        line.color);
                }

                if (!lastDrawn)
                {
                    rasterizer.Rasterize(new Circle(current, lineRadius, line.color, line.color, 0, true, 1));
                    lastDrawn = true;
                }
            }
            else if (lastDrawn)
            {
                rasterizer.Rasterize(new Circle(current, lineRadius, line.color, line.color, 0, true, 1));
                lastDrawn = false;
            }

            if (current == end) break;

            if (2 * error >= direction.Y)
            {
                error += direction.Y;
                current = new Point(current.X + sign.X, current.Y);
            }
            else if (2 * error <= direction.X)
            {
                error += direction.X;
                current = new Point(current.X, current.Y + sign.Y);
            }
        }

        if ((int)Helper.Distance(start - current) % dashCycleLength <= dashOnLength)
            rasterizer.Rasterize(new Circle(current, lineRadius, line.color, line.color, 0, true, 1));
    }
}