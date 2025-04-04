using System;
using Microsoft.Xna.Framework;

public static class CircleRasterizer
{
    public static void Rasterize(Circle circle, Canvas canvas)
    {
        int radiusSquared = circle.radius * circle.radius;
        int innerRadiusSquared = (circle.radius - circle.borderWidth) * (circle.radius - circle.borderWidth);

        for (int x = -circle.radius; x <= circle.radius; x++)
        {
            for (int y = -circle.radius; y <= circle.radius; y++)
            {
                if (circle.xyRatio < 0.00001)
                    circle.xyRatio = 0.00001f;
                else if (circle.xyRatio > 10000)
                    circle.xyRatio = 10000f;

                long localX = (long)Math.Round(x * (circle.xyRatio > 1 ? circle.xyRatio : 1));
                long localY = (long)Math.Round(y / (circle.xyRatio < 1 ? circle.xyRatio : 1));

                long distSquared = localX * localX + localY * localY;

                if (distSquared <= radiusSquared)
                {
                    if (distSquared > innerRadiusSquared)
                        canvas.SetPixel(new Point(circle.center.X + x, circle.center.Y + y), circle.borderColor);
                    else if (circle.filled)
                        canvas.SetPixel(new Point(circle.center.X + x, circle.center.Y + y), circle.color);
                }
            }
        }
    }

}