using ComposableUi;

namespace TankRacerViewer.Core
{
    public sealed class InspectorWindow : WindowElement
    {
        public InspectorWindow() : base("Inspector")
        {
            Pivot = Alignment.Center;
        }
    }
}
