using System.Collections.Generic;

namespace ComposableUi
{
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
        internal CompositionType CompositionType { get; private set; }

        private readonly List<WindowNodeElement<WindowContainerElement>> _items = [];

        private void ClearItems()
        {
            _items.Clear();
        }
    }
}
