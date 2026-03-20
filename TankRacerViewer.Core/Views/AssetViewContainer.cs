using System.Collections.Generic;

using FastFileUnpacker;

using Microsoft.Xna.Framework.Graphics;

namespace TankRacerViewer.Core
{
    public sealed class AssetViewContainer
    {
        public FastFile FastFile { get; }

        private readonly Dictionary<string, TextureAssetView> _textureAssetViews;
        public IReadOnlyDictionary<string, TextureAssetView> TextureAssetViews { get; }
        private readonly Dictionary<string, ModelAssetView> _modelAssetViews;
        public IReadOnlyDictionary<string, ModelAssetView> ModelAssetViews { get; }
        private readonly Dictionary<string, BackgroundAssetView> _backgroundAssetViews;
        public IReadOnlyDictionary<string, BackgroundAssetView> BackgroundAssetViews { get; }
        private readonly Dictionary<string, DataAssetView> _dataAssetViews;
        public IReadOnlyDictionary<string, DataAssetView> DataAssetViews { get; }
        private readonly Dictionary<string, UnsupportedAssetView> _unsupportedAssetViews;
        public IReadOnlyDictionary<string, UnsupportedAssetView> UnsupportedAssetViews { get; }

        public IReadOnlyList<AssetView> AssetViews { get; }
        public List<AssetView> ExtraAssetViews { get; } = [];

        private readonly GraphicsDevice _graphicsDevice;

        public AssetViewContainer(GraphicsDevice graphicsDevice, FastFile fastFile)
        {
            FastFile = fastFile;
            _graphicsDevice = graphicsDevice;

            _textureAssetViews = [];
            TextureAssetViews = _textureAssetViews.AsReadOnly();

            _dataAssetViews = [];
            DataAssetViews = _dataAssetViews.AsReadOnly();

            _unsupportedAssetViews = [];
            UnsupportedAssetViews = _unsupportedAssetViews.AsReadOnly();

            var assetView = new List<AssetView>();
            AssetViews = assetView.AsReadOnly();

            foreach (var asset in FastFile.Assets)
            {
                if (asset is TextureAsset textureAsset)
                {
                    var view = new TextureAssetView(_graphicsDevice, textureAsset);
                    _textureAssetViews.Add(view.Name.ToLowerInvariant(), view);
                    assetView.Add(view);
                }
                else if (asset is DataAsset dataAsset)
                {
                    var view = new DataAssetView(dataAsset.FullName, dataAsset.Text);
                    _dataAssetViews.Add(view.Name.ToLowerInvariant(), view);
                    assetView.Add(view);
                }
                else if (asset is UnsupportedAsset unsupportedAsset)
                {
                    var view = new UnsupportedAssetView(unsupportedAsset.FullName,
                        unsupportedAsset.Description);
                    _unsupportedAssetViews.Add(view.Name.ToLowerInvariant(), view);
                    assetView.Add(view);
                }
            }

            _modelAssetViews = [];
            ModelAssetViews = _modelAssetViews.AsReadOnly();

            foreach (var asset in FastFile.Assets)
            {
                if (asset is ModelContainerAsset modelContainerAsset)
                {
                    foreach (var modelAsset in modelContainerAsset.Models)
                    {
                        var view = new ModelAssetView(_graphicsDevice, modelAsset.FullName,
                            modelAsset.Polygons, _textureAssetViews);
                        _modelAssetViews[view.Name.ToLowerInvariant()] = view;
                        assetView.Add(view);
                    }
                }
                else if (asset is ModelAsset modelAsset)
                {
                    var view = new ModelAssetView(_graphicsDevice, modelAsset.FullName,
                        modelAsset.Polygons, _textureAssetViews);
                    _modelAssetViews[view.Name.ToLowerInvariant()] = view;
                    assetView.Add(view);
                }
                else if (asset is MapAsset mapAsset)
                {
                    var view = new ModelAssetView(_graphicsDevice, mapAsset.FullName,
                        mapAsset.Polygons, _textureAssetViews);
                    _modelAssetViews[view.Name.ToLowerInvariant()] = view;
                    assetView.Add(view);
                }
            }

            _backgroundAssetViews = [];
            BackgroundAssetViews = _backgroundAssetViews.AsReadOnly();

            foreach (var asset in FastFile.Assets)
            {
                if (asset is BackgroundAsset backgroundAsset)
                {
                    var view = new BackgroundAssetView(_graphicsDevice, backgroundAsset.FullName,
                        backgroundAsset.Sides, _textureAssetViews);
                    _backgroundAssetViews.Add(view.Name.ToLowerInvariant(), view);
                    assetView.Add(view);
                }
            }
        }
    }
}
