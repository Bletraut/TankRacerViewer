using ComposableUi;

using Microsoft.Xna.Framework;

namespace TankRacerViewer.Core
{
    public sealed class MessageElement : ClipMaskElement,
        ILazyListItem<MessageData>
    {
        public const float DefaultSpacing = 8;
        public const float DefaultPaddings = 6;

        // Static.
        public static readonly Vector2 DefaultIconSize = new(30);
        public static readonly Color DefaultEvenBackgroundColor = Color.DarkSlateGray;
        public static readonly Color DefaultOddBackgroundColor = Color.SlateGray;

        // Class.
        private readonly SpriteElement _background;
        private readonly SpriteElement _icon;
        private readonly TextElement _message;

        public MessageElement()
        {
            _background = new SpriteElement(
                skin: StandardSkin.WhitePixel
            );

            _icon = new SpriteElement(
                size: DefaultIconSize,
                skin: StandardSkin.WhitePixel,
                sizeToSource: true
            );

            _message = new TextElement(
                textAlignmentFactor: Alignment.TopLeft,
                sizeToTextWidth: true,
                sizeToTextHeight: true
            );

            InnerElement = new RowLayout(
                alignmentFactor: Alignment.MiddleLeft,
                leftPadding: DefaultPaddings,
                rightPadding: DefaultPaddings,
                topPadding: DefaultPaddings,
                bottomPadding: DefaultPaddings,
                spacing: DefaultSpacing,
                sizeMainAxisToContent: true,
                sizeCrossAxisToContent: true,
                children: [
                    new LayoutElement(
                        ignoreLayout: true,
                        innerElement: new ExpandedElement(
                            innerElement: _background
                        )
                    ),
                    _icon,
                    _message
                ]
            );
        }

        void ILazyListItem<MessageData>.SetData(MessageData data)
        {
            _background.Color = data.Index % 2 == 0
                ? DefaultEvenBackgroundColor
                : DefaultOddBackgroundColor;

            _icon.Color = data.Type switch
            {
                MessageType.Info => Color.White,
                MessageType.Warning => Color.Yellow,
                MessageType.Error => Color.Red,
                _ => Color.White
            };

            _message.Text = $"[{data.DateTime.TimeOfDay}]\n{data.Message}";
        }

        void ILazyListItem<MessageData>.ClearData()
        {
        }
    }
}
