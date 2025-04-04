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
}