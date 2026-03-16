using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public sealed class TabElement : PointerInputHandlerElement
    {
        public const int DefaultLeftPadding = 6;
        public const int DefaultRightPadding = 12;
        public const int DefaultItemSpacing = 4;

        public SpriteElement Background { get; }
        public SpriteElement Icon { get; }
        public TextElement Title { get; }

        public Sprite InactiveSprite { get; set; }
        public Sprite NormalSprite { get; set; }
        public Sprite SelectedSprite { get; set; }

        public StandardSkin InactiveSkin { get; set; }
        public StandardSkin NormalSkin { get; set; }
        public StandardSkin SelectedSkin { get; set; }

        public TabState CurrentState { get; private set; }

        public TabElement(string titleText = default,
            Sprite iconSprite = default,
            Sprite inactiveSprite = default,
            Sprite activeSprite = default,
            Sprite focusedSprite = default,
            StandardSkin inactiveSkin = StandardSkin.InactiveTab, 
            StandardSkin activeSkin = StandardSkin.ActiveTab, 
            StandardSkin focusedSkin = StandardSkin.SelectedTab) 
        {
            InactiveSprite = inactiveSprite;
            NormalSprite = activeSprite;
            SelectedSprite = focusedSprite;
            InactiveSkin = inactiveSkin;
            NormalSkin = activeSkin;
            SelectedSkin = focusedSkin;

            Background = new SpriteElement();
            var backgroundParent = new LayoutElement(
                ignoreLayout: true,
                innerElement: new ExpandedElement(
                    innerElement: Background
                )
            );

            Icon = new SpriteElement(
                size: new Vector2(10),
                sprite: iconSprite,
                skin: StandardSkin.RectangleButton,
                sizeToSource: true
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

            (Sprite Sprite, StandardSkin Skin) = CurrentState switch
            {
                TabState.Inactive => (InactiveSprite, InactiveSkin),
                TabState.Normal => (NormalSprite, NormalSkin),
                TabState.Selected => (SelectedSprite, SelectedSkin),
                _ => (InactiveSprite, InactiveSkin),
            };
            Background.Sprite = Sprite;
            Background.Skin = Skin;
        }

        public void CopyHeaderFrom(TabElement tab)
        {
            Icon.Sprite = tab.Icon.Sprite;
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
