using System;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    using Item = WindowNodeElement<WindowContainerElement>;

    public partial class WindowElement
    {
        private static readonly Element _tabPreviewPlaceHolder = new();
        private static readonly WindowElement _splitPreviewWindow = CreateNonInteractiveWindow();

        private static readonly List<Element> _tabList = [];

        internal static WindowElement CreateNonInteractiveWindow()
        {
            var window = new WindowElement()
            {
                IsInteractable = false,
                BlockInput = false
            };
            window.DragHandle.IsInteractable = false;
            window.DragHandle.BlockInput = false;
            window.Tab.IsInteractable = false;
            window.Tab.BlockInput = false;
            window._tabPreviewInputArea.IsEnabled = false;
            window._splitPreviewInputArea.IsEnabled = false;

            return window;
        }

        // Replace.
        internal static void Replace(Item oldItem, Item newItem)
        {
            oldItem.PrepareReplacementWith(newItem);

            var parentContainer = oldItem.Container;
            if (parentContainer is not null)
            {
                var index = parentContainer.Layout.IndexOf(oldItem);
                parentContainer.Layout.InsertChild(index, newItem);

                parentContainer.ReplaceItem(oldItem, newItem);
            }
            else
            {
                oldItem.Parent?.AddChild(newItem);
            }

            var localPosition = oldItem.LocalPosition;
            newItem.LocalPosition = localPosition - oldItem.PivotOffset + newItem.PivotOffset;

            oldItem.Parent?.RemoveChild(oldItem);
        }

        // Docking.
        public static void DockTo(WindowElement source, Item target, Edge edge)
        {
            Undock(source, Vector2.Zero);

            var dockingMode = EdgeToDockingMode(edge);

            var shouldDockToParent = target.Container is not null
                && target.Container.DockingMode == dockingMode;
            if (shouldDockToParent)
            {
                DockToParent(source, target, edge);
                return;
            }

            var isTabbed = target.Container is not null
                && target.Container.DockingMode is DockingMode.Tab;
            Item currentTarget = isTabbed ? target.Container : target;

            var container = WindowContainerElement.Rent(dockingMode);
            Replace(currentTarget, container);

            var sourceSize = container.Size * SplitPreviewSizeFactor;
            var targetSize = container.Size - sourceSize;

            container.InsertItem(0, container.Layout.ChildCount, currentTarget);
            currentTarget.SetSize(targetSize);

            var insertIndex = EdgeToInsertIndex(edge);
            var inLayoutIndex = container.Layout.IndexOf(currentTarget) + insertIndex;
            container.InsertItem(insertIndex, inLayoutIndex, source);
            source.SetSize(sourceSize);

            source.Focus();
        }

        public static void DockAsTab(WindowElement source, WindowElement target, int index)
        {
            var isSameContainer = target.Container is not null
                && target.Container.DockingMode is DockingMode.Tab
                && source.Container == target.Container;
            if (isSameContainer)
            {
                target.Container.MoveItem(index, source);
                target.MoveTab(index, source.Tab);

                return;
            }

            if (source == target)
                return;

            Undock(source, Vector2.Zero);

            var shouldDockAsTabToParent = target.Container?.DockingMode is DockingMode.Tab;
            if (shouldDockAsTabToParent)
            {
                DockAsTabToParent(source, target, index);
                return;
            }

            var container = WindowContainerElement.Rent(DockingMode.Tab);
            container.ViewHolder.PropagateToInnerElementChildren = true;
            Replace(target, container);

            target.SetViewActive(false);
            target.InsertTab(index, source.Tab);
            container.InsertItem(0, 0, target);

            source.SetViewActive(false);
            container.InsertItem(index, index, source);

            source.Focus();
        }

        public static void Undock(WindowElement source, Vector2 position)
        {
            var container = source.Container;
            if (container is null)
                return;

            var parent = container.Root?.Parent ?? container.Parent;
            parent.AddChild(source);

            source.IsInteractable = true;
            source.RestoreTab();
            source.SetViewActive(true);
            source.ApplyRoot(null);
            source.Position = position;
            container.RemoveItem(source);

            if (container.DockingMode is DockingMode.Tab)
            {
                for (var i = 0; i < container.ItemCount; i++)
                {
                    var item = container.GetItemAt(i);
                    if (item is not WindowElement window)
                        continue;

                    window.Focus();
                    break;
                }
            }

            if (container.ItemCount == 1)
            {
                var lastItem = container.GetLastItem();
                Replace(container, lastItem);
                WindowContainerElement.Return(container);
            }
            else
            {
                container.RecalculateMinSize();
            }

            source.Focus();
        }

        // Conversions.
        private static int EdgeToInsertIndex(Edge edge)
            => edge is Edge.Left or Edge.Top ? 0 : 1;

        private static DockingMode EdgeToDockingMode(Edge edge)
            => edge is Edge.Left or Edge.Right ? DockingMode.HorizontalSplit : DockingMode.VerticalSplit;

        private static Edge EdgeNormalToEdge(Vector2 edgeNormal)
        {
            var edge = edgeNormal switch
            {
                { X: < 0 } => Edge.Left,
                { X: > 0 } => Edge.Right,
                { Y: < 0 } => Edge.Top,
                { Y: > 0 } => Edge.Bottom,
                _ => throw new NotImplementedException(),
            };

            return edge;
        }

        private static DockingMode EdgeNormalToDockingMode(Vector2 edgeNormal)
        {
            var edge = EdgeNormalToEdge(edgeNormal);
            return EdgeToDockingMode(edge);
        }

        // Docking.
        private static void DockToParent(WindowElement source, Item target, Edge edge)
        {
            var insertIndex = EdgeToInsertIndex(edge);
            var targetIndex = target.Container.IndexOfItem(target) + insertIndex;
            var inLayoutIndex = target.Container.Layout.IndexOf(target) + insertIndex;

            var sourceSize = target.Size * SplitPreviewSizeFactor;
            var targetSize = target.Size - sourceSize;

            target.SetSize(targetSize);
            source.SetSize(sourceSize);

            target.Container.InsertItem(targetIndex, inLayoutIndex, source);
        }

        private static void DockAsTabToParent(WindowElement source, WindowElement target, int index)
        {
            source.SetViewActive(false);
            target.Container.InsertItem(index, index, source);

            target.InsertTab(index, source.Tab);
            target.SetViewActive(false);

            source.Focus();
        }
    }
}
