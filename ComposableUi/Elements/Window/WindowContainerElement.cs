using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    using Item = WindowNodeElement<WindowContainerElement>;

    public sealed class WindowContainerElement : Item
    {
        // Static.
        private static readonly Stack<WindowContainerElement> _pool = new();

        // Pool.
        internal static WindowContainerElement Rent(DockingMode dockingMode)
        {
            if (!_pool.TryPop(out var container))
                container = new WindowContainerElement();

            container.IsEnabled = true;
            container.DockingMode = dockingMode;

            switch (dockingMode)
            {
                case DockingMode.HorizontalSplit:
                    container.ApplyLayout(GetRow());
                    break;
                case DockingMode.VerticalSplit:
                    container.ApplyLayout(GetColumn());
                    break;
                case DockingMode.Tab:
                    container.ApplyLayout(new ContainerElement());
                    container.ViewHolder.PropagateToInnerElementChildren = true;
                    break;
            }

            return container;
        }

        internal static void Return(WindowContainerElement container)
        {
            container.IsEnabled = false;
            container.DockingMode = DockingMode.None;
            container.ViewHolder.PropagateToInnerElementChildren = false;
            container.ApplyContainer(null);
            container.ApplyRootContainer(null);
            container.ApplyLayout(null);
            container.ClearItems();

            _pool.Push(container);
        }

        // Inner resizing.
        internal static void IncreaseSizeInHierarchyIfPossible(Item source, DockingMode dockingMode,
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

        internal static void IncreaseSizeIfPossible(Item target, Vector2 axis,
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

        // Layout.
        private static LineLayout GetRow()
            => ApplyLineLayoutParameters(new RowLayout());

        private static LineLayout GetColumn()
            => ApplyLineLayoutParameters(new ColumnLayout());

        private static LineLayout ApplyLineLayoutParameters(LineLayout layout)
        {
            layout.ExpandChildrenCrossAxis = true;
            layout.ExpandChildrenMainAxis = true;
            layout.MainAxisChildrenExpandingMode = LineLayout.ExpandingMode.Size;

            layout.AddChild(new LayoutElement(
                ignoreLayout: true,
                innerElement: new ExpandedElement(
                    innerElement: new SpriteElement(
                        skin: StandardSkin.SolidDarkPixel
                    )
                )
            ));

            return layout;
        }

        // Class.
        internal ContainerElement Layout { get; private set; }

        internal int ItemCount => _items.Count;

        internal DockingMode DockingMode { get; private set; }

        private readonly List<Item> _items = [];

        // Methods.
        internal void RecalculateMinSize()
        {
            if (_items.Count == 0)
            {
                MinSize = Vector2.Zero;
                return;
            }

            var totalSize = Vector2.Zero;
            var maxMinSize = Vector2.Zero;

            foreach (var item in _items)
            {
                totalSize += item.MinSize;
                maxMinSize = Vector2.Max(maxMinSize, item.MinSize);
            }

            MinSize = DockingMode switch
            {
                DockingMode.Tab => maxMinSize,
                DockingMode.HorizontalSplit => new Vector2(totalSize.X, maxMinSize.Y),
                DockingMode.VerticalSplit => new Vector2(maxMinSize.X, totalSize.Y),
                _ => throw new NotSupportedException()
            };

            Container?.RecalculateMinSize();
        }

        internal void ReplaceItem(Item oldItem, Item newItem)
        {
            var index = _items.IndexOf(oldItem);
            if (index < 0)
                return;

            var inLayoutIndex = Layout.IndexOf(oldItem);

            RemoveItem(oldItem);
            InsertItem(index, inLayoutIndex, newItem);
        }

        internal void InsertItem(int index, int inLayoutIndex, Item item)
        {
            item.Container?.RemoveItem(item);

            item.ApplyContainer(this);
            item.ApplyRootContainer(RootContainer ?? this);
            item.IsInteractable = false;

            Layout.InsertChild(inLayoutIndex, item);
            _items.Insert(index, item);

            RecalculateMinSize();
        }

        internal void MoveItem(int index, Item item)
        {
            _items.Remove(item);
            _items.Insert(index, item);
        }

        internal void RemoveItem(Item item)
        {
            if (_items.Remove(item))
            {
                item.IsInteractable = true;
                item.ApplyContainer(null);
                item.ApplyRootContainer(null);

                Layout.RemoveChild(item);
            }
        }

        internal int IndexOfItem(Item item)
            => _items.IndexOf(item);

        internal Item GetFirstItem()
            => _items[0];

        internal Item GetLastItem()
            => _items[^1];

        internal Item GetItemAt(int index) 
            => _items[index];

        internal void ClearItems()
            => _items.Clear();

        private void ApplyLayout(ContainerElement layout)
        {
            Layout = layout;
            ViewHolder.InnerElement = layout;
        }

        internal override void ApplyRootContainer(WindowContainerElement root)
        {
            base.ApplyRootContainer(root);

            foreach (var item in _items)
                item.ApplyRootContainer(root ?? this);
        }

        internal override void PrepareReplacementWith(Item node)
        {
            RemoveItem(node);
            base.PrepareReplacementWith(node);
        }
    }
}
