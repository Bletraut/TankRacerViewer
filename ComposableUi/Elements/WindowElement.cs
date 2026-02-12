using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class WindowElement : ResizableElement
    {
        public const int DefaultHeaderHeight = 30;

        public static readonly Vector2 DefaultSize = new(250, 300);
        public static readonly Vector2 DefaultMinSize = new(80, 100);
        public static readonly Vector2 DefaultContentPadding = new(10, 10);
        public static readonly Vector2 DefaultContentMargin = new(6, 6);

        public Element Content
        {
            get => _contentHolder.InnerElement;
            set => _contentHolder.InnerElement = value;
        }

        public Element Header { get; }
        public Element Body { get; }
        public SpriteElement ContentBackground { get; }

        private readonly PointerInputHandlerElement _headerInputHandler;
        private readonly HolderElement _contentHolder;

        public WindowElement(Element content = default,
            Vector2? size = default,
            Vector2? minSize = default)
        {
            MinSize = minSize ?? DefaultMinSize;

            _headerInputHandler = new PointerInputHandlerElement(
                innerElement: new SpriteElement(
                    size: new Vector2(0, DefaultHeaderHeight),
                    skin: StandardSkin.TabNormalHeader)
                );
            _headerInputHandler.PointerDrag += OnWindowDrag;

            Header = new ExpandedElement(
                expandHeight: false,
                innerElement: new AlignmentElement(
                    alignmentFactor: Alignment.TopCenter,
                    pivot: Alignment.TopCenter,
                    innerElement: _headerInputHandler)
                );

            Body = new ExpandedElement(
                topPadding: DefaultHeaderHeight,
                innerElement: new AlignmentElement(
                    alignmentFactor: Alignment.BottomCenter,
                    pivot: Alignment.BottomCenter,
                    innerElement: new SpriteElement(
                        skin: StandardSkin.TabBody)
                    )
                );

            ContentBackground = new SpriteElement(skin: StandardSkin.ContentPanel);
            var contentBackgroundParent = new ExpandedElement(
                leftPadding: DefaultContentPadding.X,
                rightPadding: DefaultContentPadding.Y,
                topPadding: DefaultHeaderHeight + DefaultContentPadding.Y,
                bottomPadding: DefaultContentPadding.Y,
                innerElement: ContentBackground);

            _contentHolder = new HolderElement();
            var contentParent = new ExpandedElement(
                leftPadding: DefaultContentPadding.X + DefaultContentMargin.X,
                rightPadding: DefaultContentPadding.X + DefaultContentMargin.X,
                topPadding: DefaultHeaderHeight + DefaultContentPadding.Y + DefaultContentMargin.Y,
                bottomPadding: DefaultContentPadding.Y + DefaultContentMargin.Y,
                innerElement: _contentHolder);

            Content = content;

            InnerElement = new ContainerElement([Header, Body, contentBackgroundParent, contentParent])
            {
                Size = size ?? DefaultSize
            };
        }

        private void OnWindowDrag(Element sender, (Point Position, Point delta) arguments)
        {
            LocalPosition += arguments.delta.ToVector2();
        }
    }
}
