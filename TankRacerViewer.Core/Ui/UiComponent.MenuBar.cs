using System;
using System.Collections.Generic;
using System.IO;

using ComposableUi;

using Microsoft.Xna.Framework;

namespace TankRacerViewer.Core
{
    public sealed partial class UiComponent
    {
        private const string RecentPathsKey = "Open Recent";

        // Static.
        private static readonly ItemFilter[] _itemFilters = [
            new("FastFile files", "DAT"),
            new("All Files", "*")
        ];

        private static void ShowContextMenu(ContextMenuElement menu, MenuBarItemElement item)
        {
            var boundingRectangle = item.BoundingRectangle;
            menu.Show(new Vector2(boundingRectangle.Left, boundingRectangle.Bottom));
        }

        // Class.
        public List<string> RecentPaths { get; set; }

        private readonly List<ContextMenuItemElement> _recentPathItems = [];
        private readonly Stack<ContextMenuItemElement> _pool = [];

        private MenuBarElement _menuBar;

        private MenuBarItemElement _fileMenuItem;
        private MenuBarItemElement _editMenuItem;
        private MenuBarItemElement _windowMenuItem;
        private MenuBarItemElement _viewMenuItem;
        private MenuBarItemElement _aboutMenuItem;

        private ContextMenuElement _fileContextMenu;
        private ContextMenuElement _editContextMenu;
        private ContextMenuElement _viewContextMenu;
        private ContextMenuElement _windowContextMenu;
        private ContextMenuElement _aboutContextMenu;

        private ContextMenuItemElement _clearRecentPathsMenuItem;
        private ContextMenuItemElement _recentPathsMenuItem;

        public void RefreshRecentPaths()
        {
            foreach (var item in _recentPathItems)
            {
                _fileContextMenu.RemoveItem(item);
                _pool.Push(item);
            }
            _fileContextMenu.RemoveItem(_clearRecentPathsMenuItem);

            _recentPathItems.Clear();

            foreach (var path in RecentPaths)
            {
                var item = GetRecentPathMenuItem();
                item.Name = path;
                Action<string> action = Directory.Exists(path)
                    ? _mainWindow.OpenGameFolder
                    : _mainWindow.OpenFile;
                item.ClickAction = _ => SelectContextMenuItem(() => action(path));
                _fileContextMenu.AddItem(item);

                _recentPathItems.Add(item);
            }
            _fileContextMenu.AddItem(_clearRecentPathsMenuItem);

            _recentPathsMenuItem.IsInteractable = RecentPaths.Count > 0;
        }

