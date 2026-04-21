using System;

using ComposableUi;

namespace TankRacerViewer.Core
{
    public class TankInspectorElement : InspectorElement<TankView>
    {
        public Action<LevelObject> LevelObjectSelectedAction
        {
            get => _containerExplorer.LevelObjectSelectedAction;
            set => _containerExplorer.LevelObjectSelectedAction = value;
        }

        private readonly FoldableGroupElement _nodesGroup;
        private readonly LevelObjectContainerExplorerElement _containerExplorer;

        public TankInspectorElement()
        {
            InfoGroup.Name.Text = "Tank Info";

            _nodesGroup = new FoldableGroupElement(
                name: "Nodes Container"
            );
            _nodesGroup.Icon.IsEnabled = false;
            _nodesGroup.ContentLayout.RightPadding = _nodesGroup.ContentLayout.LeftPadding;
            _nodesGroup.ContentLayout.ExpandChildrenCrossAxis = true;
            GroupLayout.AddChild(_nodesGroup);

            _containerExplorer = new LevelObjectContainerExplorerElement("Nodes:");
            _nodesGroup.ContentLayout.AddChild(_containerExplorer);
        }

        protected override void OnTargetSet()
        {
            StringBuilder.Clear();
            StringBuilder.AppendLine($"Name: {Target.FullName}");
            StringBuilder.Append($"Nodes: {Target.TankNodeContainer.LevelObjects.Count}");
            InfoText.Text = StringBuilder.ToString();

            _containerExplorer.ApplyContainer(Target.TankNodeContainer);
        }
    }
}
