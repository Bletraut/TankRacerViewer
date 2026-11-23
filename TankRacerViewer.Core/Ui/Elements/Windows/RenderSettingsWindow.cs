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
            var textInput = TestTextWindow.Instance.TextInput;

            var richText = textInput.TextElement;
            var scrollView = new ScrollViewElement();

            var horizontalAlignmentMode = new DropDownListElement(
                items: Enum.GetNames<HorizontalAlignmentMode>()
                    .Select(name => new DropDownListTextItemElement(name))
            );
            horizontalAlignmentMode.ItemSelected += (_, i) =>
            {
                richText.HorizontalAlignmentMode = (HorizontalAlignmentMode)i;
            };
            horizontalAlignmentMode.SelectItem(0);

            var verticalAlignmentMode = new DropDownListElement(
                items: Enum.GetNames<VerticalAlignmentMode>()
                    .Select(name => new DropDownListTextItemElement(name))
            );
            verticalAlignmentMode.ItemSelected += (_, i) =>
            {
                richText.VerticalAlignmentMode = (VerticalAlignmentMode)i;
            };
            verticalAlignmentMode.SelectItem(0);

            var horizontalAlignmentBoundsMode = new DropDownListElement(
                items: Enum.GetNames<TextBoundsMode>()
                    .Select(name => new DropDownListTextItemElement(name))
            );
            horizontalAlignmentBoundsMode.ItemSelected += (_, i) =>
            {
                richText.HorizontalAlignmentBoundsMode = (TextBoundsMode)i;
            };
            horizontalAlignmentBoundsMode.SelectItem(0);

            var verticalAlignmentBoundsMode = new DropDownListElement(
                items: Enum.GetNames<TextBoundsMode>()
                    .Select(name => new DropDownListTextItemElement(name))
            );
            verticalAlignmentBoundsMode.ItemSelected += (_, i) =>
            {
                richText.VerticalAlignmentBoundsMode = (TextBoundsMode)i;
            };
            verticalAlignmentBoundsMode.SelectItem(0);

            var multilineToggle = UiElementFactory.CreateToggleButton("Multiline");
            RefreshToggle(multilineToggle, richText.Multiline);
            multilineToggle.PointerClick += (_, _) =>
            {
                richText.Multiline = !richText.Multiline;
                RefreshToggle(multilineToggle, richText.Multiline);
            };

            var wordWrapToggle = UiElementFactory.CreateToggleButton("Wrap Mode");
            RefreshToggle(wordWrapToggle, richText.WordWrap);
            wordWrapToggle.PointerClick += (_, _) =>
            {
                richText.WordWrap = !richText.WordWrap;
                RefreshToggle(wordWrapToggle, richText.WordWrap);
            };

            var preserveWhitespaceToggle = UiElementFactory.CreateToggleButton("Preserve Whitespace");
            RefreshToggle(preserveWhitespaceToggle, richText.PreserveWhitespace);
            preserveWhitespaceToggle.PointerClick += (_, _) =>
            {
                richText.PreserveWhitespace = !richText.PreserveWhitespace;
                RefreshToggle(preserveWhitespaceToggle, richText.PreserveWhitespace);
            };

            var sizeToTextWidthToggle = UiElementFactory.CreateToggleButton("Size To Text Width");
            RefreshToggle(sizeToTextWidthToggle, richText.SizeToTextWidth);
            scrollView.ExpandingContentWidthMode = richText.SizeToTextWidth
                ? ScrollViewElement.ExpandingMode.None
                : ScrollViewElement.ExpandingMode.FillParent;
            sizeToTextWidthToggle.PointerClick += (_, _) =>
            {
                richText.SizeToTextWidth = !richText.SizeToTextWidth;
                RefreshToggle(sizeToTextWidthToggle, richText.SizeToTextWidth);

                scrollView.ExpandingContentWidthMode = richText.SizeToTextWidth
                    ? ScrollViewElement.ExpandingMode.None
                    : ScrollViewElement.ExpandingMode.FillParent;
            };

            var sizeToTextHeightToggle = UiElementFactory.CreateToggleButton("Size To Text Height");
            RefreshToggle(sizeToTextHeightToggle, richText.SizeToTextHeight);
            scrollView.ExpandingContentHeightMode = richText.SizeToTextHeight
                ? ScrollViewElement.ExpandingMode.None
                : ScrollViewElement.ExpandingMode.FillParent;
            sizeToTextHeightToggle.PointerClick += (_, _) =>
            {
                richText.SizeToTextHeight = !richText.SizeToTextHeight;
                RefreshToggle(sizeToTextHeightToggle, richText.SizeToTextHeight);

                scrollView.ExpandingContentHeightMode = richText.SizeToTextHeight
                    ? ScrollViewElement.ExpandingMode.None
                    : ScrollViewElement.ExpandingMode.FillParent;
            };

            var horizontalSizingMode = new DropDownListElement(
                items: Enum.GetNames<TextBoundsMode>()
                    .Select(name => new DropDownListTextItemElement(name))
            );
            horizontalSizingMode.ItemSelected += (_, i) =>
            {
                richText.HorizontalSizingMode = (TextBoundsMode)i;
            };
            horizontalSizingMode.SelectItem(0);

            var verticalSizingMode = new DropDownListElement(
                items: Enum.GetNames<TextBoundsMode>()
                    .Select(name => new DropDownListTextItemElement(name))
            );
            verticalSizingMode.ItemSelected += (_, i) =>
            {
                richText.VerticalSizingMode = (TextBoundsMode)i;
            };
            verticalSizingMode.SelectItem(0);

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
                    new TextElement(
                        text: "Wrapping:",
                        sizeToTextHeight: true
                    ),
                    multilineToggle,
                    wordWrapToggle,
                    preserveWhitespaceToggle,
                    new TextElement(
                        text: "Sizing:",
                        sizeToTextHeight: true
                    ),
                    sizeToTextWidthToggle,
                    sizeToTextHeightToggle,
                    new TextElement(
                        text: "Horizontal Sizing Mode:",
                        sizeToTextHeight: true
                    ),
                    horizontalSizingMode,
                    new TextElement(
                        text: "Vertical Sizing Mode:",
                        sizeToTextHeight: true
                    ),
                    verticalSizingMode
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
