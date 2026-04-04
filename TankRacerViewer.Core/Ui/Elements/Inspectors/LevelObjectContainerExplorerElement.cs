using System;

using ComposableUi;

namespace TankRacerViewer.Core
{
    public sealed class LevelObjectContainerExplorerElement : SizedToContentHolderElement
    {
        public const float DefaultContentSpacing = 4;

        // Static.
        private static VisibilityMode DetermineVisibilityMode(int gameTypeCount, int editorTypeCount,
            int visibleGameTypeCount, int visibleEditorTypeCount)
        {
            var areAllInvisible = visibleGameTypeCount == 0
                && visibleEditorTypeCount == 0;
            if (areAllInvisible)
                return VisibilityMode.HideAll;

            var areOnlyGameTypesVisible = gameTypeCount == visibleGameTypeCount
                && visibleEditorTypeCount == 0;
            if (areOnlyGameTypesVisible)
                return VisibilityMode.ShowGameTypesOnly;

            var areOnlyEditorTypesVisible = editorTypeCount == visibleEditorTypeCount
                && visibleGameTypeCount == 0;
            if (areOnlyEditorTypesVisible)
                return VisibilityMode.ShowEditorTypesOnly;

            var areAllVisible = gameTypeCount == visibleGameTypeCount
                && editorTypeCount == visibleEditorTypeCount;
            if (areAllVisible)
                return VisibilityMode.ShowAll;

            return VisibilityMode.ResetAll;
        }

        private static BoundingBoxMode DetermineBoundingBoxMode(int objectCount,
            int visibleBoundingBoxCount)
        {
            if (visibleBoundingBoxCount == 0)
                return BoundingBoxMode.HideAll;

            if (visibleBoundingBoxCount == objectCount)
                return BoundingBoxMode.ShowAll;

            return BoundingBoxMode.ResetAll;
        }

        private static VisibilityMode GetNextVisibilityMode(VisibilityMode currentVisibilityMode)
        {
            return currentVisibilityMode switch
            {
                VisibilityMode.ResetAll => VisibilityMode.ShowAll,
                VisibilityMode.ShowAll => VisibilityMode.ShowGameTypesOnly,
                VisibilityMode.ShowGameTypesOnly => VisibilityMode.ShowEditorTypesOnly,
                VisibilityMode.ShowEditorTypesOnly => VisibilityMode.HideAll,
                VisibilityMode.HideAll => VisibilityMode.ShowAll,
                _ => VisibilityMode.ShowAll
            };
        }

        private static BoundingBoxMode GetNextBoundingMode(BoundingBoxMode currentBoundingBoxMode)
        {
            return currentBoundingBoxMode switch
            {
                BoundingBoxMode.ResetAll => BoundingBoxMode.ShowAll,
                BoundingBoxMode.ShowAll => BoundingBoxMode.HideAll,
                BoundingBoxMode.HideAll => BoundingBoxMode.ShowAll,
                _ => BoundingBoxMode.ShowAll
            };
        }

        // Class.
        public Action<LevelObject> LevelObjectSelectedAction { get; set; }

        private readonly ContentButtonElement _visibilityModeButton;
        private readonly ContentButtonElement _boundingBoxModeButton;
        private readonly RowLayout _modeButtonLayout;

        private readonly LazyListViewElement<LevelObject, LevelObjectElement> _lazyListView;

        private LevelObjectContainer _currentContainer;

        private VisibilityMode _currentVisibilityMode;
        private BoundingBoxMode _currentBoundingBoxMode;

        public LevelObjectContainerExplorerElement(string title)
        {
            _visibilityModeButton = LevelObjectElement.CreateButton();
            _visibilityModeButton.PointerClick += OnVisibilityModeButtonPointerClick;

            _boundingBoxModeButton = LevelObjectElement.CreateButton();
            _boundingBoxModeButton.PointerClick += OnBoundingBoxModeButtonPointerClick;

            _modeButtonLayout = new RowLayout(
                alignmentFactor: Alignment.MiddleLeft,
                leftPadding: LevelObjectElement.DefaultContentPaddings,
                rightPadding: LevelObjectElement.DefaultContentPaddings,
                topPadding: LevelObjectElement.DefaultContentPaddings,
                bottomPadding: LevelObjectElement.DefaultContentPaddings,
                spacing: DefaultContentSpacing,
                sizeCrossAxisToContent: true,
                children: [
                    new LayoutElement(
                        ignoreLayout: true,
                        innerElement: new ExpandedElement(
                            innerElement: new SpriteElement(
                                skin: StandardSkin.WhitePixel,
                                color: LevelObjectElement.DefaultNormalBackgroundColor
                            )
                        )
                    ),
                    _visibilityModeButton,
                    _boundingBoxModeButton
                ]
            );

            _lazyListView = new LazyListViewElement<LevelObject, LevelObjectElement>(
                itemFactory: CreateLevelObject
            );
            _lazyListView.ItemColumn.Spacing = DefaultContentSpacing;
            _lazyListView.ItemColumn.ExpandChildrenCrossAxis = true;

            InnerElement = new ColumnLayout(
                alignmentFactor: Alignment.TopLeft,
                spacing: DefaultContentSpacing,
                sizeMainAxisToContent: true,
                sizeCrossAxisToContent: true,
                expandChildrenCrossAxis: true,
                children: [
                    new TextElement(
                        text: title,
                        textAlignmentFactor: Alignment.TopLeft,
                        sizeToTextWidth: true,
                        sizeToTextHeight: true
                    ),
                    _modeButtonLayout,
                    _lazyListView
                ]
            );
        }

