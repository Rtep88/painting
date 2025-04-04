using Microsoft.Xna.Framework;

public class Circle{
    public Point center;
    public int radius;
    public Color color;
    public Color borderColor;
    public int borderWidth = 0;
    public bool filled = true;
    public float xyRatio = 1;

    public Circle(Point center, int radius, Color color, Color borderColor, int borderWidth, bool filled, float xyRatio){
        this.center = center;
        this.radius = radius;
        this.color = color;
        this.borderColor = borderColor;
        this.borderWidth = borderWidth;
        this.filled = filled;
        this.xyRatio = xyRatio;
    }

    public Circle(Point center, int radius, Color color, float xyRatio){
        this.center = center;
        this.radius = radius;
        this.color = color;
        borderColor = color;
        this.xyRatio = xyRatio;
    }
}