using System.Collections.Generic;

namespace ComposableUi
{
    public interface IKeyboardInputHandler
    {
        void OnKeyboardInput(in KeyboardInputEvent keyboardEvent);
    }

    public readonly struct KeyboardInputEvent(
        IReadOnlyList<char> typedCharacters,
        bool isShiftPressed,
        bool isCtrlPressed,
        bool isBackspaceDown,
        bool isDeleteDown,
        bool isLeftArrowDown,
        bool isRightArrowDown,
        bool isUpArrowDown,
        bool isDownArrowDown,
        bool isHomeDown,
        bool isEndDown,
        bool isReturnDown,
        bool isSelectAllDown,
        bool isCopyDown,
        bool isPasteDown,
        bool isCutDown,
        IClipboardProvider clipboardProvider)
    {
        public readonly IReadOnlyList<char> TypedCharacters = typedCharacters;
        public readonly bool IsShiftPressed = isShiftPressed;
        public readonly bool IsCtrlPressed = isCtrlPressed;
        public readonly bool IsBackspaceDown = isBackspaceDown;
        public readonly bool IsDeleteDown = isDeleteDown;
        public readonly bool IsLeftArrowDown = isLeftArrowDown;
        public readonly bool IsRightArrowDown = isRightArrowDown;
        public readonly bool IsUpArrowDown = isUpArrowDown;
        public readonly bool IsDownArrowDown = isDownArrowDown;
        public readonly bool IsHomeDown = isHomeDown;
        public readonly bool IsEndDown = isEndDown;
        public readonly bool IsReturnDown = isReturnDown;
        public readonly bool IsSelectAllDown = isSelectAllDown;
        public readonly bool IsCopyDown = isCopyDown;
        public readonly bool IsPasteDown = isPasteDown;
        public readonly bool IsCutDown = isCutDown;
        public readonly IClipboardProvider ClipboardProvider = clipboardProvider;
    }
}
