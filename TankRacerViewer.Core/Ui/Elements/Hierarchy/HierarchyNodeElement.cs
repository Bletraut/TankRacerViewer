using ComposableUi;

using Microsoft.Xna.Framework;

namespace TankRacerViewer.Core
{
    public sealed class HierarchyNodeElement : SizedToContentHolderElement,
        ILazyListItem<HierarchyNodeData>
    {
        public const float DefaultSpacing = 4;

        private readonly float DefaultTitleHorizontalPadding = 4;

        public readonly Vector2 DefaultFoldButtonSize = new(12);
        public readonly Vector2 DefaultIconSize = new(12);

        public readonly Color DefaultNormalBackgroundColor = Color.Transparent;
        public readonly Color DefaultHoverBackgroundColor = Color.Gray;
        public readonly Color DefaultSelectedBackgroundColor = Color.BlueViolet;

        private Color _normalBackgroundColor;
        public Color NormalBackgroundColor
        {
            get => _normalBackgroundColor;
            set
            {
                _normalBackgroundColor = value;
                RefreshBackgroundVisualState();
            }
        }
        private Color _hoverBackgroundColor;
        public Color HoverBackgroundColor
        {
            get => _hoverBackgroundColor;
            set
            {
                _hoverBackgroundColor = value;
                RefreshBackgroundVisualState();
            }
        }
        private Color _selectedBackgroundColor;
        public Color SelectedBackgroundColor
        {
            get => _selectedBackgroundColor;
            set
            {
                _selectedBackgroundColor = value;
                RefreshBackgroundVisualState();
            }
        }

        public float Indent
        {
            get => _titleRow.LeftPadding;
            set => _titleRow.LeftPadding = value;
        }

        public PointerInputHandlerElement ClickInputHandler { get; }
        public PointerInputHandlerElement HoverInputHandler { get; }
        public SpriteElement Background { get; }
        public ButtonElement FoldButton { get; }
        public SpriteElement Icon { get; }
        public TextElement Name { get; }

        public RowLayout TitleLayout { get; }

        public HierarchyNodeData Data { get; private set; }

        public event ElementEventHandler<HierarchyNodeElement> OnClicked;
        public event ElementEventHandler<HierarchyNodeElement> OnFoldButtonClicked;

        private readonly Element _foldButtonPlaceholder;
        private readonly RowLayout _titleRow;

        private bool _isHover;

        public HierarchyNodeElement(Color? normalBackgroundColor = default,
            Color? hoverBackgroundColor = default,
            Color? selectedBackgroundColor = default)
        {
            _normalBackgroundColor = normalBackgroundColor ?? DefaultNormalBackgroundColor;
            _hoverBackgroundColor = hoverBackgroundColor ?? DefaultHoverBackgroundColor;
            _selectedBackgroundColor = selectedBackgroundColor ?? DefaultSelectedBackgroundColor;

            Background = new SpriteElement(
                skin: StandardSkin.WhitePixel,
                color: _normalBackgroundColor
            );

            ClickInputHandler = new PointerInputHandlerElement(
                innerElement: new ExpandedElement(Background)
            );

            ClickInputHandler.PointerClick += OnClickInputHandlerPointerClick;

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
                size: DefaultIconSize
            );

            Name = new TextElement(
                textAlignmentFactor: Alignment.MiddleLeft,
                sizeToTextWidth: true,
                sizeToTextHeight: true
            );

            TitleLayout = new RowLayout(
                alignmentFactor: Alignment.MiddleLeft,
                leftPadding: DefaultTitleHorizontalPadding,
                rightPadding: DefaultTitleHorizontalPadding,
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
                //leftPadding: DefaultIndent,
                sizeMainAxisToContent: true,
                sizeCrossAxisToContent: true,
                children: [
                    new LayoutElement(
                        ignoreLayout: true,
                        innerElement: new ExpandedElement(
                            innerElement: ClickInputHandler
                        )
                    ),
                    TitleLayout,
                    new LayoutElement(
                        ignoreLayout: true,
                        innerElement: new ExpandedElement(
                            innerElement: HoverInputHandler
                        )
                    ),
                ]
            );

            InnerElement = _titleRow;
        }

        public void RefreshFoldButtonSkin()
        {
            if (Data is null)
                return;

            var skin = Data.IsFolded
                ? StandardSkin.RightArrowIcon
                : StandardSkin.DownArrowIcon;

            FoldButton.NormalSkin = skin;
            FoldButton.HoverSkin = skin;
            FoldButton.PressedSkin = skin;
            FoldButton.DisabledSkin = skin;
        }

        public void RefreshBackgroundVisualState()
        {
            if (Data is null)
                return;

            if (Data.IsSelected)
            {
                Background.Color = SelectedBackgroundColor;
            }
            else
            {
                Background.Color = _isHover
                    ? HoverBackgroundColor
                    : NormalBackgroundColor;
            }
        }

        private void RefreshFoldButtonVisibility()
        {
            if (Data is null)
                return;

            var isVisible = Data.Children.Count > 0;

            FoldButton.IsEnabled = isVisible;
            _foldButtonPlaceholder.IsEnabled = !isVisible;
            _foldButtonPlaceholder.Size = FoldButton.Size;
        }

        void ILazyListItem<HierarchyNodeData>.SetData(HierarchyNodeData data)
        {
            Data = data;

            Icon.Sprite = Data.Sprite;
            Icon.Skin = Data.Skin;
            Name.Text = Data.Name;
            Indent = Data.Indent;

            RefreshFoldButtonSkin();
            RefreshFoldButtonVisibility();
            RefreshBackgroundVisualState();
        }

        void ILazyListItem<HierarchyNodeData>.ClearData()
        {
            Data = null;
        }

        private void OnClickInputHandlerPointerClick(PointerInputHandlerElement sender, PointerEvent arguments)
        {
            OnClicked?.Invoke(this);
        }

        private void OnFoldButtonPointerClick(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            OnFoldButtonClicked?.Invoke(this);
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
