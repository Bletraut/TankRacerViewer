using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using ComposableUi;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using TankRacerViewer.Core.Ui;

namespace TankRacerViewer.Core
{
    public sealed partial class UiComponent
    {
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
        private MenuBarElement _menuBar;

        private MenuBarItemElement _fileMenuItem;
        private MenuBarItemElement _editMenuItem;
        private MenuBarItemElement _windowMenuItem;
        private MenuBarItemElement _viewMenuItem;
        private MenuBarItemElement _aboutMenuItem;

        private ContextMenuElement _fileContextMenu;
        private ContextMenuElement _editContextMenu;
        private ContextMenuElement _windowContextMenu;
        private ContextMenuElement _viewContextMenu;
        private ContextMenuElement _aboutContextMenu;

        private void CreateMenuBar()
        {
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
                new ContextMenuItemElement(
                    name: "Exit",
                    keyBindings: "Alt+F4",
                    clickAction: _ => SelectContextMenuItem(null)
                ),
            ]);

            _editContextMenu = CreateAndAddContextMenu([
                new ContextMenuItemElement(
                    name: "Try Recreate Extra Assets Views",
                    clickAction: _ => SelectContextMenuItem(RecreateAllExtraAssetViewsIfPossible)
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

            _viewContextMenu = CreateAndAddContextMenu([
                new ContextMenuItemElement(
                    keyBindings: "Ctrl+F",
                    name: "Advanced Mode",
                    clickAction: _ => SelectContextMenuItem(_mainWindow.ToggleRenderContext)
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

            _windowMenuItem = new MenuBarItemElement("Window",
                () => ShowContextMenu(_windowContextMenu, _windowMenuItem),
                _windowContextMenu.Hide);
            _menuBar.AddItem(_windowMenuItem);

            _viewMenuItem = new MenuBarItemElement("View",
                () => ShowContextMenu(_viewContextMenu, _viewMenuItem),
                _viewContextMenu.Hide);
            _menuBar.AddItem(_viewMenuItem);

            _aboutMenuItem = new MenuBarItemElement("About", null, null);
            _menuBar.AddItem(_aboutMenuItem);
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

            if (!Directory.Exists(folderPath))
            {
                ConsoleWindow.LogMessage(MessageType.Error, $"Directory not found: '{folderPath}'.");
                return;
            }

            var filePaths = Directory.GetFiles(folderPath, "*.dat", SearchOption.AllDirectories);
            _mainWindow.OpenGameFolder(filePaths);
        }

        private void OpenFile()
        {
            var filePath = _mainWindow.FileDialogProvider.OpenFileDialog(_itemFilters);
            if (string.IsNullOrEmpty(filePath))
                return;

            if (!File.Exists(filePath))
            {
                ConsoleWindow.LogMessage(MessageType.Error, $"File not found: '{filePath}'.");
                return;
            }

            _mainWindow.OpenFile(filePath);
        }

        private void RecreateAllExtraAssetViewsIfPossible()
        {
            _mainWindow.RecreateAllExtraAssetViewsIfPossible();
        }
    }
}
