using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class WindowNodeElement<T> : ResizableElement
        where T : Element
    {
        internal T Container { get; private set; }

        internal T RootContainer { get; private set; }

        internal ExpandedElement ViewHolder { get; }

        private readonly AlignmentElement _overlayAlignment;
        private readonly ExpandedElement _overlayExpanded;

        public WindowNodeElement(Vector2? size = default,
            Vector2? minSize = default) 
        {
            MinSize = minSize ?? Vector2.Zero;

            ViewHolder = new ExpandedElement();

            _overlayAlignment = new AlignmentElement();
            _overlayExpanded = new ExpandedElement(_overlayAlignment);

            InnerElement = new ContainerElement(
                size: size,
                children: [
                    ViewHolder,
                    _overlayExpanded
                ]
            );
        }

        internal void ShowInOverlayAndAlignToEdge(Element element, Vector2 edgeNormal)
        {
            var alignmentFactor = Vector2.Max(Vector2.Zero, edgeNormal);

            _overlayExpanded.IsEnabled = true;
            _overlayExpanded.ExpandWidth = edgeNormal.Y != 0;
            _overlayExpanded.ExpandHeight = edgeNormal.X != 0;

            _overlayAlignment.InnerElement = element;
            _overlayAlignment.AlignmentFactor = alignmentFactor;
            _overlayAlignment.Pivot = alignmentFactor;
        }

        internal void HideOverlay()
        {
            _overlayExpanded.IsEnabled = false;
        }

        internal void SetSize(Vector2 size)
        {
            Size = size;

            if (InnerElement is not null)
                InnerElement.Size = size;
        }

        protected Element ResolveRootContainer()
            => RootContainer is not null ? RootContainer : this;

        internal virtual void ApplyContainer(T container)
        {
            Container = container;
        }

        internal virtual void ApplyRootContainer(T root)
        {
            RootContainer = root;
        }

        internal virtual void PrepareReplacementWith(WindowNodeElement<T> node)
        {
            node.SetSize(Size);
            node.ApplyContainer(Container);
            node.ApplyRootContainer(RootContainer);
            node.IsInteractable = IsInteractable;
        }

        internal virtual void SetSelected(bool value) { }
    }
}
