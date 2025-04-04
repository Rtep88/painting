using Microsoft.Xna.Framework;

public class Line{
    public Point start;
    public Point end;
    public int width;
    public Color color;
    
    public Line(Point start, Point end, int width, Color color){
        this.start = start;
        this.end = end;
        this.width = width;
        this.color = color;
    }
}