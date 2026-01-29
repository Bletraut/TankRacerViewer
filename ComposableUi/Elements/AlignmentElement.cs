using ComposableUi.Core;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public sealed class AlignmentElement : SizedToContentHolderElement
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
            Vector2? alignmentFactor = default,
            Vector2? offset = default,
            Vector2? pivot = default) 
            : base(innerElement) 
        { 
            AlignmentFactor = alignmentFactor ?? Alignment.Center;
            Offset = offset ?? Vector2.Zero;
            Pivot = pivot ?? Alignment.Center;
        }

        public override void Rebuild(Vector2 size)
        {
            base.Rebuild(size);

            if (Parent != null)
                LocalPosition = Parent.Size * AlignmentFactor + Offset - Parent.Size * Parent.Pivot;
        }
    }
}
