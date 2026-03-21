using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public sealed class RootElement : ContainerElement
    {
        internal bool IsDirty { get; private set; }

        internal void MarkAsDirty()
        {
            IsDirty = true;
        }

        public override void Rebuild(Vector2 size, bool excludeChildren)
        {
            base.Rebuild(size, excludeChildren);

            IsDirty = false;
        }
    }
}
