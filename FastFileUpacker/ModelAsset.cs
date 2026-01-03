using System.Numerics;
using System.Text;

namespace FastFileUnpacker
{
    public sealed class ModelAsset : Asset
    {
        private const byte BillboardFlagMask = 0b_0001_1110;
        private const byte DoubleSidedFlagMask = 0b_0000_0001;

        private const int VertexCountOffset = 0;
        private const int VertexCountSize = 2;
        private const int PolygonCountOffset = 2;
        private const int PolygonCountSize = 2;

        private const int VerticesDataOffset = 4;
        private const int VerticesDataSize = 4;
        private const int PolygonsDataOffset = 8;
        private const int PolygonsDataSize = 4;

        private const int VertexDataSize = 8;
        private const int VertexPositionSize = 2;

        private const int PolygonDataSize = 24;
        private const int PolygonColorOffset = 6;
        private const int PolygonColorSize = 2;
        private const int PolygonTextureNameOffsetSize = 4;
        private const int PolygonTextureNameSize = 16;

        // Static.
        private static readonly Vector3[] _vertices = new Vector3[byte.MaxValue];

        public static int ExtractPolygonCount(byte[] data, int dataOffset)
            => BitConverter.ToInt16(data.AsSpan(dataOffset + PolygonCountOffset, PolygonCountSize));

        public static IEnumerable<Polygon> ExtractPolygons(byte[] data, int headerSize, int dataOffset)
        {
            var vertexCount = BitConverter.ToInt16(data.AsSpan(dataOffset + VertexCountOffset, VertexCountSize));
            var verticesDataOffset = BitConverter.ToInt32(data.AsSpan(dataOffset + VerticesDataOffset, VerticesDataSize));

            for (var i = 0; i < vertexCount; i++)
            {
                var vertexDataOffset = headerSize + verticesDataOffset + VertexDataSize * i;

                var x = BitConverter.ToInt16(data.AsSpan(vertexDataOffset, VertexPositionSize));
                vertexDataOffset += VertexPositionSize;

                var y = BitConverter.ToInt16(data.AsSpan(vertexDataOffset, VertexPositionSize));
                vertexDataOffset += VertexPositionSize;

                var z = BitConverter.ToInt16(data.AsSpan(vertexDataOffset, VertexPositionSize));

                _vertices[i] = new Vector3(x, y, -z);
            }

            var polygonCount = ExtractPolygonCount(data, dataOffset);
            var polygonsDataOffset = BitConverter.ToInt32(data.AsSpan(dataOffset + PolygonsDataOffset, PolygonsDataSize));

            for (var i = 0; i < polygonCount; i++)
            {
                var polygonDataOffset = headerSize + polygonsDataOffset + PolygonColorOffset + PolygonDataSize * i;

                var rgb565 = BitConverter.ToUInt16(data.AsSpan(polygonDataOffset, PolygonColorSize));
                var color = ColorUtilities.Bgra5551ToColor(rgb565);
                polygonDataOffset += PolygonColorSize;

                var v1 = _vertices[data[polygonDataOffset++]];
                var v2 = _vertices[data[polygonDataOffset++]];
                var v3 = _vertices[data[polygonDataOffset++]];
                polygonDataOffset++;

                var uv1 = new Vector2(data[polygonDataOffset], data[polygonDataOffset + 1]) / byte.MaxValue;
                polygonDataOffset += 2;

                var uv2 = new Vector2(data[polygonDataOffset], data[polygonDataOffset + 1]) / byte.MaxValue;
                polygonDataOffset += 2;

                var uv3 = new Vector2(data[polygonDataOffset], data[polygonDataOffset + 1]) / byte.MaxValue;
                polygonDataOffset += 2;

                var isBillboard = (data[polygonDataOffset] & BillboardFlagMask) > 0;
                var isDoubleSided = (data[polygonDataOffset] & DoubleSidedFlagMask) > 0;
                polygonDataOffset += 2;

                var textureName = string.Empty;
                var textureNameOffset = BitConverter.ToInt32(data.AsSpan(polygonDataOffset, PolygonTextureNameOffsetSize));
                if (textureNameOffset > 0)
                {
                    textureName = Encoding.Latin1.GetString(data.AsSpan(headerSize + textureNameOffset, PolygonTextureNameSize))
                        .TrimEnd('\xCD').TrimEnd('\0');
                }

                yield return new Polygon(v1, v2, v3, uv1, uv2, uv3,
                    color, color, color, textureName, isBillboard, isDoubleSided);
            }
        }

        // Class.
        private readonly Polygon[] _polygons;
        public IReadOnlyList<Polygon> Polygons { get; }

        public ModelAsset(string fullName, byte[] data) : base(fullName, data)
        {
            _polygons = new Polygon[ExtractPolygonCount(data, 0)];
            Polygons = _polygons.AsReadOnly();

            var index = 0;
            foreach (var polygon in ExtractPolygons(data, 0, 0))
            {
                _polygons[index] = polygon;
                index++;
            }
        }
    }
}
