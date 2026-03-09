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
            placeHolder.SetFocus(true);

            return placeHolder;
        }

        internal static void Return(WindowPlaceHolderElement placeHolder)
        {
            placeHolder.IsEnabled = false;
            placeHolder.ApplyContainer(null);
            placeHolder.ApplyRoot(null);

            _pool.Push(placeHolder);
        }

        // Class.
        public WindowPlaceHolderElement() 
        {
            ViewHolder.InnerElement = new SpriteElement(
                skin: StandardSkin.SolidDarkPixel
            );
        }

        internal override void SetFocus(bool value)
        {
            base.SetFocus(value);
            ViewHolder.InnerElement.IsEnabled = value;
        }
    }
}
