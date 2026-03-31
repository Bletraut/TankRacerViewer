using System;
using System.Collections.Generic;
using System.Diagnostics;

using ComposableUi.Elements.DropDownList;

using Microsoft.Xna.Framework;

using TankRacerViewer.Core;

namespace ComposableUi
{
    public sealed class DropDownListElement : DropDownListElement<DropDownListTextItemElement>
    {
        public DropDownListElement(Vector2? size = default,
            float maxHeight = default,
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
        public const float DefaultMaxListHeight = 400;

        // Static.
        public static readonly Vector2 DefaultSize = new(10, 26);

        public static readonly Vector2 DefaultButtonSize = new(26);
        public static readonly Vector2 DefaultButtonIconSize = new(10);

        public float MaxListHeight { get; set; }

        public SpriteElement Background { get; }
        public IconButtonElement OpenButton { get; }
        public SpriteElement ContentBackground { get; }

        public TItem CurrentItem { get; private set; }
        public int CurrentItemIndex { get; private set; } = EmptyItemIndex;

        private readonly List<TItem> _items;
        public IReadOnlyList<TItem> Items { get; }

        public event ElementEventHandler<TItem, int> ItemSelected;

        private readonly ExpandedElement _overlayInputInterceptorParent;
        private readonly PointerInputHandlerElement _overlayInputInterceptor;

        private readonly ContainerElement _itemContainer;
        private readonly ColumnLayout _itemLayout;

        private readonly ExpandedElement _itemPreviewParent;

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

            OpenButton = new IconButtonElement(
                size: DefaultButtonSize,
                iconSize: DefaultButtonIconSize,
                iconSkin: StandardSkin.DownArrowIcon
            );
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
            _overlayInputInterceptor.PointerSecondaryDown += OnOverlayInputInterceptorPointerDown;
            _overlayInputInterceptor.ScrollWheel += OnOverlayInputInterceptorScrollWheel;
            _overlayInputInterceptor.HorizontalScrollWheel += OnOverlayInputInterceptorScrollWheel;

            _itemLayout = new ColumnLayout(
                sizeMainAxisToContent: true,
                sizeCrossAxisToContent: true
            );
            _itemContainer = new ContainerElement(
                children: [
                    new ExpandedElement(
                        innerElement: new PointerInputHandlerElement(
                            innerElement: ContentBackground
                        )
                    ),
                    new ExpandedElement(
                        leftPadding: DefaultPaddings,
                        rightPadding: DefaultPaddings,
                        topPadding: DefaultPaddings,
                        bottomPadding: DefaultPaddings,
                        innerElement: new ScrollViewElement(
                            content: _itemLayout
                        )
                    )
                ]
            )
            {
                Pivot = Alignment.TopLeft
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

        public void SelectItem(int index)
        {
            if (CurrentItemIndex == index)
                return;

            CurrentItem?.SetSelected(false);

            var hasItem = index >= 0 && index < _items.Count;
            if (!hasItem)
            {
                CurrentItemIndex = EmptyItemIndex;
                CurrentItem = null;

                return;
            }

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
            _itemPreview.Clone(item);

            ItemSelected?.Invoke(item, _items.IndexOf(item));
        }

        private void ShowItemList()
        {
            var boundingRectangle = BoundingRectangle;

            var contentSize = _itemLayout.CalculatePreferredSize();
            var size = new Vector2()
            {
                X = boundingRectangle.Size.X,
                Y = MathF.Min(MaxListHeight, contentSize.Y + DefaultPaddings * 2)
            };
            _itemContainer.Size = size;

            var position = new Vector2(boundingRectangle.Left,
                boundingRectangle.Bottom - DefaultPaddings);
            var fallbackPosition = new Vector2()
            {
                X = boundingRectangle.Left,
                Y = boundingRectangle.Top - size.Y + DefaultPaddings
            };

            Root.ShowInOverlay(_overlayInputInterceptorParent,
                Vector2.Zero, Vector2.Zero, false, false);
            Root.ShowInOverlay(_itemContainer, position, fallbackPosition);
        }

        private void HideItemList()
        {
            _itemContainer.IsEnabled = false;
            _overlayInputInterceptorParent.IsEnabled = false;
        }

        private void OnItemSelected(TItem item)
        {
            SelectItem(_items.IndexOf(item));
            HideItemList();
        }

        private void OnOpenButtonPointerClick(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            ShowItemList();
        }

        private void OnOverlayInputInterceptorPointerDown(PointerInputHandlerElement sender,
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
