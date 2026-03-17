using System;

using ComposableUi;

namespace TankRacerViewer.Core
{
    public sealed class ExplorerWindow : WindowElement
    {
        private readonly ScrollViewElement _scrollView;
        private readonly ColumnLayout _groups;

        public ExplorerWindow() : base("Explorer")
        {
            _groups = new ColumnLayout(
                alignmentFactor: Alignment.TopLeft,
                sizeMainAxisToContent: true,
                sizeCrossAxisToContent: true
            );

            _scrollView = new ScrollViewElement(
                sizeToContentWidth: true,
                sizeToContentHeight: true,
                content: _groups
            );

            for (var i = 0; i < 20; i++)
            {
                var g1 = new FoldableGroupElement(
                    iconSkin: StandardSkin.WhitePixel,
                    name: $"First group {i}"
                );
                _groups.AddChild(g1);

                var count = Random.Shared.Next(5);
                for (var j = 0; j < count; j++)
                {
                    var g2 = new FoldableGroupElement(
                        iconSkin: StandardSkin.WhitePixel,
                        name: "Inner group"
                    );
                    g1.AddItem(g2);

                    if (Random.Shared.NextSingle() < 0.8f)
                        continue;

                    var count2 = Random.Shared.Next(10);
                    for (var m = 0; m < count2; m++)
                    {
                        var g3 = new FoldableGroupElement(
                            iconSkin: StandardSkin.WhitePixel,
                            name: "Inner group"
                        );
                        g2.AddItem(g3);
                    }
                }
            }

            ContentContainer.AddChild(new ExpandedElement(_scrollView));
        }
    }
}
