using Microsoft.Xna.Framework;

public enum LineType
{
    Normal,
    Dashed,
    Dotted
}

public class Line
{
    public Point start;
    public Point end;
    public int width;
    public Color color;
    public LineType lineType;

    public Line(Point start, Point end, int width, Color color, LineType lineType)
    {
        this.start = start;
        this.end = end;
        this.width = width;
        this.color = color;
        this.lineType = lineType;
    }
}