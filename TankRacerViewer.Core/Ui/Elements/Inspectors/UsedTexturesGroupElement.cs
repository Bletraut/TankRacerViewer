using System.Collections.Generic;

using ComposableUi;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TankRacerViewer.Core
{
    public sealed class UsedTexturesGroupElement : FoldableGroupElement
    {
        // Static.
        public static readonly Color HighlightColor = new(Color.Fuchsia, 0f);

        // Class.
        public readonly LazyListViewElement<UsedTextureData, UsedTextureElement> _lazyListView;

        private readonly List<ModelAssetView> _modelAssetViews = [];

        private readonly Dictionary<string, Texture2D> _usedTextures = [];
        private readonly List<MeshPart> _highlightedMeshParts = [];

        public UsedTexturesGroupElement(Sprite iconSprite = default,
            StandardSkin iconSkin = default,
            string name = default,
            Element content = default,
            bool isFolded = default,
            StandardSkin titleBackgroundSkin = DefaultTitleBackgroundSkin,
            StandardSkin contentBackgroundSkin = DefaultContentBackgroundSkin)
            : base(iconSprite,
                  iconSkin,
                  name,
                  content,
                  isFolded,
                  titleBackgroundSkin,
                  contentBackgroundSkin)
        {
            Icon.IsEnabled = false;
            ContentBackground.Color = Color.Black;
            ContentLayout.LeftPadding = 0;
            ContentLayout.TopPadding = 0;
            ContentLayout.BottomPadding = 0;
            ContentLayout.ExpandChildrenCrossAxis = true;

            _lazyListView = new LazyListViewElement<UsedTextureData, UsedTextureElement>(
                itemFactory: CreateUsedTexture
            );
            _lazyListView.ItemColumn.ExpandChildrenCrossAxis = true;
            ContentLayout.AddChild(_lazyListView);
        }

        public void ApplyModel(ModelAssetView modelAssetView)
        {
            _modelAssetViews.Clear();
            _modelAssetViews.Add(modelAssetView);

            ApplyModels();
        }

        public void ApplyModels(IReadOnlyList<ModelAssetView> modelAssetViews)
        {
            _modelAssetViews.Clear();
            foreach (var modelAssetView in modelAssetViews)
                _modelAssetViews.Add(modelAssetView);

            ApplyModels();
        }

        public void ClearMeshPartsHighlight()
        {
            foreach (var meshPart in _highlightedMeshParts)
                meshPart.HighlightColor = Color.White;

            _highlightedMeshParts.Clear();
        }

        private void ApplyModels()
        {
            CollectUsedTextures();

            _lazyListView.ClearData();

            var index = 0;
            foreach (var (name, texture) in _usedTextures)
            {
                _lazyListView.AddData(new UsedTextureData(index, texture, name));
                index++;
            }
        }

        private UsedTextureElement CreateUsedTexture()
        {
            var element = new UsedTextureElement();
            element.PointerEnter += OnUsedTexturePointerEnter;
            element.PointerLeave += OnUsedTexturePointerLeave;

            return element;
        }

        private void CollectUsedTextures()
        {
            _usedTextures.Clear();

            foreach (var modelAssetView in _modelAssetViews)
            {
                CollectUsedTextures(modelAssetView.Opaque);
                CollectUsedTextures(modelAssetView.OpaqueDoubleSided);
                CollectUsedTextures(modelAssetView.Transparent);
                CollectUsedTextures(modelAssetView.TransparentDoubleSided);
            }
        }
        private void CollectUsedTextures(IReadOnlyList<MeshPart> meshParts)
        {
            foreach (var meshPart in meshParts)
                _usedTextures.TryAdd(meshPart.TextureName, meshPart.Texture);
        }

        private void HighlightMeshParts(string textureName)
        {
            ClearMeshPartsHighlight();

            foreach (var modelAssetView in _modelAssetViews)
            {
                HighlightMeshParts(textureName, modelAssetView.Opaque);
                HighlightMeshParts(textureName, modelAssetView.OpaqueDoubleSided);
                HighlightMeshParts(textureName, modelAssetView.Transparent);
                HighlightMeshParts(textureName, modelAssetView.TransparentDoubleSided);
            }
        }

        private void HighlightMeshParts(string textureName,
            IReadOnlyList<MeshPart> meshParts)
        {
            foreach (var meshPart in meshParts)
            {
                if (meshPart.TextureName == textureName)
                {
                    meshPart.HighlightColor = HighlightColor;
                    _highlightedMeshParts.Add(meshPart);
                }
            }
        }

        private void OnUsedTexturePointerEnter(UsedTextureElement element,
            PointerEvent pointerEvent)
        {
            HighlightMeshParts(element.Data.TextureName);
        }

        private void OnUsedTexturePointerLeave(UsedTextureElement element,
            PointerEvent pointerEvent)
        {
            var skipClear = _highlightedMeshParts.Count <= 0
                || _highlightedMeshParts[0].TextureName != element.Data.TextureName;
            if (skipClear)
                return;

            ClearMeshPartsHighlight();
        }
    }
}
