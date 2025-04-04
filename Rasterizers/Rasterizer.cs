public class Rasterizer
{
    Canvas canvas;

    public Rasterizer(Canvas canvas) => this.canvas = canvas;

    public void Rasterize(Line line) => LineRasterizer.Rasterize(line, this);

    public void Rasterize(Circle circle) => CircleRasterizer.Rasterize(circle, canvas);

    public void Rasterize(RectangleShape rectangle) => RectangleRasterizer.Rasterize(rectangle, canvas);
}