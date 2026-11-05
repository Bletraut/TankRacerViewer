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

            ContentContainer.AddChild(new ExpandedElement(
                innerElement: ScrollView
            ));
        }
    }
}
