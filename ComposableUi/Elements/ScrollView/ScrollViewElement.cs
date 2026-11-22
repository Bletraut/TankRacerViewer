using System;
using System.Diagnostics;

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

        private ExpandingMode _expandingContentWidthMode;
        public ExpandingMode ExpandingContentWidthMode
        {
            get => _expandingContentWidthMode;
            set => SetAndChangeState(ref _expandingContentWidthMode, value);
        }

        private ExpandingMode _expandingContentHeightMode;
        public ExpandingMode ExpandingContentHeightMode
        {
            get => _expandingContentHeightMode;
            set => SetAndChangeState(ref _expandingContentHeightMode, value);
        }

        public float ScrollWheelMultiplier { get; set; }

        private float? _changedHorizontalProgressValue;
        public float NormalizedHorizontalProgressValue
        {
            get => _changedHorizontalProgressValue ?? _normalizedProgressValue.X;
            set => _changedHorizontalProgressValue = float.Clamp(value, 0, 1);
        }

        private float? _changedVerticalProgressValue;
        public float NormalizedVerticalProgressValue
        {
            get => _changedVerticalProgressValue ?? _normalizedProgressValue.Y;
            set => _changedVerticalProgressValue = float.Clamp(value, 0, 1);
        }

        public SpriteElement Background { get; }
        public ScrollBarElement HorizontalScrollBar { get; }
        public ScrollBarElement VerticalScrollBar { get; }

        private readonly Element _view;
        private readonly ExpandedElement _viewExpanded;
        private readonly AlignmentElement _contentParent;
        private readonly ExpandedElement _contentExpanded;

        private readonly ExpandedElement _horizontalScrollBarParent;
        private readonly ExpandedElement _verticalScrollBarParent;

        private readonly ExpandedElement _bottomRightPlug;

        private readonly PointerInputHandlerElement _scrollInputHandler;

        private Vector2 _minContentPosition;
        private Vector2 _normalizedProgressValue;

        private HierarchyWheelScrollSolver _hierarchyWheelScrollSolver;
        private readonly Action<Vector2, int> _wheelScrollAction;

        public ScrollViewElement(Vector2? size = default,
            Element content = default,
            ExpandingMode expandingContentWidthMode = default,
            ExpandingMode expandingContentHeightMode = default,
            float scrollWheelMultiplier = 0.5f)
        {
            Size = size ?? DefaultSize;
            ExpandingContentWidthMode = expandingContentWidthMode;
            ExpandingContentHeightMode = expandingContentHeightMode;
            ScrollWheelMultiplier = scrollWheelMultiplier;

            Background = new SpriteElement(
                drawMode: DrawMode.Sliced
            );
            AddChild(new ExpandedElement(Background));

            _contentParent = new AlignmentElement(
                alignmentFactor: Alignment.TopLeft,
                pivot: Alignment.TopLeft
            );

            _contentExpanded = new ExpandedElement(
                expandWidth: ExpandingContentWidthMode is ExpandingMode.FillParent,
                expandHeight: ExpandingContentHeightMode is ExpandingMode.FillParent,
                innerElement: _contentParent
            );

            _view = new ClipMaskElement(
                innerElement: new HolderElement(
                    innerElement: _contentExpanded
                )
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
                    innerElement: HorizontalScrollBar
                )
            );
            AddChild(_horizontalScrollBarParent);

            _verticalScrollBarParent = new ExpandedElement(
                expandWidth: false,
                bottomPadding: ScrollBarElement.DefaultCrossAxisSize,
                innerElement: new AlignmentElement(
                    alignmentFactor: Alignment.MiddleRight,
                    pivot: Alignment.MiddleRight,
                    innerElement: VerticalScrollBar
                )
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
                        spriteSource: BuiltInSprite.SolidDarkPixel,
                        drawMode: DrawMode.Sliced
                    )
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
            => HorizontalScrollBar.NormalizedProgressValue = CalculateFitBoundsProgressValue(bounds).X;

        public void ScrollVerticalToFitBounds(Rectangle bounds)
            => VerticalScrollBar.NormalizedProgressValue = CalculateFitBoundsProgressValue(bounds).Y;

        public ContentSizingResult CalculatePreferredContentSizing(Vector2 size)
        {
            if (Content is null)
                return new ContentSizingResult();

            var shouldExpandWidth = ExpandingContentWidthMode is ExpandingMode.FillParent;
            var shouldExpandHeight = ExpandingContentHeightMode is ExpandingMode.FillParent;

            var isContentSizeFullyConstrained = shouldExpandWidth && shouldExpandHeight;
            if (isContentSizeFullyConstrained)
            {
                return new ContentSizingResult(size,
                    true, true, false, false);
            }

            var contentSizingResult = CalculateContentSizing(size, size,
                shouldExpandWidth, shouldExpandHeight);

            var shouldRefineContentSizing = contentSizingResult.WasWidthExpanded ^ contentSizingResult.WasHeightExpanded;
            if (shouldRefineContentSizing)
            {
                var refinedContentSizingResult = CalculateContentSizing(contentSizingResult.Size, size,
                    contentSizingResult.WasWidthExpanded, contentSizingResult.WasHeightExpanded);

                var shouldRecalculateContentSizing = contentSizingResult.WasWidthExpanded ^ contentSizingResult.WasHeightExpanded
                    && (contentSizingResult.HasHorizontalOverflow != refinedContentSizingResult.HasHorizontalOverflow
                    || contentSizingResult.HasVerticalOverflow != refinedContentSizingResult.HasVerticalOverflow);
                if (shouldRecalculateContentSizing)
                {
                    contentSizingResult = CalculateContentSizing(size, size,
                        refinedContentSizingResult.WasWidthExpanded, refinedContentSizingResult.WasHeightExpanded);
                }
                else
                {
                    contentSizingResult = refinedContentSizingResult;
                }
            }

            return contentSizingResult;
        }

        private ContentSizingResult CalculateContentSizing(Vector2 targetSize, Vector2 boundsSize,
            bool shouldExpandWidth, bool shouldExpandHeight)
        {
            float? exactWidth = shouldExpandWidth ? targetSize.X : null;
            float? exactHeight = shouldExpandHeight ? targetSize.Y : null;
            var preferredContentSize = Content.CalculatePreferredSize(exactWidth, exactHeight);

            if (shouldExpandWidth)
                preferredContentSize.X = targetSize.X;
            if (shouldExpandHeight)
                preferredContentSize.Y = targetSize.Y;

            var hasHorizontalOverflow = preferredContentSize.X > boundsSize.X;
            var hasVerticalOverflow = preferredContentSize.Y > boundsSize.Y;

            var availableContentSize = boundsSize - new Vector2()
            {
                X = hasVerticalOverflow
                    ? VerticalScrollBar.CrossAxisSize
                    : 0,
                Y = hasHorizontalOverflow
                    ? HorizontalScrollBar.CrossAxisSize
                    : 0
            };

            var isContentSizeUnconstrained = ExpandingContentWidthMode is ExpandingMode.None
                && ExpandingContentHeightMode is ExpandingMode.None;
            if (isContentSizeUnconstrained)
            {
                hasHorizontalOverflow |= preferredContentSize.X > availableContentSize.X;
                hasVerticalOverflow |= preferredContentSize.Y > availableContentSize.Y;

                return new ContentSizingResult(preferredContentSize,
                    false, false, hasHorizontalOverflow, hasVerticalOverflow);
            }

            var wasWidthExpanded = shouldExpandWidth;
            var wasHeightExpanded = shouldExpandHeight;

            var contentWidth = preferredContentSize.X;
            switch (ExpandingContentWidthMode)
            {
                case ExpandingMode.FillParent:
                    contentWidth = availableContentSize.X;
                    break;
                case ExpandingMode.ExpandToFit:
                    contentWidth = MathF.Max(preferredContentSize.X, availableContentSize.X);
                    wasWidthExpanded |= contentWidth > preferredContentSize.X;
                    break;
            }
            var contentHeight = preferredContentSize.Y;
            switch (ExpandingContentHeightMode)
            {
                case ExpandingMode.FillParent:
                    contentHeight = availableContentSize.Y;
                    break;
                case ExpandingMode.ExpandToFit:
                    contentHeight = MathF.Max(preferredContentSize.Y, availableContentSize.Y);
                    wasHeightExpanded |= contentHeight > preferredContentSize.Y;
                    break;
            }

            hasHorizontalOverflow = contentWidth > availableContentSize.X;
            hasVerticalOverflow = contentHeight > availableContentSize.Y;

            return new ContentSizingResult(new Vector2(contentWidth, contentHeight),
                wasWidthExpanded, wasHeightExpanded, hasHorizontalOverflow, hasVerticalOverflow);
        }

        private Vector2 CalculateFitBoundsProgressValue(Rectangle bounds)
        {
            if (Content is null)
                return _normalizedProgressValue;

            var localPosition = Vector2.Transform(bounds.Location.ToVector2(),
                Content.GlobalInverseTransformationMatrix);

            var minProgressValue = localPosition / -_minContentPosition;
            var maxProgressValue = (localPosition + bounds.Size.ToVector2() - _view.Size) / -_minContentPosition;

            var progressValue = Vector2.Min(minProgressValue, _normalizedProgressValue);
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

            var contentSizingResult = CalculatePreferredContentSizing(Size);
            var deltaSize = contentSizingResult.Size - Size;

            HorizontalScrollBar.IsEnabled = contentSizingResult.HasHorizontalOverflow;
            VerticalScrollBar.IsEnabled = contentSizingResult.HasVerticalOverflow;

            _contentExpanded.ExpandWidth = contentSizingResult.WasWidthExpanded;
            _contentExpanded.ExpandHeight = contentSizingResult.WasHeightExpanded;

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

            ApplyChangedNormalizedProgressValues();
            ApplyContentOffset(_contentParent.Offset);
        }

        private void ApplyChangedNormalizedProgressValues()
        {
            _contentParent.Offset = _contentParent.Offset with
            {
                X = _changedHorizontalProgressValue.HasValue
                    ? _minContentPosition.X * _changedHorizontalProgressValue.Value
                    : _contentParent.Offset.X,
                Y = _changedVerticalProgressValue.HasValue
                    ? _minContentPosition.Y * _changedVerticalProgressValue.Value
                    : _contentParent.Offset.Y
            };
            _changedHorizontalProgressValue = null;
            _changedVerticalProgressValue = null;
        }

        private void ApplyContentOffset(Vector2 offset)
        {
            _contentParent.Offset = Vector2.Clamp(offset, _minContentPosition, Vector2.Zero);
            _normalizedProgressValue.X = HorizontalScrollBar.IsEnabled
                ? _contentParent.Offset.X / _minContentPosition.X
                : 0;
            _normalizedProgressValue.Y = VerticalScrollBar.IsEnabled
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
            var mainAxisFillFactor = Vector2.Dot(scrollBar.MainAxis, fillFactor);

            var maxButtonSize = scrollBar.Button.Parent.Size;
            var maxButtonMainAxisSize = Vector2.Dot(scrollBar.MainAxis, maxButtonSize);

            var targetButtonMainAxisSize = MathF.Max(ScrollBarElement.DefaultCrossAxisSize,
                maxButtonMainAxisSize * mainAxisFillFactor);

            scrollBar.Button.Size = targetButtonMainAxisSize * scrollBar.MainAxis + maxButtonSize * scrollBar.CrossAxis;
            scrollBar.NormalizedProgressValue = Vector2.Dot(scrollBar.MainAxis, _normalizedProgressValue);
        }

        private void ScrollContent(Vector2 axis, int delta)
        {
            ApplyChangedNormalizedProgressValues();
            ApplyContentOffset(_contentParent.Offset + axis * delta);

            RefreshScrollBarsButtons();
        }

        private void WheelScrollContentIfPossible(Vector2 axis, int delta)
        {
            var isAnyScrollButtonPressed = HorizontalScrollBar.Button.IsPressed
                || VerticalScrollBar.Button.IsPressed;
            if (isAnyScrollButtonPressed)
                return;

            var axisProgress = Vector2.Dot(axis, _normalizedProgressValue);
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
            WheelScrollContentIfPossible(Vector2.UnitY, pointerEvent.Delta);
        }

        private void OnHorizontalScrollWheel(Element sender, PointerScrollEvent pointerEvent)
        {
            WheelScrollContentIfPossible(Vector2.UnitX, pointerEvent.Delta);
        }

        public readonly record struct ContentSizingResult(Vector2 Size,
            bool WasWidthExpanded, bool WasHeightExpanded,
            bool HasHorizontalOverflow, bool HasVerticalOverflow);

        public enum ExpandingMode
        {
            None,
            FillParent,
            ExpandToFit
        }
    }
}
