using ComposableUi;

namespace TankRacerViewer.Core
{
    public sealed class FoldableFileGroupElement : FoldableGroupElement<FoldableFileGroupElement>
    {
        public string Path { get; }
        public object File { get; }

        public FoldableFileGroupElement(string path,
            object file = default,
            Sprite iconSprite = default,
            StandardSkin iconSkin = default,
            string name = default,
            bool isFolded = default)
            : base(iconSprite,
                  iconSkin,
                  name,
                  null,
                  isFolded)
        {
            Path = path;
            File = file;

#warning TODO: Optimize it.
            if (ClickInputHandler.Parent is ExpandedElement clickExpanded)
                clickExpanded.LeftPadding = clickExpanded.RightPadding = -10_000;

            if (HoverInputHandler.Parent is ExpandedElement hoverExpanded)
                hoverExpanded.LeftPadding = hoverExpanded.RightPadding = -10_000;
        }
    }
}
