using Microsoft.Xna.Framework;

using System;
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

                OnStateChanged();
            }
        }

        public ScrollBarElement HorizontalScrollBar { get; }
        public ScrollBarElement VerticalScrollBar { get; }

        private readonly Element _view;
        private readonly AlignmentElement _contentParent;

        private readonly ExpandedElement _horizontalScrollBarParent;
        private readonly ExpandedElement _verticalScrollBarParent;

        private readonly Element _bottomRightPlug;

        private Vector2 _minContentPosition;
        private Vector2 _progressValue;

        public ScrollViewElement(Vector2? size = default,
            Element content = default)
        {
            Size = size ?? DefaultSize;

            var inputHandler = new PointerInputHandlerElement(
                innerElement: new SpriteElement(
                    skin: StandardSkin.RectanglePanel));
            inputHandler.PointerClick += (_, _) =>
            {
                Size = Size + new Vector2(50);
            };
            inputHandler.PointerSecondaryClick += (_, _) =>
            {
                Size = Size - new Vector2(50);
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

            _bottomRightPlug = new ExpandedElement(
                expandWidth: false,
                expandHeight: false,
                innerElement: new AlignmentElement(
                    alignmentFactor: Alignment.BottomRight,
                    pivot: Alignment.BottomRight,
                    innerElement: new SpriteElement(
                        size: new Vector2(ScrollBarElement.DefaultCrossAxisSize),
                        skin: StandardSkin.SolidDarkPixel)
                    )
                );
            AddChild(_bottomRightPlug);

            Content = content;
        }

        private void RefreshContentAndScrollBarsVisibility()
        {
            if (Content == null)
            {
                HorizontalScrollBar.IsEnabled = false;
                VerticalScrollBar.IsEnabled = false;

                return;
            }

            var deltaSize = Content.Size - _view.Size;
            var extraDeltaSize = deltaSize + new Vector2(VerticalScrollBar.CrossAxisSize,
                HorizontalScrollBar.CrossAxisSize);

            HorizontalScrollBar.IsEnabled = deltaSize.X > 0
                || (extraDeltaSize.X > 0 && deltaSize.Y > 0);
            VerticalScrollBar.IsEnabled = deltaSize.Y > 0
                || (extraDeltaSize.Y > 0 && deltaSize.X > 0);

            _bottomRightPlug.IsEnabled = HorizontalScrollBar.IsEnabled
                && VerticalScrollBar.IsEnabled;

            _minContentPosition = new Vector2()
            {
                X = VerticalScrollBar.IsEnabled ? extraDeltaSize.X : deltaSize.X,
                Y = HorizontalScrollBar.IsEnabled ? extraDeltaSize.Y : deltaSize.Y
            };
            _minContentPosition = -Vector2.Max(_minContentPosition, Vector2.Zero);
            _contentParent.Offset = Vector2.Clamp(_contentParent.Offset, _minContentPosition, Vector2.Zero);

            if (HorizontalScrollBar.IsEnabled)
            {
                _verticalScrollBarParent.BottomPadding = HorizontalScrollBar.CrossAxisSize;
                _progressValue.X = _contentParent.Offset.X / _minContentPosition.X;
            }
            else
            {
                _verticalScrollBarParent.BottomPadding = 0;
                _progressValue.X = 0;
            }

            if (VerticalScrollBar.IsEnabled)
            {
                _horizontalScrollBarParent.RightPadding = VerticalScrollBar.CrossAxisSize;
                _progressValue.Y = _contentParent.Offset.Y / _minContentPosition.Y;
            }
            else
            {
                _horizontalScrollBarParent.RightPadding = 0;
                _progressValue.Y = 0;
            }
        }

        private void RefreshScrollBarsButtons()
        {
            RefreshScrollBarButtonMainAxisSize(HorizontalScrollBar);
            RefreshScrollBarButtonMainAxisSize(VerticalScrollBar);
        }

        private void RefreshScrollBarButtonMainAxisSize(ScrollBarElement scrollBar)
        {
            if (!scrollBar.IsEnabled)
                return;

            var fillFactor = _view.Size / (_view.Size - _minContentPosition);
            var mainAxisFillFactor = Vector2.Dot(scrollBar.MainAxis, fillFactor * scrollBar.MainAxis);

            var maxButtonSize = scrollBar.Button.Parent.Size;
            var maxButtonMainAxisSize = Vector2.Dot(scrollBar.MainAxis, maxButtonSize * scrollBar.MainAxis);

            var targetButtonMainAxisSize = MathF.Max(ScrollBarElement.DefaultCrossAxisSize,
                maxButtonMainAxisSize * mainAxisFillFactor);

            scrollBar.Button.Size = targetButtonMainAxisSize * scrollBar.MainAxis + maxButtonSize * scrollBar.CrossAxis;
            scrollBar.ProgressValue = Vector2.Dot(scrollBar.MainAxis, _progressValue * scrollBar.MainAxis);
        }

        public override void ApplySize(Vector2 size)
        {
            base.ApplySize(size);

            RefreshContentAndScrollBarsVisibility();
            base.ApplySize(size);

            if (HorizontalScrollBar.IsEnabled || VerticalScrollBar.IsEnabled)
            {
                RefreshScrollBarsButtons();
                base.ApplySize(size);
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
    }
}
