using System.Diagnostics;

namespace ComposableUi
{
    public sealed class ComposableTabsSolver : IElementSolver
    {
        public TabElement SelectedTab { get; private set; }

        public void Select(TabElement tab)
        {
            SelectedTab = tab;
        }

        public void Release(TabElement tab)
        {
            if (SelectedTab == tab)
                SelectedTab = null;
        }

        void IElementSolver.Handle(Element element)
        {
            if (element is TabElement tab)
                tab.AttachSolver(this);
        }

        void IElementSolver.Resolve()
        {
        }
    }
}
