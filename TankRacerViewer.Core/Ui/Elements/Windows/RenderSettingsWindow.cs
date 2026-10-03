using System;
using System.Linq;

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
                size: new Vector2(DefaultSearchFieldHeight)
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
            var horizontalAlignmentMode = new DropDownListElement(
                items: Enum.GetNames<HorizontalAlignmentMode>()
                    .Select(name => new DropDownListTextItemElement(name))
            );
            horizontalAlignmentMode.ItemSelected += (_, i) =>
            {
                searchField.HorizontalAlignmentMode = (HorizontalAlignmentMode)i;
            };
            horizontalAlignmentMode.SelectItem(0);

            var verticalAlignmentMode = new DropDownListElement(
                items: Enum.GetNames<VerticalAlignmentMode>()
                    .Select(name => new DropDownListTextItemElement(name))
            );
            verticalAlignmentMode.ItemSelected += (_, i) =>
            {
                searchField.VerticalAlignmentMode = (VerticalAlignmentMode)i;
            };
            verticalAlignmentMode.SelectItem(0);

            var multilineToggle = UiElementFactory.CreateToggleButton("Multiline");
            RefreshToggle(multilineToggle, searchField.Multiline);
            multilineToggle.PointerClick += (_, _) =>
            {
                searchField.Multiline = !searchField.Multiline;
                RefreshToggle(multilineToggle, searchField.Multiline);
            };

            var wordWrapToggle = UiElementFactory.CreateToggleButton("Wrap Mode");
            RefreshToggle(wordWrapToggle, searchField.WordWrap);
            wordWrapToggle.PointerClick += (_, _) =>
            {
                searchField.WordWrap = !searchField.WordWrap;
                RefreshToggle(wordWrapToggle, searchField.WordWrap);
            };

            var preserveWhitespaceToggle = UiElementFactory.CreateToggleButton("Preserve Whitespace");
            RefreshToggle(preserveWhitespaceToggle, searchField.PreserveWhitespace);
            preserveWhitespaceToggle.PointerClick += (_, _) =>
            {
                searchField.PreserveWhitespace = !searchField.PreserveWhitespace;
                RefreshToggle(preserveWhitespaceToggle, searchField.PreserveWhitespace);
            };

            var propertiesColumn = new ColumnLayout(
                spacing: 4,
                sizeMainAxisToContent: true,
                expandChildrenCrossAxis: true,
                children: [
                    new TextElement(
                        text: "Horizontal alignment:",
                        sizeToTextHeight: true
                    ),
                    horizontalAlignmentMode,
                    new TextElement(
                        text: "Vertical alignment:",
                        sizeToTextHeight: true
                    ),
                    verticalAlignmentMode,
                    multilineToggle,
                    wordWrapToggle,
                    preserveWhitespaceToggle
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

        private void RefreshToggle(ContentButtonElement toggle, bool value)
        {
            if (value)
            {
                toggle.NormalSprite = BuiltInSprite.LightRectangle;
                toggle.HoverSprite = BuiltInSprite.HoverLightRectangle;
                toggle.PressedSprite = BuiltInSprite.HoverLightRectangle;
            }
            else
            {
                toggle.NormalSprite = BuiltInSprite.DarkRectangle;
                toggle.HoverSprite = BuiltInSprite.HoverDarkRectangle;
                toggle.PressedSprite = BuiltInSprite.HoverDarkRectangle;
            }
        }
    }
}
