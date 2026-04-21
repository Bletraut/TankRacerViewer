using System;

using ComposableUi;

using Microsoft.Xna.Framework;

namespace TankRacerViewer.Core
{
    public sealed class MenuBarItemElement : PointerInputHandlerElement
    {
        private const int DefaultContentHorizontalPadding = 8;

        public const StandardSkin DefaultBackgroundNormalSkin = StandardSkin.WhitePixel;
        public const StandardSkin DefaultBackgroundHoverSkin = StandardSkin.HoverSoftLightPixel;
        public const StandardSkin DefaultBackgroundSelectedSkin = StandardSkin.SelectionSoftDarkPixel;

        public static readonly Color DefaultTextNormalColor = Color.Black;
        public static readonly Color DefaultTextHoverColor = Color.Black;
        public static readonly Color DefaultTextSelectedColor = Color.White;

        public StandardSkin BackgroundNormalColor { get; set; } = DefaultBackgroundNormalSkin;
        public StandardSkin BackgroundHoverColor { get; set; } = DefaultBackgroundHoverSkin;
        public StandardSkin BackgroundSelectedColor { get; set; } = DefaultBackgroundSelectedSkin;

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
                skin: StandardSkin.WhitePixel
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
            (StandardSkin BackgroundSkin, Color TextColor) = state switch
            {
                State.Normal => (BackgroundNormalColor, TextNormalColor),
                State.Hover => (BackgroundHoverColor, TextHoverColor),
                State.Selected => (BackgroundSelectedColor, TextSelectedColor),
                _ => (BackgroundNormalColor, TextNormalColor)
            };

            Background.Skin = BackgroundSkin;
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
