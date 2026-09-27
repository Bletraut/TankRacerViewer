using ComposableUi;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TankRacerViewer.Core
{
    public sealed class RenderInfoElement : Element,
        IDrawableElement
    {
        // Static.
        public static readonly Vector2 DefaultSize = new(160, 65);

        public static readonly Color DefaultBackgroundColor = new(Color.Black, 0.8f);
        public static readonly Color DefaultTextColor = Color.White;

        public static readonly Vector2 DefaultTextOffset = new(8, 4);

        // Class.
        public SpriteFont SpriteFont { get; set; }

        public Color BackgroundColor { get; set; }
        public Color TextColor { get; set; }
        public Vector2 TextOffset { get; set; }

        public string Text { get; set; }

        private Sprite _currentSprite;

        public RenderInfoElement(Vector2? size = default,
            SpriteFont spriteFont = default,
            Color? backgroundColor = default,
            Color? textColor = default,
            Vector2? textOffset = default)
        {
            Size = size ?? DefaultSize;
            SpriteFont = spriteFont;
            BackgroundColor = backgroundColor ?? DefaultBackgroundColor;
            TextColor = textColor ?? DefaultTextColor;
            TextOffset = textOffset ?? DefaultTextOffset;
        }

        public override Vector2 CalculatePreferredSize()
        {
            ISpriteSource spriteSource = BuiltInSprite.WhitePixel;
            _currentSprite = spriteSource.Resolve(Context);

            return base.CalculatePreferredSize();
        }

        void IDrawableElement.Draw(IUiRenderer renderer)
        {
            var spriteFont = SpriteFont ?? Context?.Theme.DefaultSpriteFont;
            if (spriteFont is null)
                return;

            if (string.IsNullOrEmpty(Text))
                return;

            if (_currentSprite == null) 
                return;

            var clipMask = ClipMask;
            var boundingRectangle = BoundingRectangle;
            var textPosition = TextOffset + boundingRectangle.Location.ToVector2();

            renderer.DrawSprite(_currentSprite, DrawMode.Simple,
                boundingRectangle, clipMask, BackgroundColor);

            renderer.DrawString(spriteFont, Text, textPosition,
                clipMask, TextColor);
        }
    }
}
