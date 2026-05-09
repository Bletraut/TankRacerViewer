using System.Collections.Generic;

namespace ComposableUi
{
    public interface IKeyboardInputProvider
    {
        IReadOnlyList<char> TypedCharacters { get; }

        bool IsShiftPressed { get; }
        bool IsCtrlPressed { get; }

        bool IsBackspaceDown { get; }
        bool IsDeleteDown { get; }
        bool IsLeftArrowDown { get; }
        bool IsRightArrowDown { get; }
        bool IsUpArrowDown { get; }
        bool IsDownArrowDown { get; }
        bool IsHomeDown { get; }
        bool IsEndDown { get; }
        bool IsReturnDown { get; }

        bool IsSelectAllDown { get; }
        bool IsCopyDown { get; }
        bool IsPasteDown { get; }
        bool IsCutDown { get; }
    }
}
