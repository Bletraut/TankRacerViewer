using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ComposableUi
{
    public sealed class ContextMenu : SizedToContentHolderElement
    {
        private const int DefaultItemHeight = 22;

        private const int DefaultColumnsSpacing = 20;
        private const int DefaultContentPadding = 6;

        public SpriteElement Background { get; }

        private readonly ColumnLayout _buttonsColumn;
        private readonly ColumnLayout _itemNamesColumn;
        private readonly ColumnLayout _itemKeyBindingsColumn;
        private readonly RowLayout _columnsRow;

        public ContextMenu(IEnumerable<ContextMenuItemElement> items = default)
        {
            Background = new SpriteElement(skin: StandardSkin.RectanglePanel);
            var backgroundParent = new LayoutElement(
                ignoreLayout: true,
                innerElement: new ExpandedElement(Background)
            );

            _buttonsColumn = new ColumnLayout(
                expandChildrenMainAxisSize: true,
                expandChildrenCrossAxisSize: true);
            var buttonsParent = new LayoutElement(
                ignoreLayout: true,
                innerElement: new ExpandedElement(
                    leftPadding: DefaultContentPadding,
                    rightPadding: DefaultContentPadding,
                    topPadding: DefaultContentPadding,
                    bottomPadding: DefaultContentPadding,
                    innerElement: _buttonsColumn)
                );

            _itemNamesColumn = new ColumnLayout(
                alignmentFactor: Alignment.TopLeft,
                sizeMainAxisToContent: true,
                sizeCrossAxisToContent: true);

            _itemKeyBindingsColumn = new ColumnLayout(
                alignmentFactor: Alignment.TopRight,
                sizeMainAxisToContent: true,
                sizeCrossAxisToContent: true);

            _columnsRow = new RowLayout(
                children: [backgroundParent, buttonsParent, _itemNamesColumn, _itemKeyBindingsColumn],
                spacing: DefaultColumnsSpacing,
                leftPadding: DefaultContentPadding + 18,
                rightPadding: DefaultContentPadding,
                topPadding: DefaultContentPadding,
                bottomPadding: DefaultContentPadding,
                alignmentFactor: Alignment.TopLeft,
                sizeMainAxisToContent: true,
                sizeCrossAxisToContent: true);

            InnerElement = _columnsRow;

            for (var i = 0; i < 10; i++)
            {
                var item = new ContextMenuItemElement(
                    key: "Content",
                    name: $"AItem: {i} {string.Join("", Enumerable.Range(0,Random.Shared.Next(0,5)))}",
                    keyBindings: $"Ctrl+{i}",
                    //keyBindings: $"{i}{i}{i}{i}{i}{i}",
                    //keyBindings: $"Ctrl+{i}",
                    clickAction: OnItemClicked,
                    height: DefaultItemHeight);

                AddItem(item);
            }
        }

        public void AddItem(ContextMenuItemElement item)
        {
            _itemNamesColumn.AddChild(item.NameText);
            _itemKeyBindingsColumn.AddChild(item.ExtraContent);
            _buttonsColumn.AddChild(item.Button);
        }

        private void OnItemClicked(ContextMenuItemElement sender)
        {
            Debug.WriteLine($"Click: {sender.Name}");
        }
    }
}
