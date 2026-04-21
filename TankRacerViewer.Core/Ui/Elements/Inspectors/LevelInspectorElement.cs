using System;
using System.Collections.Generic;

using ComposableUi;

namespace TankRacerViewer.Core
{
    public class LevelInspectorElement : InspectorElement<LevelView>
    {
        public const float DefaultContentSpacing = 4;

        public Action<LevelObject> LevelObjectSelectedAction
        {
            get => _containerExplorer.LevelObjectSelectedAction;
            set => _containerExplorer.LevelObjectSelectedAction = value;
        }

        private readonly FoldableGroupElement _backgroundGroup;
        private readonly DropDownListElement _backgroundDropDownList;

        private readonly FoldableGroupElement _objectsGroup;
        private readonly DropDownListElement _containerDropDownList;
        private readonly DropDownListElement _lapDropDownList;
        private readonly LevelObjectContainerExplorerElement _containerExplorer;

        private readonly Stack<DropDownListTextItemElement> _pool = [];
        private readonly List<BackgroundAssetView> _backgrounds = [];

        public LevelInspectorElement()
        {
            InfoGroup.Name.Text = "Level Info";

            _backgroundGroup = new FoldableGroupElement(
                name: "Background"
            );
            _backgroundGroup.Icon.IsEnabled = false;
            _backgroundGroup.ContentLayout.RightPadding = _backgroundGroup.ContentLayout.LeftPadding;
            _backgroundGroup.ContentLayout.ExpandChildrenCrossAxis = true;
            GroupLayout.AddChild(_backgroundGroup);

            _backgroundGroup.ContentLayout.AddChild(new TextElement(
                text: "Current:",
                textAlignmentFactor: Alignment.TopLeft,
                sizeToTextWidth: true,
                sizeToTextHeight: true
            ));

            _backgroundDropDownList = new DropDownListElement();
            _backgroundGroup.ContentLayout.AddChild(_backgroundDropDownList);
            _backgroundDropDownList.ItemSelected += OnBackgroundSelected;

            _objectsGroup = new FoldableGroupElement(
                name: "Objects"
            );
            _objectsGroup.Icon.IsEnabled = false;
            _objectsGroup.ContentLayout.RightPadding = _objectsGroup.ContentLayout.LeftPadding;
            _objectsGroup.ContentLayout.ExpandChildrenCrossAxis = true;
            GroupLayout.AddChild(_objectsGroup);

            _objectsGroup.ContentLayout.AddChild(new TextElement(
                text: "Container:",
                textAlignmentFactor: Alignment.TopLeft,
                sizeToTextWidth: true,
                sizeToTextHeight: true
            ));
            _containerDropDownList = new DropDownListElement();
            _objectsGroup.ContentLayout.AddChild(_containerDropDownList);
            _containerDropDownList.ItemSelected += OnContainerSelected;

            _objectsGroup.ContentLayout.AddChild(new TextElement(
                text: "Lap:",
                textAlignmentFactor: Alignment.TopLeft,
                sizeToTextWidth: true,
                sizeToTextHeight: true
            ));
            _lapDropDownList = new DropDownListElement(
                items: [new("1"), new("2"), new("3")]
            );
            _objectsGroup.ContentLayout.AddChild(_lapDropDownList);
            _lapDropDownList.ItemSelected += OnLapSelected;

            _containerExplorer = new LevelObjectContainerExplorerElement("Objects:");
            _objectsGroup.ContentLayout.AddChild(_containerExplorer);
        }

        private DropDownListTextItemElement GetBackgroundListItem()
        {
            if (_pool.Count > 0)
                return _pool.Pop();

            return new DropDownListTextItemElement();
        }

        private void ClearAllAndReturnToPool(DropDownListElement dropDownList)
        {
            foreach (var item in dropDownList.Items)
                _pool.Push(item);

            dropDownList.ClearItems();
        }

        private void ApplyCurrentBackgroundIfPossible()
        {
            ClearAllAndReturnToPool(_backgroundDropDownList);

            var currentIndex = 0;
            if (Target.BackgroundAssetViews.Count > 0)
            {
                _backgrounds.Clear();
                foreach (var value in Target.BackgroundAssetViews.Values)
                {
                    var listItem = GetBackgroundListItem();
                    listItem.Value.Text = value.FullName;
                    _backgroundDropDownList.AddItem(listItem);

                    if (value == Target.BackgroundAssetView)
                        currentIndex = _backgrounds.Count;

                    _backgrounds.Add(value);
                }
            }
            _backgroundDropDownList.SelectItem(currentIndex);
        }

        private void ApplyCurrentContainerIfPossible()
        {
            ClearAllAndReturnToPool(_containerDropDownList);

            var currentIndex = 0;
            for (var i = 0; i < Target.LevelObjectContainers.Count; i++)
            {
                var container = Target.LevelObjectContainers[i];

                var listItem = GetBackgroundListItem();
                listItem.Value.Text = container.FullName;
                _containerDropDownList.AddItem(listItem);

                if (container == Target.CurrentLevelObjectContainer)
                    currentIndex = i;
            }
            _containerDropDownList.SelectItem(currentIndex);

            ApplyContainer(Target.CurrentLevelObjectContainer);
        }

        private void ApplyContainer(LevelObjectContainer container)
        {
            _containerExplorer.ApplyContainer(container);
            _containerExplorer.IsEnabled = container.LevelObjects.Count > 0;
        }

        private void OnBackgroundSelected(DropDownListTextItemElement sender, int index)
        {
            if (Target.BackgroundAssetViews.Count <= 0)
                return;

            Target.BackgroundAssetView = _backgrounds[index];
        }

        private void OnContainerSelected(DropDownListTextItemElement sender, int index)
        {
            if (Target.LevelObjectContainers.Count <= 0)
                return;

            Target.CurrentLevelObjectContainer = Target.LevelObjectContainers[index];
            ApplyContainer(Target.CurrentLevelObjectContainer);
        }

        private void OnLapSelected(DropDownListTextItemElement sender, int index)
        {
            Target.CurrentLap = index + 1;
            _containerExplorer.Refresh();
        }

        protected override void OnTargetSet()
        {
            StringBuilder.Clear();
            StringBuilder.AppendLine($"Name: {Target.FullName}");
            StringBuilder.AppendLine($"Object Containers: {Target.LevelObjectContainers.Count}");
            StringBuilder.Append($"Backgrounds: {Target.BackgroundAssetViews.Count}");
            InfoText.Text = StringBuilder.ToString();

            ApplyCurrentBackgroundIfPossible();
            ApplyCurrentContainerIfPossible();

            _lapDropDownList.SelectItem(Target.CurrentLap - 1);
        }
    }
}
