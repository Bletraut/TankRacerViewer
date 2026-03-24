using System.Text;

namespace TankRacerViewer.Core
{
    public sealed class TextureInspectorElement : InspectorElement<TextureAssetView>
    {
        public TextureInspectorElement() 
        {
            InfoGroup.Name.Text = "Texture Info";
        }

        protected override void OnTargetSet()
        {
            StringBuilder.Clear();
            StringBuilder.AppendLine($"Name: {Target.FullName}");
            StringBuilder.AppendLine($"Width: {Target.Texture.Width}");
            StringBuilder.AppendLine($"Height: {Target.Texture.Height}");
            StringBuilder.Append($"Blend Mode: {Target.BlendMode}");
            InfoText.Text = StringBuilder.ToString();
        }
    }
}
