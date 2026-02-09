using System.Drawing;
using System.Numerics;
using System.Text;

namespace FastFileUnpacker
{
    public sealed class BackgroundAsset : Asset
    {
        private const string NullTextureName = "nil";

        // Static.
        private static readonly Vector3 _bottomLeft = new(-1, -1, 0);
        private static readonly Vector3 _topLeft = new(-1, 1, 0);
        private static readonly Vector3 _bottomRight = new(1, -1, 0);
        private static readonly Vector3 _topRight = new(1, 1, 0);

        private static Vector2 ExtractUv(string[] values, int offset)
        {
            var u = int.Parse(values[offset]);
            var v = int.Parse(values[offset + 1]);

            return new Vector2(u, v) / byte.MaxValue;
        }

        private static Color ExtractColor(string[] values, int offset)
        {
            var r = int.Parse(values[offset]);
            var g = int.Parse(values[offset + 1]);
            var b = int.Parse(values[offset + 2]);

            return Color.FromArgb(byte.MaxValue, r, g, b);
        }

        private static void AddBackgroundQuad(List<Polygon> polygons,
            Color color1, Color color2, Color color3, Color color4)
        {
            var v1 = _topLeft;
            var v2 = _topRight;
            var v3 = _bottomLeft;
            polygons.Add(new Polygon(v1, v2, v3, Vector2.Zero, Vector2.Zero, Vector2.Zero,
                color1, color2, color3, string.Empty, false, true));

            v1 = _topRight;
            v2 = _bottomRight;
            v3 = _bottomLeft;
            polygons.Add(new Polygon(v1, v2, v3, Vector2.Zero, Vector2.Zero, Vector2.Zero,
                color2, color4, color3, string.Empty, false, true));
        }

        private static void AddTopQuad(List<Polygon> polygons,
            Vector2 leftBottomUv, Vector2 rightTopUv,
            string textureName)
        {
            var scale = new Vector3(1, 0.3f, 1);
            var offset = new Vector3(0, -scale.Y / 2, 0);

            AddQuad(polygons, scale, offset, leftBottomUv, rightTopUv, textureName);
        }

        private static void AddBottomQuad(List<Polygon> polygons,
            Vector2 leftBottomUv, Vector2 rightTopUv,
            string textureName)
        {
            var scale = new Vector3(1, 0.3f, 1);
            var offset = new Vector3(0, scale.Y - 1, 0);

            AddQuad(polygons, scale, offset, leftBottomUv, rightTopUv, textureName);
        }

        private static void AddQuad(List<Polygon> polygons,
            Vector3 scale, Vector3 offset,
            Vector2 leftBottomUv, Vector2 rightTopUv,
            string textureName)
        {
            var color = Color.White;

            var uv1 = leftBottomUv;
            var uv2 = rightTopUv;
            var uv3 = new Vector2(leftBottomUv.X, rightTopUv.Y);
            var uv4 = new Vector2(rightTopUv.X, leftBottomUv.Y);

            var v1 = _topLeft * scale + offset;
            var v2 = _bottomRight * scale + offset;
            var v3 = _bottomLeft * scale + offset;
            polygons.Add(new Polygon(v1, v2, v3, uv1, uv2, uv3,
                color, color, color, textureName, false, true));

            v1 = _topLeft * scale + offset;
            v2 = _topRight * scale + offset;
            v3 = _bottomRight * scale + offset;
            polygons.Add(new Polygon(v1, v2, v3, uv1, uv4, uv2,
                color, color, color, textureName, false, true));
        }

        // Class.
        private readonly List<IReadOnlyList<Polygon>> _sides = [];
        public IReadOnlyList<IReadOnlyList<Polygon>> Sides { get; }

        public BackgroundAsset(string fullName, byte[] data) : base(fullName, data)
        {
            Sides = _sides.AsReadOnly();

            var text = Encoding.Latin1.GetString(data);

            var rows = text.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1);
            foreach (var row in rows)
            {
                var values = row.Split(" ", StringSplitOptions.RemoveEmptyEntries);

                var textureName1 = values[1];
                var textureName2 = values[2];

                var bottomUv1 = ExtractUv(values, 3);
                var bottomUv2 = ExtractUv(values, 5);
                var topUv1 = ExtractUv(values, 7);
                var topUv2 = ExtractUv(values, 9);

                var color1 = ExtractColor(values, 11);
                var color2 = ExtractColor(values, 14);
                var color3 = ExtractColor(values, 17);
                var color4 = ExtractColor(values, 20);

                var polygons = new List<Polygon>(2);
                _sides.Add(polygons);

                AddBackgroundQuad(polygons, color1, color2, color3, color4);

                if (textureName1 != NullTextureName)
                    AddBottomQuad(polygons, bottomUv1, bottomUv2, textureName1);

                if (textureName2 != NullTextureName)
                    AddTopQuad(polygons, topUv1, topUv2, textureName1);
            }
        }
    }
}
