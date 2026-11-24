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
        public TextInputElement TextInput { get; private set; }

        public TestTextWindow() 
            : base("Test Text", minSize: Vector2.Zero)
        {
            Instance = this;

            TextInput = new TextInputElement(
                text: "This chapter describes a method for fast, stable fluid simulation that runs entirely on the GPU. It introduces fluid dynamics and the associated mathematics, and it describes in detail the techniques to perform the simulation on the GPU. After reading this chapter, you should have a basic understanding of fluid dynamics and know how to simulate fluids using the GPU. The source code accompanying this book demonstrates the techniques described in this chapter.\nFluids are everywhere: water passing between riverbanks, smoke curling from a glowing cigarette, steam rushing from a teapot, water vapor forming into clouds, and paint being mixed in a can. Underlying all of them is the flow of fluids. All are phenomena that we would like to portray realistically in interactive graphics applications. Figure 38-1 shows examples of fluids simulated using the source code provided with this book."
            );
            TextInput.TextElement.SelectedTextColor = Color.Red;
            TextInput.TextElement.SelectionStart = 20;
            TextInput.TextElement.SelectionLength = 120;

            ContentContainer.AddChild(new ExpandedElement(
                leftPadding: 10,
                rightPadding: 10,
                topPadding: 10,
                bottomPadding: 10,
                innerElement: TextInput
            ));
        }
    }
}