        public void ApplyContainer(LevelObjectContainer container)
        {
            if (_currentContainer == container)
                return;

            _currentContainer = container;

            _lazyListView.ClearData();
            foreach (var levelObject in _currentContainer.LevelObjects)
                _lazyListView.AddData(levelObject);

            RefreshVisibilityModes();
        }

        public void Refresh()
        {
            RefreshVisibilityModes();
            RefreshLazyListViewItems();
        }

        private LevelObjectElement CreateLevelObject()
        {
            var element = new LevelObjectElement();
            element.VisibilityChanged += OnLevelObjectVisibilityChanged;
            element.BoundingBoxVisibilityChanged += OnLevelObjectBoundingBoxVisibilityChanged;
            element.TargetSelected += OnLevelObjectSelected;

            return element;
        }

        private void RefreshVisibilityModes()
        {
            var gameTypeCount = 0;
            var editorTypeCount = 0;
            var visibleGameTypeCount = 0;
            var visibleEditorTypeCount = 0;
            var visibleBoundingBoxCount = 0;

            foreach (var levelObject in _currentContainer.LevelObjects)
            {
                var value = levelObject.IsEnabled ? 1 : 0;
                if (levelObject.IsEditorType)
                {
                    editorTypeCount++;
                    visibleEditorTypeCount += value;
                }
                else
                {
                    gameTypeCount++;
                    visibleGameTypeCount += value;
                }

                var isBoundingBoxEnabled = levelObject.IsBoundingBoxEnabled
                    && levelObject.BoundingBoxColor == LevelObjectElement.EnabledBoundingBoxColor;
                visibleBoundingBoxCount += isBoundingBoxEnabled ? 1 : 0;
            }

            _currentVisibilityMode = DetermineVisibilityMode(gameTypeCount, editorTypeCount,
                visibleGameTypeCount, visibleEditorTypeCount);

            _currentBoundingBoxMode = DetermineBoundingBoxMode(_currentContainer.LevelObjects.Count,
                visibleBoundingBoxCount);

            RefreshVisibilityModeButtonVisualState();
            RefreshBoundingBoxModeButtonVisualState();
        }

        private void RefreshVisibilityModeButtonVisualState()
        {
            _visibilityModeButton.Icon.Skin = _currentVisibilityMode switch
            {
                VisibilityMode.ResetAll => StandardSkin.PressedRoundedButton,
                VisibilityMode.ShowAll => StandardSkin.PressedRoundedButton,
                VisibilityMode.ShowGameTypesOnly => StandardSkin.HoverRectangleButton,
                VisibilityMode.ShowEditorTypesOnly => StandardSkin.PressedRectangleButton,
                VisibilityMode.HideAll => StandardSkin.DisabledRoundedButton,
                _ => StandardSkin.PressedRoundedButton,
            };
        }

        private void RefreshBoundingBoxModeButtonVisualState()
        {
            _boundingBoxModeButton.Icon.Skin = _currentBoundingBoxMode switch
            {
                BoundingBoxMode.ResetAll => StandardSkin.PressedRoundedButton,
                BoundingBoxMode.ShowAll => StandardSkin.PressedRoundedButton,
                BoundingBoxMode.HideAll => StandardSkin.DisabledRoundedButton,
                _ => StandardSkin.PressedRoundedButton
            };
        }

        private void RefreshLazyListViewItems()
        {
            foreach (var item in _lazyListView.Items)
                item.RefreshButtonsVisualState();
        }

        private void OnVisibilityModeButtonPointerClick(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            _currentVisibilityMode = GetNextVisibilityMode(_currentVisibilityMode);

            foreach (var levelObject in _currentContainer.LevelObjects)
            {
                switch (_currentVisibilityMode)
                {
                    case VisibilityMode.ShowAll:
                        levelObject.IsEnabled = true;
                        break;
                    case VisibilityMode.ShowGameTypesOnly:
                        levelObject.IsEnabled = !levelObject.IsEditorType;
                        break;
                    case VisibilityMode.ShowEditorTypesOnly:
                        levelObject.IsEnabled = levelObject.IsEditorType;
                        break;
                    case VisibilityMode.HideAll:
                        levelObject.IsEnabled = false;
                        break;
                }
            }

            RefreshVisibilityModes();
            RefreshLazyListViewItems();
        }

        private void OnBoundingBoxModeButtonPointerClick(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            _currentBoundingBoxMode = GetNextBoundingMode(_currentBoundingBoxMode);

            foreach (var levelObject in _currentContainer.LevelObjects)
            {
                switch (_currentBoundingBoxMode)
                {
                    case BoundingBoxMode.ShowAll:
                        levelObject.IsBoundingBoxEnabled = true;
                        levelObject.BoundingBoxColor = LevelObjectElement.EnabledBoundingBoxColor;
                        break;
                    case BoundingBoxMode.HideAll:
                        levelObject.IsBoundingBoxEnabled = false;
                        levelObject.BoundingBoxColor = LevelObjectElement.SelectedBoundingBoxColor;
                        break;
                }
            }

            RefreshVisibilityModes();
            RefreshLazyListViewItems();
        }

        private void OnLevelObjectVisibilityChanged(LevelObject levelObject)
        {
            RefreshVisibilityModes();
        }

        private void OnLevelObjectBoundingBoxVisibilityChanged(LevelObject levelObject)
        {
            RefreshVisibilityModes();
        }

        private void OnLevelObjectSelected(LevelObject levelObject)
        {
            LevelObjectSelectedAction?.Invoke(levelObject);
        }

        private enum VisibilityMode
        {
            ResetAll,
            ShowAll,
            ShowGameTypesOnly,
            ShowEditorTypesOnly,
            HideAll
        }

        private enum BoundingBoxMode
        {
            ResetAll,
            ShowAll,
            HideAll
        }
    }
}
