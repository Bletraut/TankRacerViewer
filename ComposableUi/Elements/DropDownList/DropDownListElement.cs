using System;
using System.Collections.Generic;

using ComposableUi.Elements.DropDownList;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public sealed class DropDownListElement : DropDownListElement<DropDownListTextItemElement>
    {
        public DropDownListElement(Vector2? size = default,
            float maxHeight = DefaultMaxListHeight,
            IEnumerable<DropDownListTextItemElement> items = default)
            : base(size,
                  maxHeight,
                  items)
        {
        }
    }

    public class DropDownListElement<TItem> : ClipMaskElement
        where TItem : Element, IDropDownListItem<TItem>
    {
        public const int EmptyItemIndex = -1;

        public const float DefaultPaddings = 2;
        public const float DefaultMaxListHeight = 150;

        // Static.
        public static readonly Vector2 DefaultSize = new(100, 24);

        public static readonly Vector2 DefaultButtonSize = new(24);
        public static readonly Vector2 DefaultButtonIconSize = new(10);

        public float MaxListHeight { get; set; }

        public SpriteElement Background { get; }
        public ContentButtonElement OpenButton { get; }
        public SpriteElement ContentBackground { get; }

        public TItem CurrentItem { get; private set; }
        public int CurrentItemIndex { get; private set; } = EmptyItemIndex;

        private readonly List<TItem> _items;
        public IReadOnlyList<TItem> Items { get; }

        public event ElementEventHandler<TItem, int> ItemSelected;

        private readonly ExpandedElement _overlayInputInterceptorParent;
        private readonly PointerInputHandlerElement _overlayInputInterceptor;

        private readonly ContainerElement _itemContainer;
        private readonly ExpandedElement _itemScrollViewParent;
        private readonly ScrollViewElement _itemScrollView;
        private readonly ColumnLayout _itemLayout;

        private readonly ExpandedElement _itemPreviewParent;

        private bool _canOpenItemList = true;

        private TItem _itemPreview;

        public DropDownListElement(Vector2? size = default,
            float maxListHeight = DefaultMaxListHeight,
            IEnumerable<TItem> items = default)
        {
            _items = [];
            Items = _items.AsReadOnly();

            Size = size ?? DefaultSize;
            MaxListHeight = maxListHeight;

            Background = new SpriteElement(
                skin: StandardSkin.TextField
            );

            OpenButton = new ContentButtonElement(
                iconSize: DefaultButtonIconSize,
                iconSkin: StandardSkin.DownArrowIcon,
                normalSkin: StandardSkin.SoftDarkPixel,
                hoverSkin: StandardSkin.SelectionSoftDarkPixel,
                pressedSkin: StandardSkin.HoverSoftDarkPixel,
                disabledSkin: StandardSkin.SelectionSoftDarkPixel,
                normalButtonColor: Color.White,
                hoverButtonColor: Color.White,
                pressedButtonColor: Color.White
            );
            OpenButton.Text.IsEnabled = false;
            OpenButton.ContentLayout.LeftPadding = OpenButton.ContentLayout.RightPadding = 6;
            OpenButton.PointerUp += OnOpenButtonPointerUp;
            OpenButton.PointerClick += OnOpenButtonPointerClick;

            _itemPreviewParent = new ExpandedElement(
                leftPadding: DefaultPaddings,
                rightPadding: DefaultPaddings,
                topPadding: DefaultPaddings,
                bottomPadding: DefaultPaddings
            );

            InnerElement = new ContainerElement(
                size: Size,
                children: [
                    new ExpandedElement(
                        innerElement: Background
                    ),
                    _itemPreviewParent,
                    new ExpandedElement(
                        topPadding: DefaultPaddings,
                        bottomPadding: DefaultPaddings,
                        expandWidth: false,
                        innerElement: new AlignmentElement(
                            alignmentFactor: Alignment.MiddleRight,
                            offset: new Vector2(-DefaultPaddings, 0),
                            pivot: Alignment.MiddleRight,
                            innerElement: OpenButton
                        )
                    )
                ]
            );

            ContentBackground = new SpriteElement(
                skin: StandardSkin.TextField
            );

            _overlayInputInterceptor = new PointerInputHandlerElement(
                blockInput: false
            );
            _overlayInputInterceptorParent = new ExpandedElement(_overlayInputInterceptor);
            _overlayInputInterceptor.PointerDown += OnOverlayInputInterceptorPointerDown;
            _overlayInputInterceptor.PointerSecondaryDown += OnOverlayInputInterceptorPointerSecondaryDown;
            _overlayInputInterceptor.ScrollWheel += OnOverlayInputInterceptorScrollWheel;
            _overlayInputInterceptor.HorizontalScrollWheel += OnOverlayInputInterceptorScrollWheel;

            _itemLayout = new ColumnLayout(
                topPadding: DefaultPaddings,
                bottomPadding: DefaultPaddings,
                sizeMainAxisToContent: true,
                sizeCrossAxisToContent: true,
                expandChildrenCrossAxis: true
            );
            _itemScrollView = new ScrollViewElement(
                expandingContentWidthMode: ScrollViewElement.ExpandingMode.ExpandToFit,
                content: _itemLayout
            );
            _itemScrollViewParent = new ExpandedElement(
                leftPadding: DefaultPaddings,
                rightPadding: DefaultPaddings,
                innerElement: _itemScrollView
            );
            _itemContainer = new ContainerElement(
                children: [
                    new ExpandedElement(
                        innerElement: new PointerInputHandlerElement(
                            innerElement: ContentBackground
                        )
                    ),
                    _itemScrollViewParent
                ]
            )
            {
                Pivot = Alignment.TopLeft,
                IsEnabled = false
            };

            if (items is not null)
            {
                foreach ( var item in items )
                    AddItem(item);
            }
        }

        public void AddItem(TItem item)
        {
            if (_items.Contains(item))
                return;

            _items.Add(item);
            _itemLayout.AddChild(item);

            item.Selected += OnItemSelected;
        }

        public void RemoveItem(TItem item)
        {
            if (_items.Remove(item))
            {
                _itemLayout.RemoveChild(item);
                item.Selected -= OnItemSelected;

                if (CurrentItem == item)
                    SelectItem(0);
            }
        }

        public void ClearItems()
        {
            foreach (var item in _items)
            {
                _itemLayout.RemoveChild(item);
                item.Selected -= OnItemSelected;
            }

            _items.Clear();
            SelectItem(0);
        }

        public void SelectItem(int index)
        {
            var hasItem = index >= 0 && index < _items.Count;
            if (!hasItem)
            {
                CurrentItemIndex = EmptyItemIndex;
                CurrentItem?.SetSelected(false);
                CurrentItem = null;

                if (_itemPreview is not null)
                    _itemPreview.IsEnabled = false;

                return;
            }

            if (CurrentItemIndex == index)
                return;

            CurrentItem?.SetSelected(false);

            var item = _items[index];
            item.SetSelected(true);

            CurrentItemIndex = index;
            CurrentItem = _items[CurrentItemIndex];

            if (_itemPreview is null)
            {
                _itemPreview = CurrentItem.CreateEmpty();
                _itemPreview.IsSelectable = false;
                _itemPreviewParent.AddChild(_itemPreview);
            }
            _itemPreview.IsEnabled = true;
            _itemPreview.Clone(item);

            ItemSelected?.Invoke(item, _items.IndexOf(item));
        }

        private void ShowItemList()
        {
            if (_items.Count <= 0)
                return;

            var visibleRectangle = ClipMask ?? BoundingRectangle;

            var contentSize = _itemLayout.CalculatePreferredSize();
            var contentWidth = contentSize.X + _itemScrollViewParent.TotalHorizontalPaddings;
            var extraHeight =  contentWidth > visibleRectangle.Size.X
                ? _itemScrollView.HorizontalScrollBar.CrossAxisSize
                : 0;
            var size = new Vector2()
            {
                X = visibleRectangle.Size.X,
                Y = MathF.Min(MaxListHeight, contentSize.Y + extraHeight)
            };
            _itemContainer.Size = size;

            var position = new Vector2(visibleRectangle.Left,
                visibleRectangle.Bottom - DefaultPaddings);
            var fallbackPosition = new Vector2()
            {
                X = visibleRectangle.Left,
                Y = visibleRectangle.Top - size.Y + DefaultPaddings
            };

            Layer = BuiltInLayer.Overlay;

            Root.ShowInOverlay(_overlayInputInterceptorParent,
                Vector2.Zero, Vector2.Zero, false, false);
            Root.ShowInOverlay(_itemContainer, position, fallbackPosition);
        }

        private void HideItemList()
        {
            _itemContainer.IsEnabled = false;
            _overlayInputInterceptorParent.IsEnabled = false;

            Layer = BuiltInLayer.Main;
        }

        private void OnItemSelected(TItem item)
        {
            SelectItem(_items.IndexOf(item));
            HideItemList();
        }

        private void OnOpenButtonPointerUp(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            if (OpenButton.IsHover)
                return;

            _canOpenItemList = true;
        }

        private void OnOpenButtonPointerClick(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            if (!_canOpenItemList)
            {
                _canOpenItemList = true;
                return;
            }

            ShowItemList();
        }

        private void OnOverlayInputInterceptorPointerDown(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            if (OpenButton.IsHover)
                _canOpenItemList = false;

            HideItemList();
        }

        private void OnOverlayInputInterceptorPointerSecondaryDown(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            HideItemList();
        }

        private void OnOverlayInputInterceptorScrollWheel(PointerInputHandlerElement sender,
            PointerScrollEvent pointerEvent)
        {
            HideItemList();
        }
    }
}
