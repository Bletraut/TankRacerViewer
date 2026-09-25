using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public sealed class SpriteElement : Element, IDrawableElement
    {
        private ISpriteSource _spriteSource;
        public ISpriteSource SpriteSource
        {
            get => _spriteSource;
            set => SetAndChangeState(ref _spriteSource, value);
        }

        private StandardSkin _skin;
        public StandardSkin Skin
        {
            get => _skin;
            set => SetAndChangeState(ref _skin, value);
        }

        public Color Color { get; set; }

        private bool _sizeToSource;
        public bool SizeToSource
        {
            get => _sizeToSource;
            set => SetAndChangeState(ref _sizeToSource, value);
        }

        private DrawMode _drawMode;
        public DrawMode DrawMode
        {
            get => _drawMode;
            set => SetAndChangeState(ref _drawMode, value);
        }

        private Sprite _currentSprite;

        public SpriteElement(Vector2? size = default,
            ISpriteSource spriteSource = default,
            StandardSkin skin = StandardSkin.None,
            Color? color = default,
            bool sizeToSource = false,
            DrawMode drawMode = DrawMode.Sliced)
        {
            Size = size ?? Vector2.Zero;

            SpriteSource = spriteSource;
            Skin = skin;
            Color = color ?? Color.White;
            SizeToSource = sizeToSource;
            DrawMode = drawMode;
        }

        public override Vector2 CalculatePreferredSize()
        {
            SetAndChangeState(ref _currentSprite, SpriteSource?.Resolve(Context));

            var useSelfSize = _currentSprite is null
                || DrawMode is DrawMode.Sliced
                || !SizeToSource;
            if (useSelfSize)
                return base.CalculatePreferredSize();

            return _currentSprite.SourceRectangle.Size.ToVector2();
        }

        void IDrawableElement.Draw(IUiRenderer renderer)
        {
            if (_currentSprite is not null)
            {
                renderer.DrawSprite(_currentSprite, DrawMode,
                    BoundingRectangle, ClipMask, Color);
            }
            else if (Skin is not StandardSkin.None)
            {
                renderer.DrawSkinnedRectangle(Skin, DrawMode,
                    BoundingRectangle, ClipMask, Color);
            }
        }
    }
}
