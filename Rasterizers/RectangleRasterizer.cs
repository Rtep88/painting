using Microsoft.Xna.Framework;

public class RectangleRasterizer
{
    public static void Rasterize(RectangleShape rectangle, Canvas canvas)
    {
        int borderWidth = rectangle.borderWidth;

        for (int x = rectangle.start.X; x < rectangle.end.X; x++)
        {
            for (int y = rectangle.start.Y; y < rectangle.end.Y; y++)
            {
                bool isBorder = x < rectangle.start.X + borderWidth || x >= rectangle.end.X - borderWidth ||
                                y < rectangle.start.Y + borderWidth || y >= rectangle.end.Y - borderWidth;
                
                if (isBorder)
                    canvas.SetPixel(new Point(x, y), rectangle.borderColor);
                else if (rectangle.filled)
                    canvas.SetPixel(new Point(x, y), rectangle.color);
            }
        }
    }
}