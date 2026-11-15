using System;

using ComposableUi;

using Microsoft.Xna.Framework;

namespace TankRacerViewer.Core
{
    public sealed class TestTextWindow : WindowElement
    {
        public const int DefaultSearchFieldHeight = 8;

        // Static.
        public static TestTextWindow Instance { get; private set; }

        // Class.
        public RichTextElement SearchField { get; private set; }

        public ScrollViewElement ScrollView { get; private set; }

        public TestTextWindow() 
            : base("Test Text", minSize: Vector2.Zero)
        {
            Instance = this;

            SearchField = new RichTextElement(
                text: "How areeeeeee you.\nThis chapter describes a method for fast, stable fluid simulation A                     simulation A that runs entirely on the GPU.\nIt introduces fluid dynamics and the associated mathematics, and it describes in detail the techniques to perform the simulation on the GPU.",
                //text: "How areeeeeee you.\nThis chapter describes a method for fast, stable fluid simulation simulation A that runs entirely on the GPU.\nIt introduces fluid dynamics and the associated mathematics, and it describes in detail the techniques to perform the simulation on the GPU.",
                size: new Vector2(DefaultSearchFieldHeight)
            );

            ScrollView = new ScrollViewElement(
                content: SearchField
            );

            //ContentContainer.AddChild(new ExpandedElement(
            //    leftPadding: 2,
            //    innerElement: ScrollView
            //));

            var line = new ColumnLayout2(
                spacing: 10,
                alignmentFactor: Alignment.TopCenter,
                sizeCrossAxisToContent: true,
                expandChildrenCrossAxis: true,
                children: [
                    new LayoutElement(
                        ignoreLayout: true,
                        innerElement: new ExpandedElement(
                            expandWidth: true,
                            expandHeight: true,
                            innerElement: new SpriteElement(
                                spriteSource: BuiltInSprite.WhitePixel,
                                color: Color.Blue
                            )
                        )
                    )
                ]
            );
            line.Size = new Vector2(100, 200);

            AddText("This is long text");
            AddText("IMPOSIBLE LONG MAAAAAAAAAAX HEEEEEEEEMUUUUUUUUULLLLLL OLOLOLOLOOLOLO TETXTXTX TX TX TX");
            AddText("Short text");
            AddText("Super very very very long text and aaaaaaaaaa bbbbbbbbbb!!!!!!!!");

            //for (var i = 0; i < 5; i++)
            //{
            //    var child = new SpriteElement(
            //        size: new Vector2(10 + Random.Shared.NextSingle() * 50, 10 + Random.Shared.NextSingle() * 20),
            //        spriteSource: BuiltInSprite.WhitePixel,
            //        color: new Color(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle())
            //    );
            //    line.AddChild(child);
            //}

            ContentContainer.AddChild(new ExpandedElement(
                innerElement: line
            ));

            void AddText(string text)
            {
                line.AddChild(new RichTextElement(
                    text: text,
                    sizeToTextWidth: true,
                    sizeToTextHeight: true,
                    horizontalAlignmentMode: HorizontalAlignmentMode.Center
                ));
            }
        }
    }
}