        private void CreateMenuBar()
        {
            _clearRecentPathsMenuItem = new ContextMenuItemElement(
                key: RecentPathsKey,
                name: "Clear Recent Paths",
                clickAction: _ => SelectContextMenuItem(_mainWindow.ClearRecentPaths)
            );

            _fileContextMenu = CreateAndAddContextMenu([
                new ContextMenuItemElement(
                    name: "Open Game Folder...",
                    keyBindings: "Ctrl+O",
                    clickAction: _ => SelectContextMenuItem(OpenGameFolder)
                ),
                new ContextMenuItemElement(
                    name: "Open File...",
                    clickAction: _ => SelectContextMenuItem(OpenFile)
                ),
                _clearRecentPathsMenuItem,
                new ContextMenuItemElement(
                    name: "Exit",
                    keyBindings: "Alt+F4",
                    clickAction: _ => SelectContextMenuItem(_mainWindow.Exit)
                ),
            ]);
            if (_fileContextMenu.TryGetSubmenu(RecentPathsKey, out var submenu))
                _recentPathsMenuItem = submenu.Button;

            _editContextMenu = CreateAndAddContextMenu([
                new ContextMenuItemElement(
                    name: "Try Recreate Extra Assets Views",
                    clickAction: _ => SelectContextMenuItem(RecreateAllExtraAssetViewsIfPossible)
                )
            ]);

            _viewContextMenu = CreateAndAddContextMenu([
                new ContextMenuItemElement(
                    keyBindings: "Ctrl+F",
                    name: "Advanced Mode",
                    clickAction: _ => SelectContextMenuItem(_mainWindow.ToggleRenderContext)
                )
            ]);

            _windowContextMenu = CreateAndAddContextMenu([
                new ContextMenuItemElement(
                    name: "Viewer",
                    clickAction: _ => SelectContextMenuItem(() => ShowWindow(ViewerWindow))
                ),
                new ContextMenuItemElement(
                    name: "Explorer",
                    clickAction: _ => SelectContextMenuItem(() => ShowWindow(ExplorerWindow))
                ),
                new ContextMenuItemElement(
                    name: "Inspector",
                    clickAction: _ => SelectContextMenuItem(() => ShowWindow(InspectorWindow))
                ),
                new ContextMenuItemElement(
                    name: "Console",
                    clickAction: _ => SelectContextMenuItem(() => ShowWindow(ConsoleWindow))
                ),
            ]);

            _aboutContextMenu = CreateAndAddContextMenu([
                new ContextMenuItemElement(
                    name: "About",
                    clickAction: _ => SelectContextMenuItem(ShowAboutWindow)
                )
            ]);

            _menuBar = new MenuBarElement();
            _mainLayer.AddChild(new ExpandedElement(_menuBar));

            _fileMenuItem = new MenuBarItemElement("File",
                () => ShowContextMenu(_fileContextMenu, _fileMenuItem),
                _fileContextMenu.Hide);
            _menuBar.AddItem(_fileMenuItem);

            _editMenuItem = new MenuBarItemElement("Edit",
                () => ShowContextMenu(_editContextMenu, _editMenuItem),
                _editContextMenu.Hide);
            _menuBar.AddItem(_editMenuItem);

            _viewMenuItem = new MenuBarItemElement("View",
                () => ShowContextMenu(_viewContextMenu, _viewMenuItem),
                _viewContextMenu.Hide);
            _menuBar.AddItem(_viewMenuItem);

            _windowMenuItem = new MenuBarItemElement("Window",
                () => ShowContextMenu(_windowContextMenu, _windowMenuItem),
                _windowContextMenu.Hide);
            _menuBar.AddItem(_windowMenuItem);

            _aboutMenuItem = new MenuBarItemElement("About",
                () => ShowContextMenu(_aboutContextMenu, _aboutMenuItem),
                _aboutContextMenu.Hide);
            _menuBar.AddItem(_aboutMenuItem);
        }

        private ContextMenuItemElement GetRecentPathMenuItem()
        {
            if (_pool.Count > 0)
                return _pool.Pop();

            return new ContextMenuItemElement(
                key: RecentPathsKey
            );
        }

        private ContextMenuElement CreateAndAddContextMenu(IEnumerable<ContextMenuItemElement> items)
        {
            var contextMenu = new ContextMenuElement(items)
            {
                Pivot = Alignment.TopLeft,
                IsEnabled = false
            };
            _overlayLayer.AddChild(contextMenu);

            return contextMenu;
        }

        private void SelectContextMenuItem(Action action)
        {
            _menuBar.ResetSelectedItem();
            action?.Invoke();
        }

        private void OpenGameFolder()
        {
            var folderPath = _mainWindow.FileDialogProvider.OpenFolderDialog();
            if (string.IsNullOrEmpty(folderPath))
                return;

            _mainWindow.OpenGameFolder(folderPath);
        }

        private void OpenFile()
        {
            var filePath = _mainWindow.FileDialogProvider.OpenFileDialog(_itemFilters);
            if (string.IsNullOrEmpty(filePath))
                return;

            _mainWindow.OpenFile(filePath);
        }

        private void RecreateAllExtraAssetViewsIfPossible()
        {
            _mainWindow.RecreateAllExtraAssetViewsIfPossible();
        }
    }
}
