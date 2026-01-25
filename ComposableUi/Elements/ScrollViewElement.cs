using Microsoft.Xna.Framework;

using System.Diagnostics;

namespace ComposableUi
{
    public class ScrollViewElement : ContainerElement
    {
        public static readonly Vector2 DefaultSize = new(300, 300);

        private Element _content;
        public Element Content
        {
            get => _content;
            set
            {
                if (_content == value) 
                    return;

                _contentParent.RemoveChild(_content);

                _content = value;

                if (_content != null)
                    _contentParent.AddChild(_content);

                RefreshScrollBars();
                OnStateChanged();
            }
        }

        public ScrollBarElement HorizontalScrollBar { get; }
        public ScrollBarElement VerticalScrollBar { get; }

        private readonly Element _view;
        private readonly AlignmentElement _contentParent;

        private readonly ExpandedElement _horizontalScrollBarParent;
        private readonly ExpandedElement _verticalScrollBarParent;

        private Vector2 _minContentPosition;

        public ScrollViewElement(Vector2? size = default)
        {
            var inputHandler = new PointerInputHandlerElement(
                innerElement: new SpriteElement(
                    skin: StandardSkin.RectanglePanel));
            inputHandler.PointerClick += (_, _) =>
            {
                ApplySize(Vector2.Floor(Size * 1.1f));
            };
            inputHandler.PointerSecondaryClick += (_, _) =>
            {
                ApplySize(Vector2.Floor(Size / 1.1f));
            };
            AddChild(new ExpandedElement(
                innerElement: inputHandler));

            _contentParent = new AlignmentElement(
                alignmentFactor: Alignment.TopLeft,
                pivot: Alignment.TopLeft);

            _view = new ClipMaskElement(
                innerElement: new HolderElement(
                    innerElement: new ExpandedElement(
                        expandWidth: false,
                        expandHeight: false,
                        innerElement: _contentParent)
                    )
                );
            AddChild(new ExpandedElement(_view));

            HorizontalScrollBar = new HorizontalScrollBarElement();
            HorizontalScrollBar.ProgressValueChanged += OnHorizontalScrollValueChanged;
            VerticalScrollBar = new VerticalScrollBarElement();
            VerticalScrollBar.ProgressValueChanged += OnVerticalScrollValueChanged;

            _horizontalScrollBarParent = new ExpandedElement(
                expandHeight: false,
                rightPadding: ScrollBarElement.DefaultCrossAxisSize,
                innerElement: new AlignmentElement(
                    alignmentFactor: Alignment.BottomCenter,
                    pivot: Alignment.BottomCenter,
                    innerElement: HorizontalScrollBar)
                );
            AddChild(_horizontalScrollBarParent);

            _verticalScrollBarParent = new ExpandedElement(
                expandWidth: false,
                bottomPadding: ScrollBarElement.DefaultCrossAxisSize,
                innerElement: new AlignmentElement(
                    alignmentFactor: Alignment.MiddleRight,
                    pivot: Alignment.MiddleRight,
                    innerElement: VerticalScrollBar)
                );
            AddChild(_verticalScrollBarParent);

            Content = new SpriteElement(
                size: new Vector2(200, 300),
                skin: StandardSkin.ContentPanel);

            ApplySize(size ?? DefaultSize);
            //_contentParent.SizeChanged += OnContentSizeChanged;
        }

        private void RefreshScrollBars()
        {
            Debug.WriteLine("RefreshScrollBars");

            if (Content == null)
            {
                HorizontalScrollBar.IsEnabled = false;
                VerticalScrollBar.IsEnabled = false;

                return;
            }

            var deltaSize = Content.Size - _view.Size;
            var extraScrollBarsSize = new Vector2()
            {
                X = deltaSize.Y > 0 ? VerticalScrollBar.CrossAxisSize : 0,
                Y = deltaSize.X > 0 ? HorizontalScrollBar.CrossAxisSize : 0,
            };
            _minContentPosition = -Vector2.Max(deltaSize + extraScrollBarsSize, Vector2.Zero);
            _contentParent.Offset = Vector2.Clamp(_contentParent.Offset, _minContentPosition, Vector2.Zero);

            if (_minContentPosition.X < 0)
            {
                HorizontalScrollBar.IsEnabled = true;
                HorizontalScrollBar.ProgressValue = _contentParent.Offset.X / _minContentPosition.X;

                _verticalScrollBarParent.BottomPadding = HorizontalScrollBar.CrossAxisSize;
            }
            else
            {
                HorizontalScrollBar.IsEnabled = false;
                _verticalScrollBarParent.BottomPadding = 0;
            }

            if (_minContentPosition.Y < 0)
            {
                VerticalScrollBar.IsEnabled = true;
                VerticalScrollBar.ProgressValue = _contentParent.Offset.Y / _minContentPosition.Y;

                _horizontalScrollBarParent.RightPadding = VerticalScrollBar.CrossAxisSize;
            }
            else
            {
                VerticalScrollBar.IsEnabled = false;
                _horizontalScrollBarParent.RightPadding = 0;
            }
        }

        private void OnHorizontalScrollValueChanged(Element sender, float value)
        {
            if (Content == null)
                return;

            _contentParent.Offset = _contentParent.Offset with
            {
                X = _minContentPosition.X * value
            };
        }

        private void OnVerticalScrollValueChanged(Element sender, float value)
        {
            if (Content == null)
                return;

            _contentParent.Offset = _contentParent.Offset with
            {
                Y = _minContentPosition.Y * value
            };
        }

        private void OnContentSizeChanged(Element sender)
        {
            RefreshScrollBars();
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            RefreshScrollBars();
        }
    }
}
