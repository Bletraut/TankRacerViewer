using System;

using ComposableUi;

using Microsoft.Xna.Framework;

namespace TankRacerViewer.Core
{
    public class LevelInspectorElement : InspectorElement<LevelView>
    {
        public const float DefaultContentSpacing = 4;

        public Action<LevelObject> LevelObjectSelectedAction { get; set; }

        private readonly FoldableGroupElement _backgroundGroup;
        private readonly DropDownListElement<DropDownListTextItemElement> _backgroundList;

        private readonly FoldableGroupElement _objectsGroup;
        private readonly LazyListViewElement<LevelObject, LevelObjectElement> _lazyListView;

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

            _backgroundList = new();
            _backgroundList.AddItem(new DropDownListTextItemElement("SHIT"));
            _backgroundList.AddItem(new DropDownListTextItemElement("FUCK"));
            _backgroundList.AddItem(new DropDownListTextItemElement("DICK"));
            _backgroundList.AddItem(new DropDownListTextItemElement("Who dem a program"));
            _backgroundList.SelectItem(0);
            _backgroundGroup.ContentLayout.AddChild(_backgroundList);

            _objectsGroup = new FoldableGroupElement(
                name: "Objects Container"
            );
            _objectsGroup.Icon.IsEnabled = false;
            _objectsGroup.ContentBackground.Color = Color.DarkSlateBlue;
            _objectsGroup.ContentLayout.LeftPadding = 0;
            _objectsGroup.ContentLayout.TopPadding = 0;
            _objectsGroup.ContentLayout.BottomPadding = 0;
            _objectsGroup.ContentLayout.ExpandChildrenCrossAxis = true;
            GroupLayout.AddChild(_objectsGroup);

            _lazyListView = new LazyListViewElement<LevelObject, LevelObjectElement>(
                itemFactory: CreateLevelObject
            );
            _lazyListView.ItemColumn.Spacing = DefaultContentSpacing;
            _lazyListView.ItemColumn.ExpandChildrenCrossAxis = true;
            _objectsGroup.ContentLayout.AddChild(_lazyListView);
        }

        private LevelObjectElement CreateLevelObject()
        {
            var element = new LevelObjectElement();
            element.TargetSelected += OnTargetSelected;

            return element;
        }

        private void OnTargetSelected(LevelObject levelObject)
        {
            LevelObjectSelectedAction?.Invoke(levelObject);
        }

        protected override void OnTargetSet()
        {
            StringBuilder.Clear();
            StringBuilder.AppendLine($"Name: {Target.FullName}");
            StringBuilder.AppendLine($"Object Containers: {Target.LevelObjectContainers.Count}");
            StringBuilder.Append($"Backgrounds: {Target.BackgroundAssetViews.Count}");
            InfoText.Text = StringBuilder.ToString();

            _lazyListView.ClearData();

            foreach (var levelObject in Target.CurrentLevelObjectContainer.LevelObjects)
                _lazyListView.AddData(levelObject);
        }
    }
}
