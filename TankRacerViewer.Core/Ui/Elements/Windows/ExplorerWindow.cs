using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using ComposableUi;

using Microsoft.Xna.Framework;

namespace TankRacerViewer.Core
{
    public sealed class ExplorerWindow : WindowElement
    {
        private const string ExtraGroupName = "Extra";

        // Static.
        private static readonly Stack<HierarchyNodeData> _stack = [];
        private static readonly List<HierarchyNodeData> _list = [];

        private static void AddAssetViewGroup(string name,
            HierarchyNodeData parentNode,
            IEnumerable<AssetView> assetViews)
        {
            var hasItems = assetViews.TryGetNonEnumeratedCount(out var count) && count > 0
                || assetViews.Any();
            if (!hasItems)
                return;

            var assetGroupNodeData = new HierarchyNodeData()
            {
                Sprite = IconCollection.Get(IconName.AssetGroup),
                Name = name,
                IsFolded = true,
            };
            parentNode.AddChild(assetGroupNodeData);

            foreach (var assetView in assetViews)
            {
                var assetNodeData = new HierarchyNodeData()
                {
                    File = assetView,
                    Sprite = GetAssetViewIcon(assetView),
                    Name = assetView.FullName,
                    IsFolded = true,
                };
                assetGroupNodeData.AddChild(assetNodeData);
            }
        }

        private static Sprite GetAssetViewIcon(AssetView assetView)
        {
            if (assetView is TextureAssetView)
                return IconCollection.Get(IconName.Texture);

            if (assetView is ModelAssetView)
                return IconCollection.Get(IconName.Model);

            if (assetView is BackgroundAssetView)
                return IconCollection.Get(IconName.Background);

            if (assetView is DataAssetView)
                return IconCollection.Get(IconName.Data);

            if (assetView is LevelView)
                return IconCollection.Get(IconName.Level);

            if (assetView is TankView)
                return IconCollection.Get(IconName.Tank);

            return IconCollection.Get(IconName.Unsupported);
        }

        // Class.
        public event Action<AssetView> AssetViewSelected;

        private readonly ScrollViewElement _scrollView;
        private readonly LazyListViewElement<HierarchyNodeData, HierarchyNodeElement> _lazyListView;

        private readonly List<HierarchyNodeData> _rootNodes = [];
        private readonly List<HierarchyNodeData> _fastFileNodes = [];
        private readonly Dictionary<string, HierarchyNodeData> _folderNodeCache = [];

        private readonly ContextMenuElement _contextMenu;
        private readonly PointerInputHandlerElement _inputArea;

        private readonly ExpandedElement _overlayInputInterceptorParent;
        private readonly PointerInputHandlerElement _overlayInputInterceptor;

        private HierarchyNodeData _selectedNodeData;

        public ExplorerWindow() : base("Explorer")
        {
            this.SetScaledIcon(IconName.Explorer, UiElementFactory.DefaultSpriteScale);

            _lazyListView = new LazyListViewElement<HierarchyNodeData, HierarchyNodeElement>(
                itemFactory: CreateHierarchyNode
            );
            _lazyListView.ItemColumn.ExpandChildrenCrossAxis = true;

            _scrollView = new ScrollViewElement(
                expandingContentWidthMode: ScrollViewElement.ExpandingMode.ExpandToFit,
                content: _lazyListView
            );
            ContentContainer.AddChild(new ExpandedElement(_scrollView));

            _contextMenu = new ContextMenuElement(
                items: [
                    new ContextMenuItemElement(
                        name: "Fold All",
                        clickAction: _ => SelectContextMenuItem(FoldAll)
                    ),
                    new ContextMenuItemElement(
                        name: "Expand All",
                        clickAction: _ => SelectContextMenuItem(ExpandAll)
                    ),
                ]
            )
            {
                Pivot = Alignment.TopLeft,
                IsEnabled = false
            };
            ContentContainer.AddChild(_contextMenu);

            _inputArea = new PointerInputHandlerElement(blockInput: false);
            ContentContainer.AddChild(new ExpandedElement(_inputArea));
            _inputArea.PointerSecondaryClick += OnInputAreaClicked;

            _overlayInputInterceptor = new PointerInputHandlerElement(
                blockInput: false
            );
            _overlayInputInterceptorParent = new ExpandedElement(_overlayInputInterceptor);
            _overlayInputInterceptor.PointerDown += OnOverlayInputInterceptorPointerDown;
            _overlayInputInterceptor.PointerSecondaryDown += OnOverlayInputInterceptorPointerDown;
        }

        public void AddFastFiles(List<(string Path, AssetViewContainer File)> files)
        {
            foreach (var (path, file) in files)
                AddFastFileNode(path, file);

            RefreshLazyListViewItems();
        }

