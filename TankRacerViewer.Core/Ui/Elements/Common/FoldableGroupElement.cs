using System.Collections.Generic;
using System.Diagnostics;

using ComposableUi;

using Microsoft.Xna.Framework;

namespace TankRacerViewer.Core
{
    public class FoldableGroupElement : SizedToContentHolderElement
    {
        public const float DefaultIndent = 30;
        public const float DefaultSpacing = 4;

        public const float DefaultBackgroundPadding = 10_000;

        public readonly Vector2 DefaultFoldButtonSize = new(12);
        public readonly Vector2 DefaultIconSize = new(12);

        public readonly Color DefaultNormalBackgroundColor = Color.Transparent;
        public readonly Color DefaultHoverBackgroundColor = Color.Gray;
        public readonly Color DefaultSelectedBackgroundColor = Color.BlueViolet;

        public float Indent
        {
            get => _itemLayout.LeftPadding;
            set => _itemLayout.LeftPadding = value;
        }

        private bool _isFolded;
        public bool IsFolded
        {
            get => _isFolded;
            set
            {
                _isFolded = value;

                _itemLayout.IsEnabled = !_isFolded;
                RefreshFoldButtonSkin();
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                RefreshBackgroundVisualState();
            }
        }

        public PointerInputHandlerElement ClickInputHandler { get; }
        public PointerInputHandlerElement HoverInputHandler { get; }
        public SpriteElement Background { get; }
        public ButtonElement FoldButton { get; }
        public SpriteElement Icon { get; }
        public TextElement Name { get; }

        public RowLayout TitleLayout { get; }

        private readonly List<Element> _items;
        public IReadOnlyList<Element> Items { get; }

        public Color NormalBackgroundColor { get; set; }
        public Color HoverBackgroundColor { get; set; }
        public Color SelectedBackgroundColor { get; set; }

        private readonly Element _foldButtonPlaceholder;
        private readonly ColumnLayout _groupColumn;
        private readonly RowLayout _titleRow;
        private readonly ColumnLayout _itemLayout;

        private bool _isHover;

        public FoldableGroupElement(Sprite iconSprite = default,
            StandardSkin iconSkin = default,
            string name = default,
            Element content = default,
            bool isFolded = default)
        {
            _items = [];
            Items = _items.AsReadOnly();

            Background = new SpriteElement(
                skin: StandardSkin.WhitePixel,
                color: DefaultNormalBackgroundColor
            );

            ClickInputHandler = new PointerInputHandlerElement(
                innerElement: new ExpandedElement(Background)
            );

            HoverInputHandler = new PointerInputHandlerElement(
                blockInput: false
            );

            HoverInputHandler.PointerEnter += OnHoverInputHandlerPointerEnter;
            HoverInputHandler.PointerLeave += OnHoverInputHandlerPointerLeave;

            FoldButton = new ButtonElement(
                size: DefaultFoldButtonSize,
                normalColor: Color.Black,
                hoverColor: Color.BlanchedAlmond,
                pressedColor: Color.DarkCyan
            );
            _foldButtonPlaceholder = new Element
            {
                Size = FoldButton.Size,
            };

            FoldButton.PointerClick += OnFoldButtonPointerClick;

            Icon = new SpriteElement(
                size: DefaultIconSize,
                sprite: iconSprite,
                skin: iconSkin
            );

            Name = new TextElement(
                textAlignmentFactor: Alignment.MiddleLeft,
                text: name,
                sizeToTextWidth: true,
                sizeToTextHeight: true
            );

            TitleLayout = new RowLayout(
                alignmentFactor: Alignment.MiddleLeft,
                spacing: DefaultSpacing,
                sizeMainAxisToContent: true,
                sizeCrossAxisToContent: true,
                children: [
                    _foldButtonPlaceholder,
                    FoldButton,
                    Icon,
                    Name,
                ]
            );

            _titleRow = new RowLayout(
                alignmentFactor: Alignment.MiddleLeft,
                sizeMainAxisToContent: true,
                sizeCrossAxisToContent: true,
                children: [
                    new LayoutElement(
                        ignoreLayout: true,
                        innerElement: new ExpandedElement(
                            leftPadding: -DefaultBackgroundPadding,
                            rightPadding: -DefaultBackgroundPadding,
                            innerElement: ClickInputHandler
                        )
                    ),
                    TitleLayout,
                    new LayoutElement(
                        ignoreLayout: true,
                        innerElement: new ExpandedElement(
                            leftPadding: -DefaultBackgroundPadding,
                            rightPadding: -DefaultBackgroundPadding,
                            innerElement: HoverInputHandler
                        )
                    ),
                ]
            );

            _itemLayout = new ColumnLayout(
                leftPadding: DefaultIndent,
                sizeMainAxisToContent: true,
                sizeCrossAxisToContent: true
            );

            _groupColumn = new ColumnLayout(
                sizeMainAxisToContent: true,
                sizeCrossAxisToContent: true,
                children: [
                    _titleRow,
                    _itemLayout
                ]
            );

            if (content is not null)
            {
                AddItem(content);
            }
            {
                RefreshFoldButtonVisibility();
            }

            InnerElement = _groupColumn;

            IsFolded = isFolded;
        }

        public void AddItem(Element item)
        {
            if (_items.Contains(item))
                return;

            _items.Add(item);
            _itemLayout.AddChild(item);

            RefreshFoldButtonVisibility();
        }

        public void RemoveItem(Element item)
        {
            if (_items.Remove(item))
                _itemLayout.RemoveChild(item);

            RefreshFoldButtonVisibility();
        }

        private void RefreshFoldButtonSkin()
        {
            var skin = IsFolded 
                ? StandardSkin.RightArrowIcon 
                : StandardSkin.DownArrowIcon;

            FoldButton.NormalSkin = skin;
            FoldButton.HoverSkin = skin;
            FoldButton.PressedSkin = skin;
            FoldButton.DisabledSkin = skin;
        }

        private void RefreshFoldButtonVisibility()
        {
            var isVisible = _items.Count > 0;

            FoldButton.IsEnabled = isVisible;
            _foldButtonPlaceholder.IsEnabled = !isVisible;
            _foldButtonPlaceholder.Size = FoldButton.Size;
        }

        private void RefreshBackgroundVisualState()
        {
            if (IsSelected)
            {
                Background.Color = DefaultSelectedBackgroundColor;
            }
            else
            {
                Background.Color = _isHover 
                    ? DefaultHoverBackgroundColor 
                    : DefaultNormalBackgroundColor;
            }
        }

        private void OnFoldButtonPointerClick(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            IsFolded = !IsFolded;
        }

        private void OnHoverInputHandlerPointerEnter(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            _isHover = true;
            RefreshBackgroundVisualState();
        }

        private void OnHoverInputHandlerPointerLeave(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            _isHover = false;
            RefreshBackgroundVisualState();
        }
    }
}
