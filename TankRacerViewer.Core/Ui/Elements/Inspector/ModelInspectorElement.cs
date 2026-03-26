using System.Collections.Generic;

using ComposableUi;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TankRacerViewer.Core
{
    public sealed class ModelInspectorElement : InspectorElement<ModelAssetView>
    {
        public static readonly Color HighlightColor = new(Color.Fuchsia, 0f);

        private readonly FoldableGroupElement _usedTexturesGroup;
        public readonly LazyListViewElement<UsedTextureData, UsedTextureElement> _lazyListView;

        private readonly Dictionary<string, Texture2D> _usedTextures = [];

        private readonly List<MeshPart> _highlightedMeshParts = [];

        public ModelInspectorElement()
        {
            InfoGroup.Name.Text = "Model Info";

            _usedTexturesGroup = new FoldableGroupElement(
                name: "Used Textures"
            );
            _usedTexturesGroup.Icon.IsEnabled = false;
            _usedTexturesGroup.ContentBackground.Color = Color.Black;
            _usedTexturesGroup.ContentLayout.LeftPadding = 0;
            _usedTexturesGroup.ContentLayout.TopPadding = 0;
            _usedTexturesGroup.ContentLayout.BottomPadding = 0;
            _usedTexturesGroup.ContentLayout.ExpandChildrenCrossAxis = true;
            GroupLayout.AddChild(_usedTexturesGroup);

            _lazyListView = new LazyListViewElement<UsedTextureData, UsedTextureElement>(
                itemFactory: CreateUsedTexture
            );
            _lazyListView.ItemColumn.ExpandChildrenCrossAxis = true;
            _usedTexturesGroup.ContentLayout.AddChild(_lazyListView);
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

            CollectUsedTextures(Target.Opaque);
            CollectUsedTextures(Target.OpaqueDoubleSided);
            CollectUsedTextures(Target.Transparent);
            CollectUsedTextures(Target.TransparentDoubleSided);
        }
        private void CollectUsedTextures(IReadOnlyList<MeshPart> meshParts)
        {
            foreach (var meshPart in meshParts)
                _usedTextures.TryAdd(meshPart.TextureName, meshPart.Texture);
        }

        private void HighlightMeshParts(string textureName)
        {
            ClearMeshPartsHighlight();

            HighlightMeshParts(textureName, Target.Opaque);
            HighlightMeshParts(textureName, Target.OpaqueDoubleSided);
            HighlightMeshParts(textureName, Target.Transparent);
            HighlightMeshParts(textureName, Target.TransparentDoubleSided);
        }

        private void HighlightMeshParts(string textureName,
            IReadOnlyList<MeshPart> meshParts)
        {
            foreach(var meshPart in meshParts)
            {
                if (meshPart.TextureName == textureName)
                {
                    meshPart.HighlightColor = HighlightColor;
                    _highlightedMeshParts.Add(meshPart);
                }
            }
        }

        private void ClearMeshPartsHighlight()
        {
            foreach (var meshPart in _highlightedMeshParts)
                meshPart.HighlightColor = Color.White;

            _highlightedMeshParts.Clear();
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

        protected override void OnTargetSet()
        {
            ClearMeshPartsHighlight();

            StringBuilder.Clear();
            StringBuilder.AppendLine($"Name: {Target.FullName}");
            StringBuilder.AppendLine($"Triangles: {Target.PolygonCount}");
            StringBuilder.AppendLine($"Opaque: {Target.Opaque.Count + Target.OpaqueDoubleSided.Count}");
            StringBuilder.Append($"Transparent: {Target.Transparent.Count + Target.TransparentDoubleSided.Count}");
            InfoText.Text = StringBuilder.ToString();

            CollectUsedTextures();

            _lazyListView.ClearData();

            var index = 0;
            foreach (var (name, texture) in _usedTextures)
            {
                _lazyListView.AddData(new UsedTextureData(index, texture, name));
                index++;
            }
        }
    }
}
