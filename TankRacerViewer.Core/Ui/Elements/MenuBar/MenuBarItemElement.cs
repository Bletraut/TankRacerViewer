using System;

using ComposableUi;

using Microsoft.Xna.Framework;

namespace TankRacerViewer.Core
{
    public sealed class MenuBarItemElement : PointerInputHandlerElement
    {
        private const int DefaultContentHorizontalPadding = 8;

        // Static.
        public static readonly ISpriteSource DefaultBackgroundNormalSprite = BuiltInSprite.WhitePixel;
        public static readonly ISpriteSource DefaultBackgroundHoverSprite = BuiltInSprite.HoverSoftLightPixel;
        public static readonly ISpriteSource DefaultBackgroundSelectedSprite = BuiltInSprite.SelectionSoftDarkPixel;

        public static readonly Color DefaultTextNormalColor = Color.Black;
        public static readonly Color DefaultTextHoverColor = Color.Black;
        public static readonly Color DefaultTextSelectedColor = Color.White;

        // Class.
        public ISpriteSource BackgroundNormalSprite { get; set; } = DefaultBackgroundNormalSprite;
        public ISpriteSource BackgroundHoverSprite { get; set; } = DefaultBackgroundHoverSprite;
        public ISpriteSource BackgroundSelectedSprite { get; set; } = DefaultBackgroundSelectedSprite;

        public Color TextNormalColor { get; set; } = DefaultTextNormalColor;
        public Color TextHoverColor { get; set; } = DefaultTextHoverColor;
        public Color TextSelectedColor { get; set; } = DefaultTextSelectedColor;

        public SpriteElement Background { get; }
        public TextElement Text { get; }

        public Action SelectAction { get; set; }
        public Action UnselectAction { get; set; }

        public event ElementEventHandler<MenuBarItemElement> Enter;
        public event ElementEventHandler<MenuBarItemElement> Leave;
        public event ElementEventHandler<MenuBarItemElement> Clicked;

        public MenuBarItemElement(string text,
            Action selectAction, Action unselectAction)
        {
            SelectAction = selectAction;
            UnselectAction = unselectAction;

            Background = new SpriteElement(
                spriteSource: BuiltInSprite.WhitePixel
            );

            Text = new TextElement(
                text: text,
                textAlignmentFactor: Alignment.Center,
                sizeToTextWidth: true
            );

            InnerElement = new RowLayout(
                leftPadding: DefaultContentHorizontalPadding,
                rightPadding: DefaultContentHorizontalPadding,
                expandChildrenCrossAxis: true,
                sizeMainAxisToContent: true,
                children: [
                    new LayoutElement(
                        ignoreLayout: true,
                        innerElement: new ExpandedElement(Background)
                    ),
                    Text
                ]
            );

            SetState(State.Normal);
        }

        public void SetState(State state)
        {
            (ISpriteSource BackgroundSprite, Color TextColor) = state switch
            {
                State.Normal => (BackgroundNormalSprite, TextNormalColor),
                State.Hover => (BackgroundHoverSprite, TextHoverColor),
                State.Selected => (BackgroundSelectedSprite, TextSelectedColor),
                _ => (BackgroundNormalSprite, TextNormalColor)
            };

            Background.SpriteSource = BackgroundSprite;
            Text.Color = TextColor;
        }

        protected override void OnPointerEnter(in PointerEvent pointerEvent)
        {
            base.OnPointerEnter(pointerEvent);

            Enter?.Invoke(this);
        }

        protected override void OnPointerLeave(in PointerEvent pointerEvent)
        {
            base.OnPointerLeave(pointerEvent);

            Leave?.Invoke(this);
        }

        protected override void OnPointerClick(in PointerEvent pointerEvent)
        {
            base.OnPointerClick(pointerEvent);

            Clicked?.Invoke(this);
        }

        public enum State
        {
            Normal,
            Hover,
            Selected
        }
    }
}
