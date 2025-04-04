using Microsoft.Xna.Framework;

public enum ToolType
{
    Brush,
    Eraser,
    Fill,
    Rectangle,
    Circle,
    Line,
    Polygon
}

public class Tool
{

    public ToolType toolType;
    public Color firstColor = Color.Black;
    public Color secondColor = Color.Black;
    public bool shiftPressed;

    public Tool(ToolType toolType) => this.toolType = toolType;
}