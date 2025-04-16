using Microsoft.Xna.Framework;

public enum ToolType
{
    Brush,
    Line,
    DashedLine,
    DottedLine,
    Polygon,
    Eraser,
    Rectangle,
    Circle,
    Select,
    Fill,
}

public class Tool
{

    public ToolType toolType;
    public Color settedFirstColor = Color.Black;
    public float settedFirstOpacity = 1f;
    public Color settedSecondColor = Color.White;
    public float settedSecondOpacity = 0f;

    public Color firstColor = Color.Black;
    public Color secondColor = Color.White;

    public int thickness = 2;

    public bool shiftPressed;

    public Tool(ToolType toolType) => this.toolType = toolType;

    public void Update()
    {
        firstColor = new Color(settedFirstColor.R * settedFirstOpacity, settedFirstColor.G * settedFirstOpacity, 
            settedFirstColor.B * settedFirstOpacity, settedFirstOpacity);

        secondColor = new Color(settedSecondColor.R * settedSecondOpacity, settedSecondColor.G * settedSecondOpacity, 
            settedSecondColor.B * settedSecondOpacity, settedSecondOpacity);

        firstColor = settedFirstColor;
        secondColor = settedSecondColor;
        firstColor.A = (byte)(settedFirstOpacity * 255);
        secondColor.A = (byte)(settedSecondOpacity * 255);
    }
}