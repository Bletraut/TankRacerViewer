using System.Numerics;

namespace FastFileUnpacker
{
    public record Polygon(Vector3 V1, Vector3 V2, Vector3 V3,
        Vector2 Uv1, Vector2 Uv2, Vector2 Uv3,
        Rgba8888 Color1, Rgba8888 Color2, Rgba8888 Color3,
        string TextureName, bool IsDoubleSided,
        bool IsBillboardTriangleFlipped, float BillboardSize);
}
