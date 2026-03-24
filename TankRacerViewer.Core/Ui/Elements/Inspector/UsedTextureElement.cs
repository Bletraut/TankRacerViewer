using ComposableUi;

using Microsoft.Xna.Framework;

namespace TankRacerViewer.Core
{
    public sealed class UsedTextureElement : SizedToContentHolderElement,
        ILazyListItem<UsedTextureData>
    {
        public const float DefaultNameHeight = 20;

        public static readonly Vector2 DefaultSpriteSize = new(50);
        public static readonly Color DefaultEvenBackgroundColor = Color.Orange;
        public static readonly Color DefaultOddBackgroundColor = Color.Red;

        private readonly SpriteElement _background;
        private readonly TextElement _name;
        private readonly AspectRatioFitterElement _aspectRatioFitter;
        private readonly SpriteElement _image;

        private readonly Sprite _sprite;

        public UsedTextureElement() 
        {
            _background = new SpriteElement(
                skin: StandardSkin.WhitePixel
            );

            _name = new TextElement(
                size: new Vector2(DefaultNameHeight),
                textAlignmentFactor: Alignment.MiddleLeft,
                pivot: Alignment.MiddleLeft,
                sizeToTextWidth: true
            );

            _sprite = new Sprite();
            _image = new SpriteElement(
                sprite: _sprite,
                sizeToSource: true
            );

            _aspectRatioFitter = new AspectRatioFitterElement(
                aspectRatioMode: AspectRatioMode.FitInParent,
                innerElement: _image
            );

            InnerElement = new ColumnLayout(
                sizeMainAxisToContent: true,
                sizeCrossAxisToContent: true,
                children: [
                    new LayoutElement(
                        ignoreLayout: true,
                        innerElement: new ExpandedElement(
                            innerElement: _background
                        )
                    ),
                    _name,
                    new HolderElement(
                        size: DefaultSpriteSize,
                        innerElement: _aspectRatioFitter
                    )
                ]
            );
        }

        void ILazyListItem<UsedTextureData>.SetData(UsedTextureData data)
        {
            _background.Color = data.Index % 2 == 0
                ? DefaultEvenBackgroundColor
                : DefaultOddBackgroundColor;

            _name.Text = data.TextureName;

            if (data.Texture is not null)
            {
                _sprite.Texture = data.Texture;
                _sprite.SourceRectangle = data.Texture.Bounds;

                _aspectRatioFitter.AspectRatio = data.Texture.Width / (float)data.Texture.Height;
            }
            else
            {
                _sprite.Texture = DefaultUiRenderer.FallbackTexture;
                _sprite.SourceRectangle = Rectangle.Empty;

                _aspectRatioFitter.AspectRatio = 1;
            }
        }

        void ILazyListItem<UsedTextureData>.ClearData()
        {
            _sprite.Texture = null;
        }
    }
}
