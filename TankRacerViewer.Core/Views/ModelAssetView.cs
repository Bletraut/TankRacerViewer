using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

using FastFileUnpacker;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TankRacerViewer.Core
{
    public sealed class ModelAssetView : AssetView
    {
        public const string NullTextureName = "Null";

        // Static.
        private static readonly List<VertexPositionColorTextureOffset> _verticesData = [];

        // Class.
        public BoundingBox BoundingBox { get; }

        private readonly List<MeshPart> _opaque = [];
        public IReadOnlyList<MeshPart> Opaque { get; }
        private readonly List<MeshPart> _opaqueDoubleSided = [];
        public IReadOnlyList<MeshPart> OpaqueDoubleSided { get; }
        private readonly List<MeshPart> _transparent = [];
        public IReadOnlyList<MeshPart> Transparent { get; }
        private readonly List<MeshPart> _transparentDoubleSided = [];
        public IReadOnlyList<MeshPart> TransparentDoubleSided { get; }

        public int PolygonCount { get; }

        private readonly GraphicsDevice _graphicsDevice;

        public ModelAssetView(GraphicsDevice graphicsDevice, string fullName,
            IEnumerable<Polygon> polygons, IReadOnlyDictionary<string, TextureAssetView> textureAssetViewCache)
            : base(fullName)
        {
            _graphicsDevice = graphicsDevice;

            var boundingBox = new BoundingBox()
            {
                Min = new Vector3(float.PositiveInfinity),
                Max = new Vector3(float.NegativeInfinity)
            };

            Opaque = _opaque.AsReadOnly();
            OpaqueDoubleSided = _opaqueDoubleSided.AsReadOnly();
            Transparent = _transparent.AsReadOnly();
            TransparentDoubleSided = _transparentDoubleSided.AsReadOnly();

            var cullModeGroups = polygons.GroupBy(polygon => polygon.isDoubleSided);
            foreach (var cullModeGroup in cullModeGroups)
            {
                var textureGroups = cullModeGroup.GroupBy(polygon => polygon.TextureName.ToLowerInvariant());
                foreach (var textureGroup in textureGroups)
                {
                    Texture2D texture = null;
                    var textureName = NullTextureName;

                    var blendMode = BlendMode.Opaque;
                    if (textureAssetViewCache.TryGetValue(textureGroup.Key, out var textureAsset))
                    {
                        texture = textureAsset.Texture;
                        textureName = textureAsset.FullName;
                        blendMode = textureAsset.BlendMode;
                    }
                    else if (!string.IsNullOrEmpty(textureGroup.Key))
                    {
                        textureName = $"Missing_{textureGroup.Key}";
                        Debug.WriteLine($"Can't find texture with name '{textureGroup.Key}' for '{fullName}' asset.");
                    }

                    _verticesData.Clear();
                    foreach (var polygon in textureGroup)
                    {
                        var billboardFlag = polygon.IsBillboard ? 1 : 0;

                        var v1 = new Vector3(polygon.V1.X, polygon.V1.Y, polygon.V1.Z);
                        var v2 = new Vector3(polygon.V2.X, polygon.V2.Y, polygon.V2.Z);
                        var v3 = new Vector3(polygon.V3.X, polygon.V3.Y, polygon.V3.Z);

                        var uv1 = new Vector2(polygon.Uv1.X, polygon.Uv1.Y);
                        var uv2 = new Vector2(polygon.Uv2.X, polygon.Uv2.Y);
                        var uv3 = new Vector2(polygon.Uv3.X, polygon.Uv3.Y);

                        var color1 = new Color(polygon.Color1.R, polygon.Color1.G, polygon.Color1.B, polygon.Color1.A);
                        var color2 = new Color(polygon.Color2.R, polygon.Color2.G, polygon.Color2.B, polygon.Color2.A);
                        var color3 = new Color(polygon.Color3.R, polygon.Color3.G, polygon.Color3.B, polygon.Color3.A);

                        var quadCenter = Vector3.Zero;

                        var min = Vector3.Min(Vector3.Min(v1, v2), v3);
                        var max = Vector3.Max(Vector3.Max(v1, v2), v3);
                        boundingBox.Min = Vector3.Min(boundingBox.Min, min);
                        boundingBox.Max = Vector3.Max(boundingBox.Max, max);

                        if (polygon.IsBillboard)
                        {
                            quadCenter = min + (max - min) / 2;

                            var normal = Vector3.Normalize(Vector3.Cross(v2 - v1, v3 - v1));

                            var angle = -MathF.Atan2(normal.X, normal.Z);
                            var rotationMatrix = Matrix.CreateRotationY(angle);

                            v1 = quadCenter + Vector3.Transform(v1 - quadCenter, rotationMatrix);
                            v2 = quadCenter + Vector3.Transform(v2 - quadCenter, rotationMatrix);
                            v3 = quadCenter + Vector3.Transform(v3 - quadCenter, rotationMatrix);
                        }

                        _verticesData.Add(new()
                        {
                            Position = new Vector4(v1, billboardFlag),
                            Color = color1,
                            TextureCoordinate = uv1,
                            QuadCenter = quadCenter
                        });

                        _verticesData.Add(new()
                        {
                            Position = new Vector4(v2, billboardFlag),
                            Color = color2,
                            TextureCoordinate = uv2,
                            QuadCenter = quadCenter
                        });

                        _verticesData.Add(new()
                        {
                            Position = new Vector4(v3, billboardFlag),
                            Color = color3,
                            TextureCoordinate = uv3,
                            QuadCenter = quadCenter
                        });

                        PolygonCount++;
                    }

                    var vertexBuffer = new VertexBuffer(_graphicsDevice, VertexPositionColorTextureOffset.VertexDeclaration,
                        _verticesData.Count, BufferUsage.WriteOnly);
                    vertexBuffer.SetData(_verticesData.ToArray());

                    var indexBuffer = new IndexBuffer(_graphicsDevice, IndexElementSize.ThirtyTwoBits,
                        _verticesData.Count, BufferUsage.WriteOnly);
                    indexBuffer.SetData(Enumerable.Range(0, _verticesData.Count).ToArray());

                    var targetList = (cullModeGroup.Key, blendMode) switch
                    {
                        (false, BlendMode.Opaque or BlendMode.AlphaTest) => _opaque,
                        (true, BlendMode.Opaque or BlendMode.AlphaTest) => _opaqueDoubleSided,
                        (false, BlendMode.Transparent) => _transparent,
                        (true, BlendMode.Transparent) => _transparentDoubleSided,
                        _ => throw new NotImplementedException(),
                    };
                    targetList.Add(new MeshPart()
                    {
                        VertexBuffer = vertexBuffer,
                        IndexBuffer = indexBuffer,
                        PrimitiveCount = _verticesData.Count / 3,
                        Texture = texture,
                        TextureName = textureName,
                        BlendMode = blendMode,
                    });
                }
            }

            BoundingBox = boundingBox;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct VertexPositionColorTextureOffset(Vector4 position, Color color,
            Vector2 textureCoordinate, Vector3 triangleCenter) : IVertexType
        {
            // Static.
            public static VertexDeclaration VertexDeclaration { get; } = new([
                new(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 0),
                new(16, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new(20, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                new(28, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 1)
                ]);

            // Struct.
            public Vector4 Position = position;
            public Color Color = color;
            public Vector2 TextureCoordinate = textureCoordinate;
            public Vector3 QuadCenter = triangleCenter;

            readonly VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
        }
    }

    public sealed class MeshPart
    {
        public VertexBuffer VertexBuffer { get; init; }
        public IndexBuffer IndexBuffer { get; init; }
        public int PrimitiveCount { get; init; }
        public Texture2D Texture { get; init; }
        public string TextureName { get; init; }
        public BlendMode BlendMode { get; init; }
        public Color HighlightColor { get; set; } = Color.White;
    }
}
