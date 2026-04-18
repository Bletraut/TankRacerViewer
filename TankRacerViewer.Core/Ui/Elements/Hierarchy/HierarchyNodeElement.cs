using ComposableUi;

using Microsoft.Xna.Framework;

namespace TankRacerViewer.Core
{
    public sealed class HierarchyNodeElement : SizedToContentHolderElement,
        ILazyListItem<HierarchyNodeData>
    {
        public const float DefaultSpacing = 4;

        public const StandardSkin DefaultNormalBackgroundSkin = StandardSkin.None;
        public const StandardSkin DefaultHoverBackgroundSkin = StandardSkin.HoverSoftDarkPixel;
        public const StandardSkin DefaultSelectedBackgroundSkin = StandardSkin.SelectionStrongDarkPixel;

        private readonly float DefaultTitleHorizontalPadding = 4;

        public readonly Vector2 DefaultFoldButtonSize = new(12);
        public readonly Vector2 DefaultIconSize = new(24);

        private StandardSkin _normalBackgroundSkin;
        public StandardSkin NormalBackgroundSkin
        {
            get => _normalBackgroundSkin;
            set
            {
                _normalBackgroundSkin = value;
                RefreshBackgroundVisualState();
            }
        }
        private StandardSkin _hoverBackgroundSkin;
        public StandardSkin HoverBackgroundSkin
        {
            get => _hoverBackgroundSkin;
            set
            {
                _hoverBackgroundSkin = value;
                RefreshBackgroundVisualState();
            }
        }
        private StandardSkin _selectedBackgroundSkin;
        public StandardSkin SelectedBackgroundSkin
        {
            get => _selectedBackgroundSkin;
            set
            {
                _selectedBackgroundSkin = value;
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

        public HierarchyNodeElement(StandardSkin normalBackgroundSkin = DefaultNormalBackgroundSkin,
            StandardSkin hoverBackgroundSkin = DefaultHoverBackgroundSkin,
            StandardSkin selectedBackgroundSkin = DefaultSelectedBackgroundSkin)
        {
            _normalBackgroundSkin = normalBackgroundSkin;
            _hoverBackgroundSkin = hoverBackgroundSkin;
            _selectedBackgroundSkin = selectedBackgroundSkin;

            Background = new SpriteElement(
                skin: _normalBackgroundSkin
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
                normalColor: Color.FloralWhite,
                hoverColor: Color.BlanchedAlmond,
                pressedColor: Color.LightSteelBlue
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
                Background.Skin = SelectedBackgroundSkin;
            }
            else
            {
                Background.Skin = HoverInputHandler.IsHover
                    ? HoverBackgroundSkin
                    : NormalBackgroundSkin;
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
            RefreshBackgroundVisualState();
        }

        private void OnHoverInputHandlerPointerLeave(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            RefreshBackgroundVisualState();
        }
    }
}
