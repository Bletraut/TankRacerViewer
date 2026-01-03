using System.Collections.Generic;

using FastFileUnpacker;

using Microsoft.Xna.Framework.Graphics;

namespace TankRacerViewer.Core.Views
{
    public sealed class BackgroundAssetView : AssetView
    {
        private readonly List<ModelAssetView> _modelAssetViews = [];
        public IReadOnlyList<ModelAssetView> ModelAssetViews { get; }

        public BackgroundAssetView(GraphicsDevice graphicsDevice, string fullName,
            IReadOnlyList<IReadOnlyList<Polygon>> sides,
            IReadOnlyDictionary<string, TextureAssetView> textureAssetViewCache) 
            : base(fullName)
        {
            ModelAssetViews = _modelAssetViews.AsReadOnly();

            foreach (var polygons in sides)
            {
                _modelAssetViews.Add(new ModelAssetView(graphicsDevice, fullName,
                    polygons, textureAssetViewCache));
            }

        }
    }
}
