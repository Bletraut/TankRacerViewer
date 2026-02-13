using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public sealed class TabLayout : ContainerElement
    {
        private readonly ContainerElement _tabContainer;
        private readonly TabElement _tempTab;
        private readonly HolderElement _overlayTabHolder;

        public TabLayout()
        {
            _tabContainer = new ContainerElement();
            AddChild(new ExpandedElement(_tabContainer));

            _tempTab = TabElement.CreateNonInteractiveTab();
            _tempTab.IsEnabled = false;
            AddChild(_tempTab);

            _overlayTabHolder = new HolderElement()
            {
                IsEnabled = false
            };
            AddChild(_overlayTabHolder);
        }

        public void AddTab(TabElement tab)
        {
            tab.TabButtonPointerDown += OnTabButtonPointerDown;
            tab.TabButtonPointerUp += OnTabButtonPointerUp;
            tab.TabButtonPointerDrag += OnTabButtonPointerDrag;
            tab.SplitPreviewShown += OnSplitPreviewShown;
            tab.SplitPreviewHidden += OnSplitPreviewHidden;

            _tabContainer.AddChild(tab);
        }

        private void ShowTempTab(TabElement source, Vector2 position)
        {
            _tempTab.IsEnabled = true;
            _tempTab.InnerElement.Size = source.Size;
            _tempTab.TabButton.Icon.Sprite = source.TabButton.Icon.Sprite;
            _tempTab.TabButton.Text.Text = source.TabButton.Text.Text;
            _tempTab.Position = source.Position;
        }

        private void HideTempTab()
        {
            _tempTab.IsEnabled = false;
        }

        private void OnTabButtonPointerDown(TabElement tab, Point position)
        {
            ShowTempTab(tab, position.ToVector2());
        }

        private void OnTabButtonPointerUp(TabElement tab, Point position)
        {
            HideTempTab();
        }

        private void OnTabButtonPointerDrag(TabElement sender, Point delta)
        {
            _tempTab.Position += delta.ToVector2();
        }

        private void OnSplitPreviewShown(TabElement tab)
        {
            HideTempTab();
        }

        private void OnSplitPreviewHidden(TabElement tab)
        {
            _tempTab.IsEnabled = true;
        }
    }
}
