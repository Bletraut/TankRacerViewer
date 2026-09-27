using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public sealed class TabElement : PointerInputHandlerElement
    {
        public const float DefaultIconSize = 22;

        public const int DefaultLeftPadding = 6;
        public const int DefaultRightPadding = 12;
        public const int DefaultItemSpacing = 4;

        public SpriteElement Background { get; }
        public SpriteElement Icon { get; }
        public TextElement Title { get; }

        public ISpriteSource InactiveSprite { get; set; }
        public ISpriteSource NormalSprite { get; set; }
        public ISpriteSource SelectedSprite { get; set; }

        public TabState CurrentState { get; private set; }

        public TabElement(string titleText = default,
            ISpriteSource iconSprite = default,
            ISpriteSource inactiveSprite = default,
            ISpriteSource activeSprite = default,
            ISpriteSource focusedSprite = default)
        {
            InactiveSprite = inactiveSprite ?? BuiltInSprite.InactiveTab;
            NormalSprite = activeSprite ?? BuiltInSprite.ActiveTab;
            SelectedSprite = focusedSprite ?? BuiltInSprite.SelectedTab;

            Background = new SpriteElement();
            var backgroundParent = new LayoutElement(
                ignoreLayout: true,
                innerElement: new ExpandedElement(
                    innerElement: Background
                )
            );

            Icon = new SpriteElement(
                size: new Vector2(DefaultIconSize),
                spriteSource: iconSprite ?? BuiltInSprite.RectangleButton
            );

            Title = new TextElement(
                text: titleText,
                sizeToTextWidth: true,
                sizeToTextHeight: true
            );

            InnerElement = new RowLayout(
                spacing: DefaultItemSpacing,
                leftPadding: DefaultLeftPadding,
                rightPadding: DefaultRightPadding,
                alignmentFactor: Alignment.MiddleLeft,
                sizeMainAxisToContent: true,
                children: [backgroundParent, Icon, Title]
            );

            SetState(TabState.Normal);
        }

        internal void SetState(TabState state)
        {
            if (CurrentState == state)
                return;

            CurrentState = state;

            var sprite = CurrentState switch
            {
                TabState.Inactive => InactiveSprite,
                TabState.Normal => NormalSprite,
                TabState.Selected => SelectedSprite,
                _ => InactiveSprite,
            };
            Background.SpriteSource = sprite;
        }

        public void CopyHeaderFrom(TabElement tab)
        {
            Icon.Size = tab.Icon.Size;
            Icon.SpriteSource = tab.Icon.SpriteSource;
            Title.Text = tab.Title.Text;
        }
    }

    public enum TabState
    {
        Inactive,
        Normal,
        Selected
    }
}
