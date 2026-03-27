using ComposableUi;

using Microsoft.Xna.Framework;

namespace TankRacerViewer.Core
{
    public sealed class UsedTextureElement : PointerInputHandlerElement<UsedTextureElement>,
        ILazyListItem<UsedTextureData>
    {
        public const float DefaultNameHeight = 20;
        public const float DefaultSpacing = 4;
        public const float DefaultVerticalPaddings = 8;

        public static readonly Vector2 DefaultSpriteSize = new(120);
        public static readonly Color DefaultEvenBackgroundColor = Color.MediumSlateBlue;
        public static readonly Color DefaultOddBackgroundColor = Color.SlateBlue;
        public static readonly Color DefaultHoverBackgroundColor = Color.CornflowerBlue;

        public UsedTextureData Data { get; private set; }

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
                sizeToSource: true,
                drawMode: DrawMode.Simple
            );

            _aspectRatioFitter = new AspectRatioFitterElement(
                aspectRatioMode: AspectRatioMode.FitInParent,
                innerElement: _image
            );

            InnerElement = new ColumnLayout(
                alignmentFactor: Alignment.TopCenter,
                topPadding: DefaultVerticalPaddings,
                bottomPadding: DefaultVerticalPaddings,
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
                    _name,
                    new HolderElement(
                        size: DefaultSpriteSize,
                        innerElement: _aspectRatioFitter
                    )
                ]
            );
        }

        private void RefreshBackgroundColor()
        {
            if (IsHover)
            {
                _background.Color = DefaultHoverBackgroundColor;
            }
            else
            {
                _background.Color = Data.Index % 2 == 0
                    ? DefaultEvenBackgroundColor
                    : DefaultOddBackgroundColor;
            }
        }

        void ILazyListItem<UsedTextureData>.SetData(UsedTextureData data)
        {
            Data = data;

            RefreshBackgroundColor();

            _name.Text = Data.TextureName;

            _sprite.Texture = Data.Texture is not null
                ? Data.Texture
                : DefaultUiRenderer.FallbackTexture;
            _sprite.Texture = _sprite.Texture;
            _sprite.SourceRectangle = _sprite.Texture.Bounds;

            _aspectRatioFitter.AspectRatio = _sprite.Texture.Width / (float)_sprite.Texture.Height;
        }

        void ILazyListItem<UsedTextureData>.ClearData()
        {
            Data = default;
            _sprite.Texture = null;
        }

        protected override void OnPointerEnter(in PointerEvent pointerEvent)
        {
            base.OnPointerEnter(pointerEvent);
            RefreshBackgroundColor();
        }

        protected override void OnPointerLeave(in PointerEvent pointerEvent)
        {
            base.OnPointerLeave(pointerEvent);
            RefreshBackgroundColor();
        }
    }
}
