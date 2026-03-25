using ComposableUi;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using static System.Net.Mime.MediaTypeNames;

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

        private readonly AspectRatioFitterElement _3dViewer;
        private readonly ButtonElement _aspectRationModeButton;
        private readonly PointerInputHandlerElement _inputArea;

        private readonly TextElement _text;
        private readonly ExpandedElement _textViewer;

        private readonly Sprite _sprite;
        private readonly SpriteElement _textureView;
        private readonly AspectRatioFitterElement _textureViewer;

        private bool _isPointerInInputArea;
        private bool _isPointerPressedInInputArea;

        public ViewerWindow(GraphicsDevice graphicsDevice) : base("Viewer")
        {
            _graphicsDevice = graphicsDevice;

            RenderContext = new RenderContextElement(_graphicsDevice);
            _3dViewer = new AspectRatioFitterElement(
                aspectRatioMode: AspectRatioMode.FitInParent,
                aspectRatio: RenderContext.AspectRatio,
                innerElement: RenderContext
            );
            ContentContainer.AddChild(_3dViewer);

            _text = new TextElement(
                textAlignmentFactor: Alignment.TopLeft,
                sizeToTextWidth: true,
                sizeToTextHeight: true,
                color: Color.Black
            );
            _textViewer = new ExpandedElement(
                propagateToInnerElementChildren: true,
                innerElement: new ContainerElement(
                    children: [
                        new SpriteElement(
                            skin: StandardSkin.WhitePixel,
                            color: Color.LightGoldenrodYellow
                        ),
                        new ScrollViewElement(
                            content: _text
                        )
                    ]
                )
            );
            ContentContainer.AddChild(_textViewer);

            _sprite = new Sprite();
            _textureView = new SpriteElement(
                sprite: _sprite,
                sizeToSource: true,
                drawMode: DrawMode.Simple
            );
            _textureViewer = new AspectRatioFitterElement(
                aspectRatioMode: AspectRatioMode.FitInParent,
                aspectRatio: 1,
                innerElement: _textureView
            );
            ContentContainer.AddChild(_textureViewer);

            _aspectRationModeButton = InsertButton(1, null, StandardSkin.HoverRectangleButton);
            _aspectRationModeButton.PointerClick += OnAspectRationModeButtonClicked;

            _inputArea = new PointerInputHandlerElement(blockInput: false);
            ContentContainer.AddChild(new ExpandedElement(_inputArea));

            _inputArea.PointerEnter += OnInputAreaPointerEnter;
            _inputArea.PointerLeave += OnInputAreaPointerLeave;
            _inputArea.PointerDown += OnInputAreaPointerDown;
            _inputArea.PointerUp += OnInputAreaPointerUp;

            Show3DViewer();
        }

        public void Show3DViewer()
        {
            _textViewer.IsEnabled = false;
            _textureViewer.IsEnabled = false;

            _3dViewer.IsEnabled = true;
            _aspectRationModeButton.IsEnabled = true;
        }

        public void ShowTextureViewer(Texture2D texture)
        {
            _3dViewer.IsEnabled = false;
            _aspectRationModeButton.IsEnabled = false;
            _textViewer.IsEnabled = false;

            _sprite.Texture = texture;
            _sprite.SourceRectangle = texture.Bounds;
            _textureViewer.IsEnabled = true;
            _textureViewer.AspectRatio = (float)texture.Width / texture.Height;
        }

        public void ShowTextViewer(string text)
        {
            _3dViewer.IsEnabled = false;
            _aspectRationModeButton.IsEnabled = false;
            _textureViewer.IsEnabled = false;

            _textViewer.IsEnabled = true;
            _text.Text = text;
        }

        public void HideViewer()
        {
            _3dViewer.IsEnabled = false;
            _aspectRationModeButton.IsEnabled = false;
            _textureViewer.IsEnabled = false;
            _textViewer.IsEnabled = false;
        }

        private void OnAspectRationModeButtonClicked(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            _3dViewer.AspectRatioMode = _3dViewer.AspectRatioMode is AspectRatioMode.FitInParent
                ? AspectRatioMode.EnvelopeParent
                : AspectRatioMode.FitInParent;
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
