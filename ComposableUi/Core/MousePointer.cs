using Microsoft.Xna.Framework.Input;

namespace ComposableUi
{
    public sealed class MousePointer : IPointer
    {
        void IPointer.SetCursor(PointerCursor cursor)
        {
            var mouseCursor = cursor switch
            {
                PointerCursor.Arrow => MouseCursor.Arrow,
                PointerCursor.SizeNS => MouseCursor.SizeNS,
                PointerCursor.SizeWE => MouseCursor.SizeWE,
                PointerCursor.SizeNWSE => MouseCursor.SizeNWSE,
                PointerCursor.SizeNESW => MouseCursor.SizeNESW,
                _ => MouseCursor.Arrow,
            };

            Mouse.SetCursor(mouseCursor);
        }
    }
}
