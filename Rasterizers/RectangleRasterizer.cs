using System;
using Microsoft.Xna.Framework;

public class RectangleRasterizer
{
    public static void Rasterize(RectangleShape rectangle, Canvas canvas)
    {
        int borderWidth = rectangle.borderWidth;

        if (rectangle.filled)
        {
            for (int x = rectangle.start.X; x <= rectangle.end.X; x++)
            {
                for (int y = rectangle.start.Y; y <= rectangle.end.Y; y++)
                {
                    canvas.SetPixel(new Point(x, y), rectangle.color);
                }
            }
        }

        int countOfYes = 1;
        int countOfNo = 0;

        switch (rectangle.borderType)
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

        int counter = 0;
        int realCountOfYes = countOfYes * borderWidth;
        int realCountOfNo = countOfNo * borderWidth;
        int sum = Math.Max(1, realCountOfYes + realCountOfNo);

        borderWidth--;

        for (int x = rectangle.start.X; x <= rectangle.end.X; x++)
        {
            if (counter % sum < realCountOfYes)
                for (int y = rectangle.start.Y; y <= Math.Min(rectangle.start.Y + borderWidth, rectangle.end.Y); y++)
                    canvas.SetPixel(new Point(x, y), rectangle.borderColor);

            counter++;
        }

        for (int y = rectangle.start.Y + borderWidth; y <= rectangle.end.Y; y++)
        {
            if (counter % sum < realCountOfYes)
                for (int x = Math.Max(rectangle.start.X, rectangle.end.X - borderWidth); x <= rectangle.end.X; x++)
                    canvas.SetPixel(new Point(x, y), rectangle.borderColor);

            counter++;
        }

        for (int x = rectangle.end.X - borderWidth; x >= rectangle.start.X; x--)
        {
            if (counter % sum < realCountOfYes)
                for (int y = Math.Max(rectangle.start.Y, rectangle.end.Y - borderWidth); y <= rectangle.end.Y; y++)
                    canvas.SetPixel(new Point(x, y), rectangle.borderColor);

            counter++;
        }

        for (int y = rectangle.end.Y - borderWidth; y >= rectangle.start.Y + borderWidth; y--)
        {
            if (counter % sum < realCountOfYes)
                for (int x = rectangle.start.X; x <= Math.Min(rectangle.start.X + borderWidth, rectangle.end.X); x++)
                    canvas.SetPixel(new Point(x, y), rectangle.borderColor);

            counter++;
        }
    }
}