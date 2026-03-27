using System.Text;

using ComposableUi;

namespace TankRacerViewer.Core
{
    public abstract class InspectorElement<T> : SizedToContentHolderElement
    {
        public const float DefaultGroupSpacing = 4;

        // Static.
        protected static readonly StringBuilder StringBuilder = new();

        // Class.
        public T Target { get; private set; }

        protected readonly FoldableGroupElement InfoGroup;
        protected readonly TextElement InfoText;

        protected ColumnLayout GroupLayout { get; }

        public InspectorElement()
        {
            GroupLayout = new ColumnLayout(
                spacing: DefaultGroupSpacing,
                sizeMainAxisToContent: true,
                expandChildrenCrossAxis: true
            );
            InnerElement = GroupLayout;

            InfoGroup = new FoldableGroupElement();
            InfoGroup.Icon.IsEnabled = false;
            GroupLayout.AddChild(InfoGroup);

            InfoText = new TextElement(
                sizeToTextWidth: true,
                sizeToTextHeight: true
            );
            InfoGroup.ContentLayout.AddChild(InfoText);
        }

        public void SetTarget(T target)
        {
            Target = target;
            OnTargetSet();
        }

        protected abstract void OnTargetSet();
    }
}
