using System;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    using Item = WindowNodeElement<WindowContainerElement>;

    public partial class Window3Element
    {
        private static readonly Element _tabPreviewPlaceHolder = new();
        private static readonly Window3Element _splitPreviewWindow = CreateNonInteractiveWindow();

        private static readonly List<Element> _tabList = [];

        internal static Window3Element CreateNonInteractiveWindow()
        {
            var window = new Window3Element()
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
                if (parentContainer.ViewHolder.InnerElement is ContainerElement layout)
                {
                    var index = layout.IndexOf(oldItem);
                    layout.InsertChild(index, newItem);
                }

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
        public static void DockTo(Window3Element source, Item target, Edge edge)
        {
            Undock(source, Vector2.Zero);

            var dockingMode = EdgeToDockingMode(edge);

            var shouldAttachToParent = target.Container is not null
                && target.Container.DockingMode == dockingMode;
            if (shouldAttachToParent)
            {
                if (TryDockToParent(source, target, edge))
                    return;
            }

            var insertIndex = EdgeToInsertIndex(edge);
            var layout = dockingMode is DockingMode.HorizontalSplit
                ? WindowContainerElement.GetRow()
                : WindowContainerElement.GetColumn();

            var isTabbed = target.Container is not null
                && target.Container.DockingMode is DockingMode.Tab;
            Item currentTarget = isTabbed ? target.Container : target;

            var container = WindowContainerElement.Rent();
            container.DockingMode = dockingMode;
            container.ViewHolder.PropagateToInnerElementChildren = false;
            container.ViewHolder.InnerElement = layout;
            Replace(currentTarget, container);

            var sourceSize = container.Size * SplitPreviewSizeFactor;
            var targetSize = container.Size - sourceSize;

            container.InsertItem(0, layout.ChildCount, currentTarget);
            currentTarget.SetSize(targetSize);

            var inLayoutIndex = layout.IndexOf(currentTarget) + insertIndex;
            container.InsertItem(insertIndex, inLayoutIndex, source);
            source.SetSize(sourceSize);
        }

        public static void DockAsTab(Window3Element source, Window3Element target, int index)
        {
            //var areSame = target._containerWindow is not null
            //    && target._containerWindow._compositionType == CompositionType.Tabbed
            //    && source._containerWindow == target._containerWindow;
            //if (areSame)
            //{
            //    target._containerWindow._childWindows.Remove(source);
            //    target._containerWindow._childWindows.Insert(index, source);

            //    target._tabRow.RemoveChild(source.Tab);
            //    target._tabRow.InsertChild(index, source.Tab);

            //    return;
            //}

            //if (source == target)
            //    return;

            //DetachFromParent(source, Vector2.Zero);

            //var shouldInsertToParent = target._containerWindow?._compositionType is CompositionType.Tabbed;
            //if (shouldInsertToParent)
            //{
            //    if (TryInsertToParent(source, target))
            //        return;
            //}

            //var layout = new ContainerElement();

            //var containerWindow = GetContainerWindow();
            //containerWindow._compositionType = CompositionType.Tabbed;
            //containerWindow._splitDirection = SplitDirection.None;
            //containerWindow._viewHolder.PropagateToInnerElementChildren = true;
            //containerWindow._viewHolder.InnerElement = layout;
            //containerWindow.SetSize(target.Size);
            //containerWindow._containerWindow = target._containerWindow;
            //containerWindow.IsInteractable = target.IsInteractable;
            //containerWindow.LocalPosition = target.LocalPosition
            //    - target.PivotOffset + containerWindow.PivotOffset;

            //// Replaces the target window with the container window.
            //if (target._containerWindow is not null)
            //{
            //    if (target._containerWindow._viewHolder.InnerElement is ContainerElement parentSplitLayout)
            //    {
            //        var viewIndex = parentSplitLayout.IndexOf(target);
            //        parentSplitLayout.InsertChild(viewIndex, containerWindow);
            //    }

            //    var targetIndex = target._containerWindow._childWindows.IndexOf(target);
            //    target._containerWindow._childWindows.Remove(target);
            //    target._containerWindow._childWindows.Insert(targetIndex, containerWindow);
            //}
            //else
            //{
            //    target.Parent.AddChild(containerWindow);
            //}

            //containerWindow._childWindows ??= [];

            //target._containerWindow = containerWindow;
            //target.IsInteractable = false;
            //layout.AddChild(target);
            //containerWindow._childWindows.Add(target);

            //source._containerWindow = containerWindow;
            //source.IsInteractable = false;
            //source.BlockInput = false;
            //source._viewHolder.IsEnabled = false;
            //source.SetSize(target.Size);
            //layout.AddChild(source);
            //target._tabRow.InsertChild(index, source.Tab);
            //containerWindow._childWindows.Insert(index, source);

            //containerWindow.ApplyRootWindow(target._rootWindow);
            //containerWindow.RecalculateContainerMinSize();
        }

        public static void Undock(Window3Element source, Vector2 position)
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
                Window3Element firstWindow = null;
                for (var i = 0; i < container.ItemCount; i++)
                {
                    var item = container.GetItemAt(i);
                    if (item is not Window3Element window)
                        continue;

                    firstWindow = window;
                    break;
                }
                firstWindow.RestoreTab();
                firstWindow.SetViewActive(true);

                for (var i = 0; i < container.ItemCount; i++)
                {
                    var item = container.GetItemAt(i);
                    if (item == firstWindow)
                        continue;

                    if (item is not Window3Element window)
                        continue;

                    window.SetViewActive(false);
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
        private static bool TryDockToParent(Window3Element source, Item target, Edge edge)
        {
            return true;
            //if (target._containerWindow._viewHolder.InnerElement is not ContainerElement splitLayout)
            //    return false;

            //var insertIndex = EdgeToInsertIndex(edge);
            //var targetIndex = splitLayout.IndexOf(target);

            //var sourceSize = target.Size * SplitSizeFactor;
            //var targetSize = target.Size - sourceSize;

            //target.SetSize(targetSize);

            //source._containerWindow = target._containerWindow;
            //source.ApplyRootWindow(target._rootWindow);
            //source.IsInteractable = false;
            //source.SetSize(sourceSize);
            //splitLayout.InsertChild(targetIndex + insertIndex, source);
            //targetIndex = target._containerWindow._childWindows.IndexOf(target);
            //target._containerWindow._childWindows.Insert(targetIndex + insertIndex, source);

            //target._containerWindow.RecalculateContainerMinSize();

            //return true;
        }
    }
}