        public void AddFastFile(string path, AssetViewContainer file)
        {
            AddFastFileNode(path, file);
            RefreshLazyListViewItems();
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

        public void FoldAll() => SetFoldedStateForAllNodes(true);
        public void ExpandAll() => SetFoldedStateForAllNodes(false);

        public void RefreshExtraAssetViewNodes()
        {
            foreach (var fileNode in _fastFileNodes)
            {
                if (fileNode.File is not AssetViewContainer assetViewContainer)
                    continue;

                if (assetViewContainer.ExtraAssetViews.Count <= 0)
                    continue;

                var extraGroupNode = fileNode.Children
                    .FirstOrDefault(fileNode => fileNode.Name == ExtraGroupName);
                if (extraGroupNode is not null)
                    continue;

                AddAssetViewGroup(ExtraGroupName, fileNode, assetViewContainer.ExtraAssetViews.Values);
            }

            RefreshLazyListViewItems();
        }

        private void AddFastFileNode(string filePath, AssetViewContainer file)
        {
            var fastFileNodeData = new HierarchyNodeData()
            {
                File = file,
                Sprite = IconCollection.Get(IconName.FastFile),
                Name = Path.GetFileName(filePath),
                IsFolded = false
            };
            _fastFileNodes.Add(fastFileNodeData);

            var directory = Path.GetDirectoryName(filePath);
            var folderNode = GetOrCreateFolderNode(directory);
            folderNode.IsFolded = false;
            folderNode.AddChild(fastFileNodeData);

            if (file is AssetViewContainer assetViewContainer)
            {
                AddAssetViewGroup("Models", fastFileNodeData, assetViewContainer.ModelAssetViews.Values);
                AddAssetViewGroup("Textures", fastFileNodeData, assetViewContainer.TextureAssetViews.Values);
                AddAssetViewGroup("Backgrounds", fastFileNodeData, assetViewContainer.BackgroundAssetViews.Values);
                AddAssetViewGroup("Data", fastFileNodeData, assetViewContainer.DataAssetViews.Values);
                AddAssetViewGroup("Unsupported", fastFileNodeData, assetViewContainer.UnsupportedAssetViews.Values);
                AddAssetViewGroup(ExtraGroupName, fastFileNodeData, assetViewContainer.ExtraAssetViews.Values);
            }
        }

        private HierarchyNodeData GetOrCreateFolderNode(string directory)
        {
            if (_folderNodeCache.TryGetValue(directory, out var folderNode))
                return folderNode;

            var directoryName = Path.GetFileName(directory);
            folderNode = new HierarchyNodeData()
            {
                Sprite = IconCollection.Get(IconName.Folder),
                Name = string.IsNullOrEmpty(directoryName) ? directory : directoryName,
                IsFolded = false
            };
            _folderNodeCache.Add(directory, folderNode);

            var parentDirectory = Path.GetDirectoryName(directory);
            if (string.IsNullOrEmpty(parentDirectory))
            {
                _rootNodes.Add(folderNode);
            }
            else
            {
                var parentFolderNode = GetOrCreateFolderNode(parentDirectory);
                parentFolderNode.AddChild(folderNode);
            }

            return folderNode;
        }

        private void RefreshLazyListViewItems()
        {
            _lazyListView.ClearData();

            for (var i = _rootNodes.Count - 1; i >= 0; i--)
                _stack.Push(_rootNodes[i]);

            while (_stack.Count > 0)
            {
                var node = _stack.Pop();

                node.IsHidden = node.File is null
                    && (node.Parent is null || node.Parent.IsHidden)
                    && node.Children.Count <= 1
                    && node.Children[0]?.File is null;
                if (!node.IsHidden)
                {
                    _lazyListView.AddData(node);

                    if (node.IsFolded)
                        continue;
                }

                for (var i = node.Children.Count - 1; i >= 0; i--)
                    _stack.Push(node.Children[i]);
            }
        }

        private void SetFoldedStateForAllNodes(bool value)
        {
            for (var i = _rootNodes.Count - 1; i >= 0; i--)
                _stack.Push(_rootNodes[i]);

            while (_stack.Count > 0)
            {
                var node = _stack.Pop();
                node.IsFolded = value;

                for (var i = node.Children.Count - 1; i >= 0; i--)
                    _stack.Push(node.Children[i]);
            }

            RefreshLazyListViewItems();
        }

        private void FoldNode(HierarchyNodeData nodeData)
        {
            if (nodeData.IsFolded)
                return;

            _stack.Push(nodeData);

            var childCount = 0;
            while (_stack.Count > 0)
            {
                var parent = _stack.Pop();
                if (!parent.IsFolded)
                {
                    childCount += parent.Children.Count;

                    foreach (var child in parent.Children)
                    {
                        if (parent.IsFolded)
                            continue;

                        _stack.Push(child);
                    }
                }
            }

            var dataIndex = _lazyListView.IndexOf(nodeData);
            _lazyListView.RemoveDataRange(dataIndex + 1, childCount);

            nodeData.IsFolded = true;
        }

        private void ExpandNode(HierarchyNodeData nodeData)
        {
            if (!nodeData.IsFolded)
                return;

            _list.Clear();
            _stack.Clear();

            for (var i = nodeData.Children.Count - 1; i >= 0; i--)
                _stack.Push(nodeData.Children[i]);

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

            var dataIndex = _lazyListView.IndexOf(nodeData);
            _lazyListView.InsertDataRange(dataIndex + 1, CollectionsMarshal.AsSpan(_list));

            nodeData.IsFolded = false;
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

        private void SelectContextMenuItem(Action action)
        {
            HideContextMenu();
            action?.Invoke();
        }

        private void HideContextMenu()
        {
            _contextMenu.IsEnabled = false;
            _overlayInputInterceptorParent.IsEnabled = false;
        }

        private void OnNodeClicked(HierarchyNodeElement node)
        {
            SelectNode(node.Data);
            node.RefreshBackgroundVisualState();
        }

        private void OnNodeFoldButtonClicked(HierarchyNodeElement node)
        {
            if (node.Data.IsFolded)
            {
                ExpandNode(node.Data);
            }
            else
            {
                FoldNode(node.Data);
            }
            node.RefreshFoldButtonSkin();
        }

        private void OnInputAreaClicked(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            Root.ShowInOverlay(_overlayInputInterceptorParent,
                Vector2.Zero, Vector2.Zero, false, false);
            _contextMenu.Show(pointerEvent.Position.ToVector2());
        }

        private void OnOverlayInputInterceptorPointerDown(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            HideContextMenu();
        }
    }
}
