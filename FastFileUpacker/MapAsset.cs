using System.Numerics;

namespace FastFileUnpacker
{
    public sealed class MapAsset : Asset
    {
        private const int HeaderSize = 20;

        private const int ChunkDataSize = 48;
        private const int VerticesDataOffset = 4;
        private const int VerticesDataSize = 4;
        private const int CollisionTypeOffset = 28;
        private const int PositionOffset = 36;
        private const int PositionDataSize = 4;

        // Class.
        private readonly List<Polygon> _polygons = [];
        public IReadOnlyList<Polygon> Polygons { get; }

        public MapAsset(string fullName, byte[] data) : base(fullName, data)
        {
            Polygons = _polygons.AsReadOnly();

            var chunkDataOffset = HeaderSize + VerticesDataOffset;
            var chunkCount = BitConverter.ToInt32(data.AsSpan(chunkDataOffset, VerticesDataSize)) / ChunkDataSize;

            for (var i = 0; i < chunkCount; i++)
            {
                var dataOffset = HeaderSize + ChunkDataSize * i;

                // Skip invisible walls.
                var collisionType = data[dataOffset + CollisionTypeOffset];
                if (collisionType == 1)
                    continue;

                var position = new Vector3()
                {
                    X = BitConverter.ToInt32(data.AsSpan(dataOffset + PositionOffset, PositionDataSize)),
                    Y = BitConverter.ToInt32(data.AsSpan(dataOffset + PositionOffset + PositionDataSize, PositionDataSize)),
                    Z = -BitConverter.ToInt32(data.AsSpan(dataOffset + PositionOffset + PositionDataSize * 2, PositionDataSize)),
                };

                foreach (var polygon in ModelAsset.ExtractPolygons(data, HeaderSize, dataOffset))
                {
                    var movedPolygon = polygon with
                    {
                        V1 = polygon.V1 + position,
                        V2 = polygon.V2 + position,
                        V3 = polygon.V3 + position
                    };

                    _polygons.Add(movedPolygon);
                }
            }
        }
    }
}
