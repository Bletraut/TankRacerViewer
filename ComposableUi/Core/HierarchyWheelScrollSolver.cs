using System;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public sealed class HierarchyWheelScrollSolver : IElementSolver
    {
        private Vector2 Axis;
        private int Delta;
        private Action<Vector2, int> ScrollAction;

        public void AddScrollIntent(Vector2 axis, int delta, Action<Vector2, int> scrollAction)
        {
            Axis = axis;
            Delta = delta;
            ScrollAction = scrollAction;
        }

        void IElementSolver.Handle(Element element)
        {
            if (element is IHierarchyWheelScrollable hierarchyWheelScrollable)
                hierarchyWheelScrollable.AttachSolver(this);
        }

        void IElementSolver.Resolve()
        {
            ScrollAction?.Invoke(Axis, Delta);
            ScrollAction = null;
        }
    }
}
