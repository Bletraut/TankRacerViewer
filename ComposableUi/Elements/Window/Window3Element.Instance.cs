using System;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public partial class Window3Element : WindowNodeElement<WindowContainerElement>
    {
        public const int DefaultHeaderHeight = 30;

        private const int EmptyInsertIndex = -1;

        private const float SplitSizeFactor = 0.3f;
        private const float SplitAreaSelfThicknessFactor = 0.3f;
        private const float SplitAreaParentThicknessFactor = 0.1f;
        private const int SplitAreaResizeHandleSize = 4;

        public static readonly Vector2 DefaultSize = new(250, 300);
        public static readonly Vector2 DefaultMinSize = new(80, 100);
        public static readonly Vector2 DefaultContentPadding = new(10, 10);

        // Properties.
        public Element Header { get; }
        public PointerInputHandlerElement DragHandle { get; }
        public TabElement Tab { get; }

        private bool IsResizingInternally => false;

        // Events.
        public event ElementEventHandler<Window3Element, PointerEvent> TabPointerDown;
        public event ElementEventHandler<Window3Element, PointerEvent> TabPointerUp;
        public event ElementEventHandler<Window3Element, PointerDragEvent> TabPointerDrag;
        public event ElementEventHandler<Window3Element, Element> InsertPreviewShown;
        public event ElementEventHandler<Window3Element> InsertPreviewHidden;
        public event ElementEventHandler<Window3Element> SplitPreviewShown;
        public event ElementEventHandler<Window3Element> SplitPreviewHidden;

        // Fields.
        private readonly Element _view;

        private readonly RowLayout _tabRow;
        private readonly ContainerElement _contentContainer;

        private readonly PointerInputHandlerElement _insertArea;
        private readonly PointerInputHandlerElement _splitArea;
        private readonly PointerInputHandlerElement _innerResizeHandle;

        public Window3Element(string titleText = default,
            Element content = default,
            Vector2? size = default,
            Vector2? minSize = default) 
            : base(size ?? DefaultSize,
                  minSize ?? DefaultMinSize)
        {
            var background = new ExpandedElement(
                innerElement: new SpriteElement(
                    skin: StandardSkin.TabBody,
                    color: new Color(Color.White, 0.8f)
                )
            );

            DragHandle = new PointerInputHandlerElement(
                innerElement: new SpriteElement(skin: StandardSkin.TabInactiveHeader)
            );
            DragHandle.PointerDown += OnDragHandlePointerDown;
            DragHandle.PointerFixedDrag += OnDragHandlePointerFixedDrag;

            _tabRow = new RowLayout(
                alignmentFactor: Alignment.TopLeft,
                expandChildrenCrossAxis: true
            );

            Tab = new TabElement(
                titleText: titleText
            );
            _tabRow.AddChild(Tab);

            //Tab.PointerDown += OnTabButtonPointerDown;
            //Tab.PointerUp += OnTabButtonPointerUp;
            //Tab.PointerDrag += OnTabButtonPointerDrag;

            _contentContainer = new ContainerElement();
            var contentContainerParent = new ExpandedElement(
                leftPadding: DefaultContentPadding.X,
                rightPadding: DefaultContentPadding.X,
                topPadding: DefaultContentPadding.Y + DefaultHeaderHeight,
                bottomPadding: DefaultContentPadding.Y,
                innerElement: _contentContainer
            );
            if (content is not null)
                _contentContainer.AddChild(content);

            Header = new ClipMaskElement(
                innerElement: new ContainerElement(
                    size: new Vector2(DefaultHeaderHeight),
                    children: [
                        new ExpandedElement(DragHandle),
                        new ExpandedElement(_tabRow)
                    ]
                )
            );
            var headerParent = new ExpandedElement(
                expandHeight: false,
                innerElement: new AlignmentElement(
                    alignmentFactor: Alignment.TopCenter,
                    pivot: Alignment.TopCenter,
                    innerElement: Header
                )
            );

            _insertArea = new PointerInputHandlerElement(blockInput: false)
            {
                Size = new Vector2(DefaultHeaderHeight)
            };
            var insertAreaParent = new ExpandedElement(
                expandHeight: false,
                innerElement: new AlignmentElement(
                    alignmentFactor: Alignment.TopCenter,
                    pivot: Alignment.TopCenter,
                    innerElement: _insertArea
                )
            );
            //_insertArea.PointerMove += OnInsertAreaPointerMove;
            //_insertArea.PointerLeave += OnInsertAreaPointerLeave;

            _splitArea = new PointerInputHandlerElement(
                blockInput: false
            );
            var splitAreaParent = new ExpandedElement(
                topPadding: DefaultHeaderHeight,
                innerElement: _splitArea
            );
            //_splitArea.PointerDown += OnSplitAreaPointerDown;
            //_splitArea.PointerMove += OnSplitAreaPointerMove;
            //_splitArea.PointerLeave += OnSplitAreaPointerLeave;

            _innerResizeHandle = new PointerInputHandlerElement(
                blockInput: false
            );
            var innerResizeHandleParent = new ExpandedElement(
                innerElement: _innerResizeHandle
            );
            //_innerResizeHandle.PointerMove += OnInnerResizeHandlePointerMove;
            //_innerResizeHandle.PointerLeave += OnInnerResizeHandlePointerLeave;
            //_innerResizeHandle.PointerDown += OnInnerResizeHandlePointerDown;
            //_innerResizeHandle.PointerUp += OnInnerResizeHandlePointerUp;
            //_innerResizeHandle.PointerFixedDrag += OnInnerResizeHandlePointerFixedDrag;

            _view = new ContainerElement(
                children: [
                    background,
                    contentContainerParent,
                    headerParent,
                    innerResizeHandleParent,
                    insertAreaParent,
                    splitAreaParent
                ]
            );
            ViewHolder.InnerElement = _view;
        }

        public void BringToFront()
        {
            var root = ResolveRoot();
            if (root.Parent is ContainerElement container)
                container.BringToFront(root);
        }
    }
}
