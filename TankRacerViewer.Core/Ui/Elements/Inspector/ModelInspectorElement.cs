using System.Collections.Generic;

using ComposableUi;

using Microsoft.Xna.Framework.Graphics;

namespace TankRacerViewer.Core
{
    public sealed class ModelInspectorElement : InspectorElement<ModelAssetView>
    {
        // Static.
        private static UsedTextureElement CreateUsedTexture()
        {
            return new UsedTextureElement();
        }

        // Class.
        private readonly FoldableGroupElement _usedTexturesGroup;
        public readonly LazyListViewElement<UsedTextureData, UsedTextureElement> _lazyListView;

        private readonly Dictionary<string, Texture2D> _usedTextures = [];

        public ModelInspectorElement()
        {
            InfoGroup.Name.Text = "Model Info";

            _usedTexturesGroup = new FoldableGroupElement(
                name: "Used Textures"
            );
            _usedTexturesGroup.Icon.IsEnabled = false;
            _usedTexturesGroup.ContentLayout.LeftPadding = 0;
            GroupLayout.AddChild(_usedTexturesGroup);

            _lazyListView = new LazyListViewElement<UsedTextureData, UsedTextureElement>(
                itemFactory: CreateUsedTexture
            );
            _lazyListView.a = true;
            _usedTexturesGroup.ContentLayout.AddChild(_lazyListView);
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

        protected override void OnTargetSet()
        {
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
