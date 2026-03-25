using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using ComposableUi;

namespace TankRacerViewer.Core
{
    public sealed class ExplorerWindow : WindowElement
    {
        // Static.
        private static readonly Stack<HierarchyNodeData> _stack = [];
        private static readonly List<HierarchyNodeData> _list = [];

        // Class.
        public event Action<AssetView> AssetViewSelected;

        private readonly ScrollViewElement _scrollView;
        private readonly LazyListViewElement<HierarchyNodeData, HierarchyNodeElement> _lazyListView;
        
        private HierarchyNodeData _selectedNodeData;

        public ExplorerWindow() : base("Explorer")
        {
            _lazyListView = new LazyListViewElement<HierarchyNodeData, HierarchyNodeElement>(
                itemFactory: CreateHierarchyNode
            );

            _scrollView = new ScrollViewElement(
                sizeToContentWidth: true,
                sizeToContentHeight: true,
                content: _lazyListView
            );

            ContentContainer.AddChild(new ExpandedElement(_scrollView));
        }

        public void AddFile(string filePath, object file)
        {
            var fastFileNodeData = new HierarchyNodeData()
            {
                File = file,
                Skin = StandardSkin.ContentPanel,
                Name = Path.GetFileName(filePath),
                IsFolded = false,
            };
            _lazyListView.AddData(fastFileNodeData);

            if (file is AssetViewContainer assetViewContainer)
            {
                AddAssetViewGroup("Models", fastFileNodeData, assetViewContainer.ModelAssetViews.Values);
                AddAssetViewGroup("Textures", fastFileNodeData, assetViewContainer.TextureAssetViews.Values);
                AddAssetViewGroup("Backgrounds", fastFileNodeData, assetViewContainer.BackgroundAssetViews.Values);
                AddAssetViewGroup("Data", fastFileNodeData, assetViewContainer.DataAssetViews.Values);
                AddAssetViewGroup("Unsupported", fastFileNodeData, assetViewContainer.UnsupportedAssetViews.Values);
                AddAssetViewGroup("Extra", fastFileNodeData, assetViewContainer.ExtraAssetViews.Values);
            }
        }

        public void SelectNextNode() => SelectNode(1);
        public void SelectPreventNode() => SelectNode(-1);
        public void SelectNode(int indexOffset)
        {
            if (_selectedNodeData is null)
                return;

            var currentIndex = _lazyListView.IndexOf(_selectedNodeData);
            var nextIndex = currentIndex >= 0
                ? int.Clamp(currentIndex + indexOffset, 0, _lazyListView.Data.Count - 1)
                : 0;

            SelectNode(_lazyListView.Data[nextIndex]);

            var itemBoundingRectangle = _lazyListView.CalculateItemBoundingRectangle(nextIndex);
            _scrollView.ScrollVerticalToFitBounds(itemBoundingRectangle);

            OnStateChanged();
        }

        private void AddAssetViewGroup(string name,
            HierarchyNodeData parentNode,
            IEnumerable<AssetView> assetViews)
        {
            var hasItems = assetViews.TryGetNonEnumeratedCount(out var count) && count > 0
                || assetViews.Any();
            if (!hasItems)
                return;

            var assetGroupNodeData = new HierarchyNodeData()
            {
                Skin = StandardSkin.RectanglePanel,
                Name = name,
                IsFolded = false,
            };
            parentNode.AddChild(assetGroupNodeData);

            if (!parentNode.IsFolded)
                _lazyListView.AddData(assetGroupNodeData);

            foreach (var assetView in assetViews)
            {
                var assetNodeData = new HierarchyNodeData()
                {
                    File = assetView,
                    Skin = StandardSkin.TextField,
                    Name = assetView.FullName,
                    IsFolded = true,
                };
                assetGroupNodeData.AddChild(assetNodeData);

                if (!assetGroupNodeData.IsFolded)
                    _lazyListView.AddData(assetNodeData);
            }
        }

        private HierarchyNodeElement CreateHierarchyNode()
        {
            var element = new HierarchyNodeElement();
            element.OnClicked += OnNodeClicked;
            element.OnFoldButtonClicked += OnNodeFoldButtonClicked;

            return element;
        }

        private void SelectNode(HierarchyNodeData data)
        {
            if (_selectedNodeData is not null)
            {
                _selectedNodeData.IsSelected = false;
                foreach (var item in _lazyListView.Items)
                {
                    if (_selectedNodeData == item.Data)
                    {
                        item.RefreshBackgroundVisualState();
                        break;
                    }
                }
            }

            _selectedNodeData = data;
            _selectedNodeData.IsSelected = true;

            AssetViewSelected?.Invoke(_selectedNodeData.File as AssetView);
        }

        private void OnNodeClicked(HierarchyNodeElement node)
        {
            SelectNode(node.Data);
            node.RefreshBackgroundVisualState();
        }

        private void OnNodeFoldButtonClicked(HierarchyNodeElement node)
        {
            var isFolded = !node.Data.IsFolded;

            if (isFolded)
            {
                _stack.Clear();
                _stack.Push(node.Data);

                var childCount = 0;
                while (_stack.Count > 0)
                {
                    var parent = _stack.Pop();
                    if (!parent.IsFolded)
                    {
                        childCount += parent.Children.Count;

                        foreach ( var child in parent.Children)
                        {
                            if (parent.IsFolded)
                                continue;

                            _stack.Push(child);
                        }
                    }
                }

                var dataIndex = _lazyListView.IndexOf(node.Data);
                _lazyListView.RemoveDataRange(dataIndex + 1, childCount);
            }
            else
            {
                _list.Clear();
                _stack.Clear();

                for (var i = node.Data.Children.Count - 1; i >= 0; i--)
                    _stack.Push(node.Data.Children[i]);

                while (_stack.Count > 0)
                {
                    var parent = _stack.Pop();
                    _list.Add(parent);

                    if (!parent.IsFolded)
                    {
                        for (var i = parent.Children.Count - 1; i >= 0; i--)
                            _stack.Push(parent.Children[i]);
                    }
                }

                var dataIndex = _lazyListView.IndexOf(node.Data);
                _lazyListView.InsertDataRange(dataIndex + 1, CollectionsMarshal.AsSpan(_list));
            }

            node.Data.IsFolded = isFolded;
            node.RefreshFoldButtonSkin();
        }
    }
}
