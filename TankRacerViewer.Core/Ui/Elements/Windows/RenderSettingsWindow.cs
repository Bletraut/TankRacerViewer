using ComposableUi;

using Microsoft.Xna.Framework;

namespace TankRacerViewer.Core
{
    public sealed class RenderSettingsWindow : WindowElement
    {
        public const int DefaultSearchFieldHeight = 8;

        public RenderSettingsWindow() : base("Render Settings")
        {
            MinSize = Vector2.Zero;

            var searchField = new RichTextElement(
                text: "This chapter describes a method for fast, stable fluid simulation that runs entirely on the GPU.\nIt introduces fluid dynamics and the associated mathematics, and it describes in detail the techniques to perform the simulation on the GPU.",
                size: new Vector2(DefaultSearchFieldHeight),
                wrappingMode: WrappingMode.Wrap
            );
            ContentContainer.AddChild(new ExpandedElement(
                leftPadding: 2,
                rightPadding: 2,
                expandHeight: false,
                innerElement: new AlignmentElement(
                    alignmentFactor: Alignment.TopCenter,
                    pivot: Alignment.TopCenter,
                    innerElement: searchField
                )
            ));

            // FOR DEBUG
            var alignment = new Vector2[]
            {
                Alignment.TopLeft, Alignment.TopCenter, Alignment.TopRight,
                Alignment.MiddleLeft, Alignment.Center, Alignment.MiddleRight,
                Alignment.BottomLeft, Alignment.BottomCenter, Alignment.BottomRight
            };
            var textAlignment = new DropDownListElement(
                items: [
                    new DropDownListTextItemElement(nameof(Alignment.TopLeft)),
                    new DropDownListTextItemElement(nameof(Alignment.TopCenter)),
                    new DropDownListTextItemElement(nameof(Alignment.TopRight)),
                    new DropDownListTextItemElement(nameof(Alignment.MiddleLeft)),
                    new DropDownListTextItemElement(nameof(Alignment.Center)),
                    new DropDownListTextItemElement(nameof(Alignment.MiddleRight)),
                    new DropDownListTextItemElement(nameof(Alignment.BottomLeft)),
                    new DropDownListTextItemElement(nameof(Alignment.BottomCenter)),
                    new DropDownListTextItemElement(nameof(Alignment.BottomRight)),
                ]
            );
            textAlignment.ItemSelected += (_, i) =>
            {
                searchField.TextAlignmentFactor = alignment[i];
            };
            textAlignment.SelectItem(0);

            var justificationMode = new DropDownListElement(
                items: [
                    new DropDownListTextItemElement(JustificationMode.Natural.ToString()),
                    new DropDownListTextItemElement(JustificationMode.Justified.ToString()),
                    new DropDownListTextItemElement(JustificationMode.Flush.ToString())
                ]
            );
            justificationMode.ItemSelected += (_, i) =>
            {
                searchField.JustificationMode = (JustificationMode)i;
            };
            justificationMode.SelectItem(0);

            var wrapMode = new DropDownListElement(
                items: [
                    new DropDownListTextItemElement(WrappingMode.NoWrap.ToString()),
                    new DropDownListTextItemElement(WrappingMode.Wrap.ToString()),
                ]
            );
            wrapMode.ItemSelected += (_, i) =>
            {
                searchField.WrappingMode = (WrappingMode)i;
            };
            wrapMode.SelectItem(1);

            var propertiesColumn = new ColumnLayout(
                sizeMainAxisToContent: true,
                expandChildrenCrossAxis: true,
                children: [
                    new TextElement(
                        text: "Alignment:",
                        sizeToTextHeight: true
                    ),
                    textAlignment,
                    new TextElement(
                        text: "Justification:",
                        sizeToTextHeight: true
                    ),
                    justificationMode,
                    new TextElement(
                        text: "WrapMode:",
                        sizeToTextHeight: true
                    ),
                    wrapMode,
                ]
            );

            ContentContainer.AddChild(new ExpandedElement(
                expandHeight: false,
                innerElement: new AlignmentElement(
                    alignmentFactor: Alignment.BottomCenter,
                    pivot: Alignment.BottomCenter,
                    innerElement: propertiesColumn
                )
            ));
            // END
        }
    }
}
