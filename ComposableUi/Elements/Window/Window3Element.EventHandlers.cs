namespace ComposableUi
{
    public partial class Window3Element
    {
        private void OnDragHandlePointerDown(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            BringToFront();
        }

        private void OnDragHandlePointerFixedDrag(PointerInputHandlerElement sender,
            PointerDragEvent pointerEvent)
        {
            if (IsResizingInternally)
                return;

            var root = ResolveRoot();
            root.Position += pointerEvent.Delta.ToVector2();
        }
    }
}
