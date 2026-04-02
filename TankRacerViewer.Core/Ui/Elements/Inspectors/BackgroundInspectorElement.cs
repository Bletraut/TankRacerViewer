namespace TankRacerViewer.Core
{
    public sealed class BackgroundInspectorElement : InspectorElement<BackgroundAssetView>
    {
        private readonly UsedTexturesGroupElement _usedTexturesGroup;

        public BackgroundInspectorElement() 
        {
            InfoGroup.Name.Text = "Background Info";

            _usedTexturesGroup = new UsedTexturesGroupElement(
                name: "Used Textures"
            );
            GroupLayout.AddChild(_usedTexturesGroup);
        }

        protected override void OnTargetSet()
        {
            _usedTexturesGroup.ClearMeshPartsHighlight();

            StringBuilder.Clear();
            StringBuilder.AppendLine($"Name: {Target.FullName}");
            StringBuilder.Append($"Models: {Target.ModelAssetViews.Count}");
            InfoText.Text = StringBuilder.ToString();

            _usedTexturesGroup.ApplyModels(Target.ModelAssetViews);
        }
    }
}
