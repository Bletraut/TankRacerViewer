using System;
using System.Diagnostics;
using System.Reflection;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class ScrollViewElement : ContainerElement, IHierarchyWheelScrollable
    {
        public static readonly Vector2 DefaultSize = new(300, 300);

        public Element Content
        {
            get => _contentParent.InnerElement;
            set => _contentParent.InnerElement = value;
        }

        private bool _sizeToContentWidth;
        public bool SizeToContentWidth
        {
            get => _sizeToContentWidth;
            set => SetAndChangeState(ref _sizeToContentWidth, value);
        }

        private bool _sizeToContentHeight;
        public bool SizeToContentHeight
        {
            get => _sizeToContentHeight;
            set => SetAndChangeState(ref _sizeToContentHeight, value);
        }

        private bool _expandContentWidth;
        public bool ExpandContentWidth
        {
            get => _expandContentWidth;
            set => SetAndChangeState(ref _expandContentWidth, value);
        }

        private bool _expandContentHeight;
        public bool ExpandContentHeight
        {
            get => _expandContentHeight;
            set => SetAndChangeState(ref _expandContentHeight, value);
        }

        public float ScrollWheelMultiplier { get; set; }

        public SpriteElement Background { get; }
        public ScrollBarElement HorizontalScrollBar { get; }
        public ScrollBarElement VerticalScrollBar { get; }

        private readonly Element _view;
        private readonly ExpandedElement _viewExpanded;
        private readonly AlignmentElement _contentParent;
        private readonly ExpandedElement _contentExpanded;

        private readonly ExpandedElement _horizontalScrollBarParent;
        private readonly ExpandedElement _verticalScrollBarParent;

        private readonly Element _bottomRightPlug;

        private readonly PointerInputHandlerElement _scrollInputHandler;

        private Vector2 _minContentPosition;
        private Vector2 _progressValue;

        private HierarchyWheelScrollSolver _hierarchyWheelScrollSolver;
        private readonly Action<Vector2, int> _wheelScrollAction;

        public ScrollViewElement(Vector2? size = default,
            Element content = default,
            bool sizeToContentWidth = default,
            bool sizeToContentHeight = default,
            bool expandContentWidth = default,
            bool expandContentHeight = default,
            float scrollWheelMultiplier = 0.5f)
        {
            Size = size ?? DefaultSize;
            SizeToContentWidth = sizeToContentWidth;
            SizeToContentHeight = sizeToContentHeight;
            ExpandContentWidth = expandContentWidth;
            ExpandContentHeight = expandContentHeight;
            ScrollWheelMultiplier = scrollWheelMultiplier;

            Background = new SpriteElement();
            AddChild(new ExpandedElement(Background));

            _contentParent = new AlignmentElement(
                alignmentFactor: Alignment.TopLeft,
                pivot: Alignment.TopLeft);

            _contentExpanded = new ExpandedElement(
                expandWidth: ExpandContentWidth,
                expandHeight: ExpandContentHeight,
                innerElement: _contentParent);

            _view = new ClipMaskElement(
                innerElement: new HolderElement(
                    innerElement: _contentExpanded)
                );
            _viewExpanded = new ExpandedElement(_view);
            AddChild(_viewExpanded);

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

            _scrollInputHandler = new PointerInputHandlerElement(blockInput: false);
            _scrollInputHandler.ScrollWheel += OnScrollWheel;
            _scrollInputHandler.HorizontalScrollWheel += OnHorizontalScrollWheel;
            AddChild(new ExpandedElement(_scrollInputHandler));

            Content = content;

            _wheelScrollAction = ScrollContent;
        }

        public void ScrollHorizontalToFitBounds(Rectangle bounds)
            => HorizontalScrollBar.ProgressValue = CalculateFitBoundsProgressValue(bounds).X;

        public void ScrollVerticalToFitBounds(Rectangle bounds)
            => VerticalScrollBar.ProgressValue = CalculateFitBoundsProgressValue(bounds).Y;

        private Vector2 CalculateFitBoundsProgressValue(Rectangle bounds)
        {
            if (Content is null)
                return _progressValue;

            var localPosition = Vector2.Transform(bounds.Location.ToVector2(),
                Content.GlobalInverseTransformationMatrix);

            var minProgressValue = localPosition / -_minContentPosition;
            var maxProgressValue = (localPosition + bounds.Size.ToVector2() - _view.Size) / -_minContentPosition;

            var progressValue = Vector2.Min(minProgressValue, _progressValue);
            progressValue = Vector2.Max(maxProgressValue, progressValue);

            return progressValue;
        }

        private void RefreshContentAndScrollBarsVisibility()
        {
            if (Content is null)
            {
                HorizontalScrollBar.IsEnabled = false;
                VerticalScrollBar.IsEnabled = false;
                _bottomRightPlug.IsEnabled = false;

                _viewExpanded.RightPadding = 0;
                _viewExpanded.BottomPadding = 0;

                return;
            }

            var preferredContentSize = Content.CalculatePreferredSize();
            var contentSize = new Vector2()
            {
                X = ExpandContentWidth ? Size.X : preferredContentSize.X,
                Y = ExpandContentHeight ? Size.Y : preferredContentSize.Y
            };

            var deltaSize = contentSize - Size;
            var extraDeltaSize = deltaSize + new Vector2()
            {
                X = ExpandContentWidth ? 0 : VerticalScrollBar.CrossAxisSize,
                Y = ExpandContentHeight ? 0 : HorizontalScrollBar.CrossAxisSize
            };

            HorizontalScrollBar.IsEnabled = deltaSize.X > 0
                || (extraDeltaSize.X > 0 && deltaSize.Y > 0);
            VerticalScrollBar.IsEnabled = deltaSize.Y > 0
                || (extraDeltaSize.Y > 0 && deltaSize.X > 0);

            _bottomRightPlug.IsEnabled = HorizontalScrollBar.IsEnabled
                && VerticalScrollBar.IsEnabled;

            var rightPadding = VerticalScrollBar.IsEnabled
                ? VerticalScrollBar.CrossAxisSize
                : 0;
            var bottomPadding = HorizontalScrollBar.IsEnabled
                ? HorizontalScrollBar.CrossAxisSize
                : 0;

            _viewExpanded.RightPadding = rightPadding;
            _viewExpanded.BottomPadding = bottomPadding;
            _horizontalScrollBarParent.RightPadding = rightPadding;
            _verticalScrollBarParent.BottomPadding = bottomPadding;

            _minContentPosition = deltaSize + new Vector2(rightPadding, bottomPadding);
            _minContentPosition = -Vector2.Max(_minContentPosition, Vector2.Zero);
            ApplyContentOffset(_contentParent.Offset);
        }

        private void ApplyContentOffset(Vector2 offset)
        {
            _contentParent.Offset = Vector2.Clamp(offset, _minContentPosition, Vector2.Zero);
            _progressValue.X = HorizontalScrollBar.IsEnabled
                ? _contentParent.Offset.X / _minContentPosition.X
                : 0;
            _progressValue.Y = VerticalScrollBar.IsEnabled
                ? _contentParent.Offset.Y / _minContentPosition.Y
                : 0;
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

        private void ScrollContent(Vector2 axis, int delta)
        {
            ApplyContentOffset(_contentParent.Offset + axis * delta);
            RefreshScrollBarsButtons();
        }

        private void ScrollContentIfPossible(Vector2 axis, int delta)
        {
            var isAnyScrollButtonPressed = HorizontalScrollBar.Button.IsPressed
                || VerticalScrollBar.Button.IsPressed;
            if (isAnyScrollButtonPressed)
                return;

            var axisProgress = Vector2.Dot(_progressValue, _progressValue * axis);
            var limit = delta < 0 ? 1 : 0;
            if (axisProgress == limit)
                return;

            var multipliedDelta = (int)(delta * ScrollWheelMultiplier);
            if (_hierarchyWheelScrollSolver is not null)
            {
                _hierarchyWheelScrollSolver.AddScrollIntent(axis, multipliedDelta, _wheelScrollAction);
            }
            else
            {
                ScrollContent(axis, multipliedDelta);
            }
        }

        public override void Rebuild(Vector2 size, bool excludeChildren)
        {
            Size = size;
            RefreshContentAndScrollBarsVisibility();

            base.Rebuild(size, excludeChildren);

            if (HorizontalScrollBar.IsEnabled || VerticalScrollBar.IsEnabled)
                RefreshScrollBarsButtons();
        }

        void IHierarchyWheelScrollable.AttachSolver(HierarchyWheelScrollSolver solver)
        {
            _hierarchyWheelScrollSolver = solver;
        }

        private void OnHorizontalScrollValueChanged(Element sender, float value)
        {
            if (Content is null)
                return;

            _contentParent.Offset = _contentParent.Offset with
            {
                X = _minContentPosition.X * value
            };
        }

        private void OnVerticalScrollValueChanged(Element sender, float value)
        {
            if (Content is null)
                return;

            _contentParent.Offset = _contentParent.Offset with
            {
                Y = _minContentPosition.Y * value
            };
        }

        private void OnScrollWheel(Element sender, PointerScrollEvent pointerEvent)
        {
            ScrollContentIfPossible(Vector2.UnitY, pointerEvent.Delta);
        }

        private void OnHorizontalScrollWheel(Element sender, PointerScrollEvent pointerEvent)
        {
            ScrollContentIfPossible(Vector2.UnitX, pointerEvent.Delta);
        }
    }
}
