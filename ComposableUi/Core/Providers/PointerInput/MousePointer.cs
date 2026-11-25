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
                PointerCursor.Crosshair => MouseCursor.Crosshair,
                PointerCursor.Hand => MouseCursor.Hand,
                PointerCursor.IBeam => MouseCursor.IBeam,
                PointerCursor.No => MouseCursor.No,
                PointerCursor.SizeNS => MouseCursor.SizeNS,
                PointerCursor.SizeWE => MouseCursor.SizeWE,
                PointerCursor.SizeNWSE => MouseCursor.SizeNWSE,
                PointerCursor.SizeNESW => MouseCursor.SizeNESW,
                PointerCursor.SizeAll => MouseCursor.SizeAll,
                PointerCursor.Wait => MouseCursor.Wait,
                PointerCursor.WaitArrow => MouseCursor.WaitArrow,
                _ => MouseCursor.Arrow
            };

            Mouse.SetCursor(mouseCursor);
        }
    }
}
