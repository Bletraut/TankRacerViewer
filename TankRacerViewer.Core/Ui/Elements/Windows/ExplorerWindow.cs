using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using ComposableUi;

namespace TankRacerViewer.Core
{
    public sealed class ExplorerWindow : WindowElement
    {
        public event Action<AssetView> AssetViewSelected;

        private readonly ScrollViewElement _scrollView;
        private readonly LazyListViewElement _lazyListView;
        private readonly ColumnLayout _groups;

        private FoldableFileGroupElement _selectedAssetGroup;

        public ExplorerWindow() : base("Explorer")
        {
            _groups = new ColumnLayout(
                alignmentFactor: Alignment.TopLeft,
                sizeMainAxisToContent: true,
                sizeCrossAxisToContent: true
            );

            _lazyListView = new LazyListViewElement();

            _scrollView = new ScrollViewElement(
                sizeToContentWidth: true,
                sizeToContentHeight: true,
                content: _groups
                //content: _lazyListView
            );

            ContentContainer.AddChild(new ExpandedElement(_scrollView));
        }

        public void AddFile(string filePath, object file)
        {
            var fastFileGroup = new FoldableFileGroupElement(
                path: filePath,
                iconSkin: StandardSkin.ContentPanel,
                name: Path.GetFileName(filePath),
                isFolded: true
            );
            _groups.AddChild(fastFileGroup);

            if (file is AssetViewContainer assetViewContainer)
            {
                AddAssetViewGroup("Models", fastFileGroup, assetViewContainer.ModelAssetViews.Values);
                AddAssetViewGroup("Textures", fastFileGroup, assetViewContainer.TextureAssetViews.Values);
                AddAssetViewGroup("Backgrounds", fastFileGroup, assetViewContainer.BackgroundAssetViews.Values);
                AddAssetViewGroup("Data", fastFileGroup, assetViewContainer.DataAssetViews.Values);
                AddAssetViewGroup("Unsupported", fastFileGroup, assetViewContainer.UnsupportedAssetViews.Values);
                AddAssetViewGroup("Extra", fastFileGroup, assetViewContainer.ExtraAssetViews.Values);
            }
        }

        private void AddAssetViewGroup(string name,
            FoldableFileGroupElement parentGroup,
            IEnumerable<AssetView> assetViews)
        {
            var hasItems = assetViews.TryGetNonEnumeratedCount(out var count) && count > 0
                || assetViews.Any();
            if (!hasItems)
                return;

            var assetGroup = new FoldableFileGroupElement(
                iconSkin: StandardSkin.RectanglePanel,
                path: string.Empty,
                name: name,
                isFolded: true
            );
            parentGroup.AddItem(assetGroup);

            foreach (var assetView in assetViews)
            {
                var asset = new FoldableFileGroupElement(
                    iconSkin: StandardSkin.TextField,
                    path: string.Empty,
                    name: assetView.FullName,
                    file: assetView
                );
                assetGroup.AddItem(asset);

                asset.ClickInputHandler.PointerClick += (_, _) => OnAssetGroupSelected(asset);
            }
        }

        private void OnAssetGroupSelected(FoldableFileGroupElement assetGroup)
        {
            if (_selectedAssetGroup is not null)
                _selectedAssetGroup.IsSelected = false;

            _selectedAssetGroup = assetGroup;
            _selectedAssetGroup.IsSelected = true;

            if (_selectedAssetGroup.File is AssetView assetView)
                AssetViewSelected?.Invoke(assetView);
        }
    }
}
