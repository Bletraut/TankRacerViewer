using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public interface IPointerInputHandler
    {
        public Rectangle InteractionRectangle { get; }

        public bool BlockInput { get; set; }
        public bool IsInteractable { get; set; }

        public void OnScrollWheel(in PointerScrollEvent pointerEvent) { }
        public void OnHorizontalScrollWheel(in PointerScrollEvent pointerEvent) { }

        public void OnPointerEnter(in PointerEvent pointerEvent) { }
        public void OnPointerMove(in PointerEvent pointerEvent) { }
        public void OnPointerLeave(in PointerEvent pointerEvent) { }

        public void OnPointerDown(in PointerEvent pointerEvent) { }
        public void OnPointerUp(in PointerEvent pointerEvent) { }

        public void OnPointerSecondaryDown(in PointerEvent pointerEvent) { }
        public void OnPointerSecondaryUp(in PointerEvent pointerEvent) { }

        public void OnPointerDrag(in PointerDragEvent pointerEvent) { }

        public void OnPointerClick(in PointerEvent pointerEvent) { }
        public void OnPointerSecondaryClick(in PointerEvent pointerEvent) { }

        public void OnFocusChanged(in PointerFocusEvent pointerEvent);
    }

    public readonly struct PointerEvent(IPointer pointer, Point position,
        bool isPrimaryButtonPressed, bool isSecondaryButtonPressed)
    {
        public readonly IPointer Pointer = pointer;
        public readonly Point Position = position;
        public readonly bool IsPrimaryButtonPressed = isPrimaryButtonPressed;
        public readonly bool IsSecondaryButtonPressed = isSecondaryButtonPressed;
    }

    public readonly struct PointerScrollEvent(IPointer pointer, Point position,
        bool isPrimaryButtonPressed, bool isSecondaryButtonPressed,
        int delta)
    {
        public readonly IPointer Pointer = pointer;
        public readonly Point Position = position;
        public readonly bool IsPrimaryButtonPressed = isPrimaryButtonPressed;
        public readonly bool IsSecondaryButtonPressed = isSecondaryButtonPressed;
        public readonly int Delta = delta;
    }

    public readonly struct PointerDragEvent(IPointer pointer, Point position,
        bool isPrimaryButtonPressed, bool isSecondaryButtonPressed,
        Point delta)
    {
        public readonly IPointer Pointer = pointer;
        public readonly Point Position = position;
        public readonly bool IsPrimaryButtonPressed = isPrimaryButtonPressed;
        public readonly bool IsSecondaryButtonPressed = isSecondaryButtonPressed;
        public readonly Point Delta = delta;
    }

    public readonly struct PointerFocusEvent(IPointer pointer, Point position,
        bool isPrimaryButtonPressed, bool isSecondaryButtonPressed,
        bool isFocused)
    {
        public readonly IPointer Pointer = pointer;
        public readonly Point Position = position;
        public readonly bool IsPrimaryButtonPressed = isPrimaryButtonPressed;
        public readonly bool IsSecondaryButtonPressed = isSecondaryButtonPressed;
        public readonly bool IsFocused = isFocused;
    }
}
