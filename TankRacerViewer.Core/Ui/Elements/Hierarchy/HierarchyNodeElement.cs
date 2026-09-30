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

        private ISpriteSource _normalBackgroundSprite;
        public ISpriteSource NormalBackgroundSprite
        {
            get => _normalBackgroundSprite;
            set
            {
                _normalBackgroundSprite = value;
                RefreshBackgroundVisualState();
            }
        }
        private ISpriteSource _hoverBackgroundSprite;
        public ISpriteSource HoverBackgroundSprite
        {
            get => _hoverBackgroundSprite;
            set
            {
                _hoverBackgroundSprite = value;
                RefreshBackgroundVisualState();
            }
        }
        private ISpriteSource _selectedBackgroundSprite;
        public ISpriteSource SelectedBackgroundSprite
        {
            get => _selectedBackgroundSprite;
            set
            {
                _selectedBackgroundSprite = value;
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

        public HierarchyNodeElement(ISpriteSource normalBackgroundSprite = default,
            ISpriteSource hoverBackgroundSprite = default,
            ISpriteSource selectedBackgroundSprite = default)
        {
            _normalBackgroundSprite = normalBackgroundSprite;
            _hoverBackgroundSprite = hoverBackgroundSprite ?? BuiltInSprite.HoverSoftDarkPixel;
            _selectedBackgroundSprite = selectedBackgroundSprite ?? BuiltInSprite.SelectionStrongDarkPixel;

            Background = new SpriteElement(
                spriteSource: _normalBackgroundSprite,
                drawMode: DrawMode.Sliced
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
                sizeToSource: true
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

        public void RefreshFoldButtonSprite()
        {
            if (Data is null)
                return;

            var sprite = Data.IsFolded
                ? BuiltInSprite.RightArrowIcon
                : BuiltInSprite.DownArrowIcon;

            FoldButton.NormalSprite = sprite;
            FoldButton.HoverSprite = sprite;
            FoldButton.PressedSprite = sprite;
            FoldButton.DisabledSprite = sprite;
        }

        public void RefreshBackgroundVisualState()
        {
            if (Data is null)
                return;

            if (Data.IsSelected)
            {
                Background.SpriteSource = SelectedBackgroundSprite;
            }
            else
            {
                Background.SpriteSource = HoverInputHandler.IsHover
                    ? HoverBackgroundSprite
                    : NormalBackgroundSprite;
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

            Icon.SpriteSource = Data.Sprite;
            Name.Text = Data.Name;
            Indent = Data.Indent;

            RefreshFoldButtonSprite();
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
