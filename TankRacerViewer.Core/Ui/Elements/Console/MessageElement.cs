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
        public static readonly StandardSkin DefaultEvenBackgroundSkin = StandardSkin.HoverSoftDarkPixel;
        public static readonly StandardSkin DefaultOddBackgroundSkin = StandardSkin.SoftDarkPixel;

        // Class.
        private readonly SpriteElement _background;
        private readonly SpriteElement _icon;
        private readonly TextElement _message;

        public MessageElement()
        {
            _background = new SpriteElement(
                skin: StandardSkin.HoverSoftDarkPixel
            );

            _icon = new SpriteElement(
                size: DefaultIconSize,
                sizeToSource: false
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
            _background.Skin = data.Index % 2 == 0
                ? DefaultEvenBackgroundSkin
                : DefaultOddBackgroundSkin;

            _icon.Sprite = data.Type switch
            {
                MessageType.Info => IconCollection.Get(IconName.MessageBig),
                MessageType.Warning => IconCollection.Get(IconName.ErrorBig),
                MessageType.Error => IconCollection.Get(IconName.ErrorSmall),
                _ => IconCollection.Get(IconName.MessageBig)
            };

            _message.Text = $"[{data.DateTime.TimeOfDay}]\n{data.Message}";
        }

        void ILazyListItem<MessageData>.ClearData()
        {
        }
    }
}
