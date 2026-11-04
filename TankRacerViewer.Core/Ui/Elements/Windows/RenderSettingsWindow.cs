using System;
using System.Linq;

using ComposableUi;

using Microsoft.Xna.Framework;

namespace TankRacerViewer.Core
{
    public sealed class RenderSettingsWindow : WindowElement
    {
        public RenderSettingsWindow() : base("Render Settings")
        {
            // FOR DEBUG
            var searchField = TestTextWindow.Instance.SearchField;

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

            var horizontalAlignmentBoundsMode = new DropDownListElement(
                items: Enum.GetNames<TextBoundsMode>()
                    .Select(name => new DropDownListTextItemElement(name))
            );
            horizontalAlignmentBoundsMode.ItemSelected += (_, i) =>
            {
                searchField.HorizontalAlignmentBoundsMode = (TextBoundsMode)i;
            };
            horizontalAlignmentBoundsMode.SelectItem(0);

            var verticalAlignmentBoundsMode = new DropDownListElement(
                items: Enum.GetNames<TextBoundsMode>()
                    .Select(name => new DropDownListTextItemElement(name))
            );
            verticalAlignmentBoundsMode.ItemSelected += (_, i) =>
            {
                searchField.VerticalAlignmentBoundsMode = (TextBoundsMode)i;
            };
            verticalAlignmentBoundsMode.SelectItem(0);

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
                leftPadding: 2,
                rightPadding: 2,
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
                    new TextElement(
                        text: "Horizontal Bounds:",
                        sizeToTextHeight: true
                    ),
                    horizontalAlignmentBoundsMode,
                    new TextElement(
                        text: "Vertical Bounds:",
                        sizeToTextHeight: true
                    ),
                    verticalAlignmentBoundsMode,
                    multilineToggle,
                    wordWrapToggle,
                    preserveWhitespaceToggle
                ]
            );

            ContentContainer.AddChild(new ExpandedElement(
                innerElement: new ScrollViewElement(
                    expandingContentWidthMode: ScrollViewElement.ExpandingMode.FillParent,
                    content: propertiesColumn
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
