using Microsoft.Xna.Framework;

using System.Diagnostics;

namespace ComposableUi
{
    public sealed class SpriteElement : Element, IDrawableElement
    {
        private Sprite _sprite;
        public Sprite Sprite
        {
            get => _sprite;
            set => SetAndChangeState(ref _sprite, value);
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

        public SpriteElement(Vector2? size = default,
            Sprite sprite = default,
            StandardSkin skin = StandardSkin.None,
            Color? color = default,
            bool sizeToSource = false,
            DrawMode drawMode = DrawMode.Sliced)
        {
            Size = size ?? Vector2.Zero;

            Sprite = sprite;
            Skin = skin;
            Color = color ?? Color.White;
            SizeToSource = sizeToSource;
            DrawMode = drawMode;
        }

        public override Vector2 CalculatePreferredSize()
        {
            var useSelfSize = Sprite is null
                || DrawMode is DrawMode.Sliced
                || !SizeToSource;
            if (useSelfSize)
                return base.CalculatePreferredSize();

            return Sprite.SourceRectangle.Size.ToVector2();
        }

        public int RebuildCount;
        public override void Rebuild(Vector2 size)
        {
            base.Rebuild(size);
            RebuildCount++;
        }

        void IDrawableElement.Draw(IUiRenderer renderer)
        {
            if (Sprite != null)
            {
                renderer.DrawSprite(Sprite, DrawMode.Sliced,
                    BoundingRectangle, ClipMask, Color);
            }
            else
            {
                renderer.DrawSkinnedRectangle(Skin, DrawMode.Sliced,
                    BoundingRectangle, ClipMask, Color);
            }
        }
    }
}
