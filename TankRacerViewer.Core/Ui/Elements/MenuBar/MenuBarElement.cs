using System.Collections.Generic;

using ComposableUi;

using Microsoft.Xna.Framework;

namespace TankRacerViewer.Core
{
    using ItemState = MenuBarItemElement.State;

    public sealed class MenuBarElement : ContainerElement
    {
        public const float DefaultHeight = 30;

        public const StandardSkin DefaultBackgroundSkin = StandardSkin.SoftLightPixel;

        public SpriteElement Background { get; private set; }

        private readonly RowLayout _itemsLayout;
        private readonly PointerInputHandlerElement _inputBlocker;

        private readonly List<MenuBarItemElement> _items = [];

        private MenuBarItemElement _lastSelectedItem;

        public MenuBarElement(float height = DefaultHeight)
        {
            _inputBlocker = new PointerInputHandlerElement()
            {
                IsEnabled = false
            };
            AddChild(new ExpandedElement(_inputBlocker));

            _inputBlocker.PointerDown += OnInputBlockerPointerDown;

            _itemsLayout = new RowLayout(
                alignmentFactor: Alignment.MiddleLeft,
                expandChildrenCrossAxis: true
            )
            {
                Size = new Vector2(height, DefaultHeight)
            };
            AddChild(new ExpandedElement(
                expandHeight: false,
                innerElement: new AlignmentElement(
                    alignmentFactor: Alignment.TopCenter,
                    pivot: Alignment.TopCenter,
                    innerElement: _itemsLayout
                )
            ));

            Background = new SpriteElement(
                skin: DefaultBackgroundSkin
            );
            _itemsLayout.AddChild(new LayoutElement(
                ignoreLayout: true,
                innerElement: new ExpandedElement(Background)
            ));
        }

        public void AddItem(MenuBarItemElement item)
        {
            if (_items.Contains(item))
                return;

            _items.Add(item);
            _itemsLayout.AddChild(item);

            item.Enter += OnItemEnter;
            item.Leave += OnItemLeave;
            item.Clicked += OnItemClicked;
        }

        public void RemoveItem(MenuBarItemElement item)
        {
            if (_items.Remove(item))
            {
                _itemsLayout.RemoveChild(item);

                item.Enter -= OnItemEnter;
                item.Leave -= OnItemLeave;
            }
        }

        public void ResetSelectedItem()
        {
            if (_lastSelectedItem is null)
                return;

            _lastSelectedItem.SetState(ItemState.Normal);
            _lastSelectedItem.UnselectAction?.Invoke();

            _lastSelectedItem = null;

            _inputBlocker.IsEnabled = false;
        }

        private void SelectItem(MenuBarItemElement item)
        {
            ResetSelectedItem();

            _lastSelectedItem = item;
            _lastSelectedItem.SetState(ItemState.Selected);

            _inputBlocker.IsEnabled = true;

            item.SelectAction?.Invoke();
        }

        private void OnItemEnter(MenuBarItemElement item)
        {
            if (_lastSelectedItem is not null)
            {
                SelectItem(item);
            }
            else
            {
                item.SetState(ItemState.Hover);
            }
        }

        private void OnItemLeave(MenuBarItemElement item)
        {
            if (_lastSelectedItem == item)
                return;

            item.SetState(ItemState.Normal);
        }

        private void OnItemClicked(MenuBarItemElement item)
        {
            SelectItem(item);
        }

        private void OnInputBlockerPointerDown(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            ResetSelectedItem();
        }
    }
}
