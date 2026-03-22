using ComposableUi;

using Microsoft.Xna.Framework;

namespace TankRacerViewer.Core
{
    public sealed class InspectorWindow : WindowElement
    {
        private readonly ScrollViewElement _scrollView;
        private readonly FoldableGroupElement _group;

        public InspectorWindow() : base("Inspector")
        {
            _group = new FoldableGroupElement(
                name: "Texture Info",
                normalBackgroundColor: Color.DarkSlateBlue,
                hoverBackgroundColor: Color.DarkSlateBlue,
                selectedBackgroundColor: Color.DarkSlateBlue,
                content: new SpriteElement(
                    size: new Vector2(180),
                    skin: StandardSkin.ContentPanel
                )
            )
            {
                Indent = 0
            };
            _group.Icon.IsEnabled = false;

            _scrollView = new ScrollViewElement(
                //sizeToContentWidth: true,
                expandContentWidth: true,
                sizeToContentHeight: true,
                content: _group
            );

            ContentContainer.AddChild(new ExpandedElement(_scrollView));
        }
    }
}
