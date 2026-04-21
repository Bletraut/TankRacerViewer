using System.Collections.Generic;

namespace ComposableUi
{
    public sealed class WindowPlaceHolderElement : WindowNodeElement<WindowContainerElement>
    {
        // Static.
        private static readonly Stack<WindowPlaceHolderElement> _pool = new();

        // Pool.
        internal static WindowPlaceHolderElement Rent()
        {
            if (!_pool.TryPop(out var placeHolder))
                placeHolder = new WindowPlaceHolderElement();

            placeHolder.IsEnabled = true;
            placeHolder.SetSelected(true);

            return placeHolder;
        }

        internal static void Return(WindowPlaceHolderElement placeHolder)
        {
            placeHolder.IsEnabled = false;
            placeHolder.ApplyContainer(null);
            placeHolder.ApplyRootContainer(null);

            _pool.Push(placeHolder);
        }

        // Class.
        public WindowPlaceHolderElement()
        {
            ViewHolder.InnerElement = new SpriteElement(
                skin: StandardSkin.SolidDarkPixel
            );
        }

        internal override void SetSelected(bool value)
        {
            base.SetSelected(value);
            ViewHolder.InnerElement.IsEnabled = value;
        }
    }
}
