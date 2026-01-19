using ComposableUi.Core;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public sealed class AlignmentElement : SizedToContentHolderElement, IDrawableElement
    {
        private Vector2 _alignmentFactor;
        public Vector2 AlignmentFactor
        {
            get => _alignmentFactor;
            set => SetAndChangeState(ref _alignmentFactor, value);
        }

        private Vector2 _offset;
        public Vector2 Offset
        {
            get => _offset;
            set => SetAndChangeState(ref _offset, value);
        }

        public AlignmentElement(Element innerElement = default,
            Vector2 alignmentFactor = default,
            Vector2 offset = default) 
            : base(innerElement) 
        { 
            AlignmentFactor = alignmentFactor;
            Offset = offset;
        }

        public override void ApplySize(Vector2 size)
        {
            base.ApplySize(size);

            if (Parent != null)
                LocalPosition = Parent.Size * AlignmentFactor + Offset - Parent.Size * Parent.Pivot;
        }

        void IDrawableElement.Draw(IUiRenderer renderer)
        {
            renderer.DrawRectangle(BoundingRectangle, ClipMask, new Color(Color.Red, 0.2f));
        }
    }
}
