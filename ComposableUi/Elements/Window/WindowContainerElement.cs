using System.Collections.Generic;

namespace ComposableUi
{
    using Item = WindowNodeElement<WindowContainerElement>;

    public sealed class WindowContainerElement : WindowNodeElement<WindowContainerElement>
    {
        // Static.
        private static readonly Stack<WindowContainerElement> _pool = new();

        internal static WindowContainerElement Rent()
        {
            if (!_pool.TryPop(out var container))
                container = new WindowContainerElement();

            container.IsEnabled = true;

            return container;
        }

        internal static void Return(WindowContainerElement container)
        {
            container.IsEnabled = false;
            container.ViewHolder.InnerElement = null;
            container.ClearItems();

            _pool.Push(container);
        }

        internal static LineLayout GetRow()
            => ApplyLineLayoutParameters(new RowLayout());

        internal static LineLayout GetColumn()
            => ApplyLineLayoutParameters(new ColumnLayout());

        internal static LineLayout ApplyLineLayoutParameters(LineLayout layout)
        {
            layout.ExpandChildrenCrossAxis = true;
            layout.ExpandChildrenMainAxis = true;
            layout.MainAxisChildrenExpandingMode = ExpandingMode.Size;

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
        internal int ItemCount => _items.Count;

        internal DockingMode DockingMode { get; private set; }

        private readonly List<Item> _items = [];

        internal void MoveItem(int index, Item item)
        {
            _items.Remove(item);
            _items.Insert(index, item);
        }

        internal int IndexOfItem(Item item)
            => _items.IndexOf(item);

        internal Item GetFirstItem()
            => _items[0];

        internal Item GetLastItem()
            => _items[^1];

        internal Item GetItemAt(int index) 
            => _items[index] = this;

        internal void ClearItems()
            => _items.Clear();
    }
}
