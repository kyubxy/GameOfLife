using OpenTK.Mathematics;
using Yasai.Graphics.Shapes;

namespace GameOfLife;

public class DrawableCell : Box
{
    public Vector2i Coord { get; set; }
}