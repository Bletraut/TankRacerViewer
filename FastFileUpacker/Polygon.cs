using System.Drawing;
using System.Numerics;

namespace FastFileUnpacker
{
    public record Polygon(Vector3 V1, Vector3 V2, Vector3 V3,
        Vector2 Uv1, Vector2 Uv2, Vector2 Uv3,
        Color Color1, Color Color2, Color Color3,
        string TextureName, bool IsBillboard, bool isDoubleSided);
}
