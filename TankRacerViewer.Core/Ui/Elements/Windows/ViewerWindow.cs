using ComposableUi;

using Microsoft.Xna.Framework.Graphics;

namespace TankRacerViewer.Core
{
    public sealed class ViewerWindow : WindowElement
    {
        public bool IsInputAvailable 
            => IsSelected && !IsTabPressed && !IsDragHandlePressed
            && !IsResizingInternally && !IsResizing
            && (_isPointerInInputArea || _isPointerPressedInInputArea);

        public RenderContextElement RenderContext { get; private set; }

        private readonly GraphicsDevice _graphicsDevice;

        private readonly AspectRatioFitterElement _aspectRatioFitter;
        private readonly PointerInputHandlerElement _inputArea;

        private bool _isPointerInInputArea;
        private bool _isPointerPressedInInputArea;

        public ViewerWindow(GraphicsDevice graphicsDevice) : base("Viewer")
        {
            _graphicsDevice = graphicsDevice;

            RenderContext = new RenderContextElement(_graphicsDevice);
            _aspectRatioFitter = new AspectRatioFitterElement(
                aspectRatioMode: AspectRatioMode.FitInParent,
                aspectRatio: RenderContext.AspectRatio,
                innerElement: RenderContext
            );
            ContentContainer.AddChild(_aspectRatioFitter);

            var aspectRatioMode = InsertButton(1, null, StandardSkin.HoverRectangleButton);
            aspectRatioMode.PointerClick += (_, _) =>
            {
                _aspectRatioFitter.AspectRatioMode = _aspectRatioFitter.AspectRatioMode is AspectRatioMode.FitInParent
                    ? AspectRatioMode.EnvelopeParent
                    : AspectRatioMode.FitInParent;
            };

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
