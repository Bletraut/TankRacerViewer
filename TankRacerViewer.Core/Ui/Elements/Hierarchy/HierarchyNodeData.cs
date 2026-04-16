using System.Collections.Generic;

using ComposableUi;

namespace TankRacerViewer.Core
{
    public sealed class HierarchyNodeData
    {
        public const float DefaultIndent = 22;

        public HierarchyNodeData Parent { get; private set; }
        public List<HierarchyNodeData> Children { get; } = [];

        public object File { get; set; }

        public string Name { get; set; }
        public Sprite Sprite { get; set; }
        public StandardSkin Skin { get; set; }

        public float Indent => Parent is not null && !Parent.IsHidden ? Parent.Indent + DefaultIndent : 0;

        public bool IsFolded { get; set; }
        public bool IsSelected { get; set; }
        public bool IsHidden { get; set; }

        public void AddChild(HierarchyNodeData child)
        {
            if (child.Parent == this)
                return;

            child.Parent?.RemoveChild(this);

            child.Parent = this;
            Children.Add(child);
        }

        public void RemoveChild(HierarchyNodeData child)
        {
            Children.Remove(child);
        }
    }
}
