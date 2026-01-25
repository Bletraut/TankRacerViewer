using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class ScrollViewElement : ContainerElement
    {
        public static readonly Vector2 DefaultSize = new(300, 300);

        public ClipMaskElement Content { get; }
        public ScrollBarElement HorizontalScrollBar { get; }
        public ScrollBarElement VerticalScrollBar { get; }

        public ScrollViewElement(Vector2? size = default)
        {
            ApplySize(size ?? DefaultSize);

            var inputHandler = new PointerInputHandlerElement(
                innerElement: new SpriteElement(
                    skin: StandardSkin.RectanglePanel));
            inputHandler.PointerClick += (_, _) =>
            {
                ApplySize(Vector2.Floor(Size * 1.1f));
            };
            inputHandler.PointerSecondaryClick += (_, _) =>
            {
                ApplySize(Vector2.Floor(Size / 1.1f));
                VerticalScrollBar.ProgressValue = 1;
            };
            AddChild(new ExpandedElement(
                innerElement: inputHandler));

            Content = new ClipMaskElement(
                innerElement: new SpriteElement(
                    size: new Vector2(200, 200),
                    skin: StandardSkin.ContentPanel));
            AddChild(new ExpandedElement(
                rightPadding: ScrollBarElement.DefaultCrossAxisSize,
                bottomPadding: ScrollBarElement.DefaultCrossAxisSize,
                innerElement: new AlignmentElement(
                    alignmentFactor: Alignment.TopLeft,
                    pivot: Alignment.TopLeft,
                    innerElement: Content)));

            HorizontalScrollBar = new HorizontalScrollBarElement();
            VerticalScrollBar = new VerticalScrollBarElement();

            AddChild(new ExpandedElement(
                expandHeight: false,
                rightPadding: ScrollBarElement.DefaultCrossAxisSize,
                innerElement: new AlignmentElement(
                    alignmentFactor: Alignment.BottomCenter,
                    pivot: Alignment.BottomCenter,
                    innerElement: HorizontalScrollBar)
                ));

            AddChild(new ExpandedElement(
                expandWidth: false,
                bottomPadding: ScrollBarElement.DefaultCrossAxisSize,
                innerElement: new AlignmentElement(
                    alignmentFactor: Alignment.MiddleRight,
                    pivot: Alignment.MiddleRight,
                    innerElement: VerticalScrollBar)
                ));
        }
    }
}
