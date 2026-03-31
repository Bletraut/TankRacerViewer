using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    using Submenu = (ContextMenuElement Root, ContextMenuItemElement Button, ContextMenuElement Menu);

    public sealed class ContextMenuElement : ContainerElement
    {
        private const int DefaultColumnsSpacing = 20;
        private const int DefaultContentPadding = 6;
        private const int DefaultSubmenuOffset = -2;

        // Static.
        private static readonly Stack<Submenu> _stack = [];

        // Class.
        public bool _clampToRootWidth;
        public bool ClampToRootWidth
        {
            get => _clampToRootWidth;
            set => SetAndChangeState(ref _clampToRootWidth, value);
        }

        public bool _clampToRootHeight;
        public bool ClampToRootHeight
        {
            get => _clampToRootHeight;
            set => SetAndChangeState(ref _clampToRootHeight, value);
        }

        public SpriteElement Background { get; }

        private readonly List<ContextMenuItemElement> _items;
        public IReadOnlyList<ContextMenuItemElement> Items { get; }

        private readonly ColumnLayout _buttonsColumn;
        private readonly ColumnLayout _itemNamesColumn;
        private readonly ColumnLayout _itemKeyBindingsColumn;
        private readonly RowLayout _columnsRow;

        private readonly Dictionary<string, Submenu> _submenus = [];

        private readonly ElementEventHandler<ContextMenuItemElement, PointerEvent> _onItemPointerEnter;
        private readonly ElementEventHandler<ContextMenuItemElement, PointerEvent> _onItemPointerLeave;
        private readonly ElementEventHandler<ContextMenuItemElement, PointerEvent> _onItemClicked;
        private ContextMenuItemElement _currentItem;

        public ContextMenuElement(IEnumerable<ContextMenuItemElement> items = default,
            bool clampToRootWidth = true,
            bool clampToRootHeight = true)
        {
            ClampToRootWidth = clampToRootWidth;
            ClampToRootHeight = clampToRootHeight;

            Background = new SpriteElement(skin: StandardSkin.RectanglePanel);
            var backgroundParent = new LayoutElement(
                ignoreLayout: true,
                innerElement: new ExpandedElement(
                    innerElement: new PointerInputHandlerElement(Background))
            );

            _buttonsColumn = new ColumnLayout(
                expandChildrenMainAxis: true,
                expandChildrenCrossAxis: true);
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

            AddChild(_columnsRow);

            _onItemPointerEnter = OnItemPointerEnter;
            _onItemPointerLeave = OnItemPointerLeave;
            _onItemClicked = OnItemClicked;

            _items = [];
            Items = _items.AsReadOnly();

            if (items is not null)
            {
                foreach (var item in items)
                    AddItem(item);
            }
        }

        public void AddItem(ContextMenuItemElement item)
        {
            var targetMenu = this;

            var pathSegments = item.Key.Split('\\', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (pathSegments.Length > 0)
            {
                for (var i = 0; i < pathSegments.Length; i++)
                {
                    var targetMenuKey = pathSegments[i];
                    if (!targetMenu._submenus.TryGetValue(targetMenuKey, out var submenu))
                    {
                        var parentMenu = targetMenu;

                        submenu.Root = this;
                        submenu.Menu = new ContextMenuElement()
                        {
                            IsEnabled = false
                        };
                        parentMenu.AddChild(submenu.Menu);

                        submenu.Button = new ContextMenuItemElement(
                            key: targetMenuKey,
                            name: targetMenuKey);
                        submenu.Button.EnableArrow(true);
                        submenu.Button.PointerEnter += parentMenu._onItemPointerEnter;
                        submenu.Button.PointerEnter += (_, _) => parentMenu.ShowSubmenu(submenu);

                        parentMenu.AppendToView(submenu.Button);
                        parentMenu._submenus.Add(targetMenuKey, submenu);
                    }
                    targetMenu = submenu.Menu;
                }
            }

            item.PointerEnter += targetMenu._onItemPointerEnter;
            item.PointerLeave += targetMenu._onItemPointerLeave;
            item.PointerClick += _onItemClicked;

            targetMenu.AppendToView(item);
            targetMenu._items.Add(item);

            _items.Add(item);
        }

        public void RemoveItem(ContextMenuItemElement item)
        {
            if (_items.Remove(item))
            {
                _stack.Clear();
                _stack.Push((this, null, this));

                var isItemRemoved = false;
                while (_stack.Count > 0)
                {
                    var (_, _, menu) = _stack.Pop();
                    if (menu._items.Remove(item))
                    {
                        isItemRemoved = true;

                        item.PointerEnter -= menu._onItemPointerEnter;
                        item.PointerLeave -= menu._onItemPointerLeave;
                        item.PointerClick -= _onItemClicked;
                        menu.RemoveFromView(item);

                        break;
                    }

                    foreach (var submenu in menu._submenus.Values)
                        _stack.Push(submenu);
                }

                if (!isItemRemoved)
                    return;

                _stack.Clear();
                _stack.Push((this, null, this));

                while (_stack.Count > 0)
                {
                    var (_, button, menu) = _stack.Pop();
                    if (button is not null)
                        button.IsEnabled = menu.CalculateItemCount() > 0;

                    foreach (var submenu in menu._submenus.Values)
                        _stack.Push(submenu);
                }
            }
        }

        public void Show(Vector2 position)
        {
            if (Root is null)
                return;

            ResetItemsHover();
            HideAllSubmenus();

            Root.ShowInOverlay(this, position, position,
                ClampToRootWidth, ClampToRootHeight);
        }

        public void Hide()
        {
            IsEnabled = false;
        }

        private void AppendToView(ContextMenuItemElement item)
        {
            _itemNamesColumn.AddChild(item.NameText);
            _itemKeyBindingsColumn.AddChild(item.ExtraContent);
            _buttonsColumn.AddChild(item.Button);
        }

        private void RemoveFromView(ContextMenuItemElement item)
        {
            _itemNamesColumn.RemoveChild(item.NameText);
            _itemKeyBindingsColumn.RemoveChild(item.ExtraContent);
            _buttonsColumn.RemoveChild(item.Button);
        }

        private int CalculateItemCount()
        {
            var count = _items.Count;
            foreach (var submenu in _submenus.Values)
                count += submenu.Menu.CalculateItemCount();

            return count;
        }

        private void ShowSubmenu(Submenu submenu)
        {
            submenu.Menu.ResetItemsHover();
            submenu.Menu.HideAllSubmenus();

            var boundingRectangle = submenu.Button.Button.BoundingRectangle;

            var topLeftPosition = new Vector2(boundingRectangle.Left, boundingRectangle.Top)
                + new Vector2(-DefaultContentPadding - DefaultSubmenuOffset, -DefaultContentPadding);
            var topRightPosition = new Vector2(boundingRectangle.Right, boundingRectangle.Top)
                + new Vector2(DefaultContentPadding + DefaultSubmenuOffset, -DefaultContentPadding);

            var size = submenu.Menu.CalculatePreferredSize();
            var position = topRightPosition + size * submenu.Menu.Pivot;
            var fallbackPosition = topLeftPosition + size * submenu.Menu.Pivot with { X = submenu.Menu.Pivot.X - 1};

            Root.ShowInOverlay(submenu.Menu, position, fallbackPosition,
                ClampToRootWidth, ClampToRootHeight);
        }

        private void ResetItemsHover()
        {
            _currentItem = null;
            foreach (var item in Items)
                item.SetHover(false);

            foreach (var submenu in _submenus.Values)
                submenu.Button.SetHover(false);
        }

        private void HideAllSubmenus()
        {
            foreach (var submenu in _submenus.Values)
                submenu.Menu.IsEnabled = false;
        }

        public override Vector2 CalculatePreferredSize()
        {
            return _columnsRow.CalculatePreferredSize();
        }

        public override void Rebuild(Vector2 size, bool excludeChildren)
        {
            Size = size;

            for (var i = 0; i < ChildCount; i++)
            {
                var child = GetChildAt(i);
                if (!child.IsEnabled)
                    continue;

                var childSize = child.CalculatePreferredSize();

                if (child == _columnsRow)
                {
                    child.Size = childSize;
                    _columnsRow.LocalPosition = _columnsRow.PivotOffset - PivotOffset;
                }
                child.Rebuild(childSize);
            }
        }

        private void OnItemPointerEnter(ContextMenuItemElement item,
            PointerEvent pointerEvent)
        {
            HideAllSubmenus();
            _currentItem?.SetHover(false);

            _currentItem = item;
            _currentItem.SetHover(true);
        }

        private void OnItemPointerLeave(ContextMenuItemElement item,
            PointerEvent pointerEvent)
        {
            if (_currentItem == item)
                _currentItem = null;

            item.SetHover(false);
        }

        private void OnItemClicked(ContextMenuItemElement item,
            PointerEvent pointerEvent)
        {
            item.ClickAction?.Invoke(item);
        }
    }
}
