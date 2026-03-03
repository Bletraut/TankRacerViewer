using System;
using System.Collections.Generic;

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

        // Docking.
        public static void DockTo(Window3Element source, Window3Element target, Edge edge)
        {
            //DetachFromParent(source, Vector2.Zero);

            //var splitDirection = EdgeToSplitDirection(edge);

            //var shouldAttachToParent = target._containerWindow?._splitDirection == splitDirection;
            //if (shouldAttachToParent)
            //{
            //    if (TryAttachToParent(source, target, edge))
            //        return;
            //}

            //var insertIndex = EdgeToInsertIndex(edge);
            //var splitLayout = splitDirection is SplitDirection.Horizontal
            //    ? GetRow()
            //    : GetColumn();

            //var isTabbed = target._containerWindow is not null
            //    && target._containerWindow._compositionType is CompositionType.Tabbed;
            //if (isTabbed)
            //    target = target._containerWindow;

            //var containerWindow = GetContainerWindow();
            //containerWindow._compositionType = CompositionType.Adjacent;
            //containerWindow._splitDirection = splitDirection;
            //containerWindow._viewHolder.PropagateToInnerElementChildren = false;
            //containerWindow._viewHolder.InnerElement = splitLayout;
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
            //        var index = parentSplitLayout.IndexOf(target);
            //        parentSplitLayout.InsertChild(index, containerWindow);
            //    }

            //    var targetIndex = target._containerWindow._childWindows.IndexOf(target);
            //    target._containerWindow._childWindows.Remove(target);
            //    target._containerWindow._childWindows.Insert(targetIndex, containerWindow);
            //}
            //else
            //{
            //    target.Parent.AddChild(containerWindow);
            //}

            //var sourceSize = containerWindow.Size * SplitSizeFactor;
            //var targetSize = containerWindow.Size - sourceSize;
            //containerWindow._childWindows ??= [];

            //target._containerWindow = containerWindow;
            //target.IsInteractable = false;
            //target.SetSize(targetSize);
            //splitLayout.AddChild(target);
            //containerWindow._childWindows.Add(target);

            //source._containerWindow = containerWindow;
            //source.IsInteractable = false;
            //source.SetSize(sourceSize);
            //splitLayout.InsertChild(splitLayout.IndexOf(target) + insertIndex, source);
            //containerWindow._childWindows.Insert(insertIndex, source);

            //containerWindow.ApplyRootWindow(target._rootWindow);
            //containerWindow.RecalculateContainerMinSize();
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
            //var containerWindow = source._containerWindow;
            //if (containerWindow is null)
            //    return;

            //var parent = containerWindow._rootWindow?.Parent ?? containerWindow.Parent;
            //parent.AddChild(source);
            //containerWindow._childWindows.Remove(source);
            //source._containerWindow = null;
            //source.ApplyRootWindow(null);
            //source.IsInteractable = true;
            //source.Position = position;
            //// For inserted tab.
            //source.BlockInput = true;
            //source._viewHolder.IsEnabled = true;
            //source._tabRow.AddChild(source.Tab);

            //if (containerWindow._compositionType == CompositionType.Tabbed)
            //{
            //    Window2Element firstWindow = null;
            //    foreach (var child in containerWindow._childWindows)
            //    {
            //        if (child != source)
            //        {
            //            firstWindow = child;
            //            break;
            //        }
            //    }
            //    firstWindow.BlockInput = true;
            //    firstWindow._viewHolder.IsEnabled = true;
            //    firstWindow._tabRow.AddChild(firstWindow.Tab);

            //    foreach (var child in containerWindow._childWindows)
            //    {
            //        if (child == firstWindow)
            //            continue;

            //        if (child == source)
            //            continue;

            //        child.BlockInput = false;
            //        child._viewHolder.IsEnabled = false;
            //        firstWindow._tabRow.AddChild(child.Tab);
            //    }
            //}

            //if (containerWindow._childWindows.Count == 1)
            //{
            //    var lastWindow = containerWindow._childWindows[0];
            //    containerWindow._childWindows.Clear();

            //    lastWindow.SetSize(containerWindow.Size);
            //    lastWindow._containerWindow = containerWindow._containerWindow;
            //    lastWindow.ApplyRootWindow(containerWindow._rootWindow);
            //    lastWindow.IsInteractable = containerWindow.IsInteractable;
            //    // For inserted tab.
            //    lastWindow.BlockInput = true;
            //    lastWindow._viewHolder.IsEnabled = true;
            //    lastWindow._tabRow.AddChild(lastWindow.Tab);

            //    if (containerWindow._containerWindow is not null)
            //    {
            //        var parentContainerWindow = containerWindow._containerWindow;

            //        if (parentContainerWindow._viewHolder.InnerElement is ContainerElement parentSplitLayout)
            //        {
            //            var index = parentSplitLayout.IndexOf(containerWindow);
            //            parentSplitLayout.InsertChild(index, lastWindow);
            //        }

            //        var containerIndex = parentContainerWindow._childWindows.IndexOf(containerWindow);
            //        parentContainerWindow._childWindows.Remove(containerWindow);
            //        parentContainerWindow._childWindows.Insert(containerIndex, lastWindow);
            //        parentContainerWindow.RecalculateContainerMinSize();
            //    }
            //    else
            //    {
            //        var localPosition = containerWindow.LocalPosition;
            //        containerWindow.Parent.AddChild(lastWindow);
            //        lastWindow.LocalPosition = localPosition - containerWindow.PivotOffset + lastWindow.PivotOffset;
            //    }

            //    containerWindow._compositionType = CompositionType.None;
            //    containerWindow._splitDirection = SplitDirection.None;
            //    containerWindow._containerWindow = null;
            //    containerWindow.ApplyRootWindow(null);
            //    containerWindow._viewHolder.InnerElement = null;
            //    containerWindow.Parent?.RemoveChild(containerWindow);

            //    ReleaseWindow(containerWindow);
            //}
            //else
            //{
            //    containerWindow.RecalculateContainerMinSize();
            //}
        }

        // Inner resizing.
        private static void IncreaseSizeInHierarchyIfPossible(Item source, DockingMode dockingMode,
            Vector2 axis, float axisDelta, float delta)
        {
            if (source.Container is null)
                return;

            if (source.Container.DockingMode != dockingMode)
            {
                IncreaseSizeInHierarchyIfPossible(source.Container, dockingMode, axis, axisDelta, delta);
                return;
            }

            var target = source;
            var targetIndex = source.Container.IndexOfItem(target);
            var neighborIndex = targetIndex + MathF.Sign(axisDelta) * MathF.Sign(delta);

            var hasNeighbor = neighborIndex >= 0
                && neighborIndex < source.Container.ItemCount;
            if (!hasNeighbor)
            {
                IncreaseSizeInHierarchyIfPossible(source.Container, dockingMode, axis, axisDelta, delta);
                return;
            }

            // Should decrease size or increase size neighbor window.
            if (delta < 0)
            {
                target = source.Container.GetItemAt(neighborIndex);
                delta *= -1;
            }

            IncreaseSizeIfPossible(target, axis, delta, axisDelta > 0);
        }

        private static void IncreaseSizeIfPossible(Item target, Vector2 axis,
            float delta, bool expandForward)
        {
            var increaseDelta = 0f;
            var decreaseDelta = delta;

            var container = target.Container;
            var targetIndex = container.IndexOfItem(target);

            var neighborIndex = targetIndex + (expandForward ? 1 : -1);
            while (neighborIndex >= 0 && neighborIndex < container.ItemCount)
            {
                var neighbor = container.GetItemAt(neighborIndex);
                neighborIndex += expandForward ? 1 : -1;

                var size = neighbor.Size;

                var axisSize = Vector2.Dot(axis, size);
                var minAxisSize = Vector2.Dot(axis, neighbor.MinSize);
                if (axisSize <= minAxisSize)
                    continue;

                var newAxisSize = MathF.Max(minAxisSize, axisSize - decreaseDelta);
                var newSize = size * (Vector2.One - axis) + new Vector2(newAxisSize) * axis;
                neighbor.SetSize(newSize);

                var sizeDelta = axisSize - newAxisSize;
                increaseDelta += sizeDelta;
                decreaseDelta -= sizeDelta;

                if (decreaseDelta <= 0)
                    break;
            }

            target.SetSize(target.Size + new Vector2(increaseDelta));
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
    }
}
