using ComposableUi;

using Microsoft.Xna.Framework.Graphics;

namespace TankRacerViewer.Core
{
    public sealed class ViewerWindow : WindowElement
    {
        public bool IsInputAvailable 
            => IsFocused && !IsTabPressed && !IsDragHandlePressed
            && !IsResizingInternally && !IsResizing
            && (_isPointerInInputArea || _isPointerPressedInInputArea);

        public RenderContextElement RenderContext { get; private set; }

        private readonly GraphicsDevice _graphicsDevice;

        private readonly PointerInputHandlerElement _inputArea;

        private bool _isPointerInInputArea;
        private bool _isPointerPressedInInputArea;

        public ViewerWindow(GraphicsDevice graphicsDevice) : base("Viewer")
        {
            _graphicsDevice = graphicsDevice;

            RenderContext = new RenderContextElement(_graphicsDevice);
            ContentContainer.AddChild(new ExpandedElement(RenderContext));

            _inputArea = new PointerInputHandlerElement(blockInput: false);
            ContentContainer.AddChild(new ExpandedElement(_inputArea));

            _inputArea.PointerEnter += OnInputAreaPointerEnter;
            _inputArea.PointerLeave += OnInputAreaPointerLeave;
            _inputArea.PointerDown += OnInputAreaPointerDown;
            _inputArea.PointerUp += OnInputAreaPointerUp;
        }

        private void OnInputAreaPointerEnter(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            _isPointerInInputArea = true;
        }

        private void OnInputAreaPointerLeave(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            _isPointerInInputArea = false;
        }

        private void OnInputAreaPointerDown(PointerInputHandlerElement sender, PointerEvent arguments)
        {
            _isPointerPressedInInputArea = true;
        }

        private void OnInputAreaPointerUp(PointerInputHandlerElement sender, PointerEvent arguments)
        {
            _isPointerPressedInInputArea = false;
        }
    }
}
