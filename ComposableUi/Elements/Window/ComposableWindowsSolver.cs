namespace ComposableUi
{
    public sealed class ComposableWindowsSolver : IElementSolver
    {
        public WindowElement Source { get; private set; }
        public WindowElement SplitTarget { get; private set; }
        public WindowElement TabTarget { get; private set; }

        internal void SetSource(WindowElement window)
        {
            Source = window;
        }

        internal void SetTabTarget(WindowElement window)
        {
            TabTarget = window;
        }

        internal void ClearTabTarget(WindowElement window)
        {
            if (TabTarget == window)
                TabTarget = null;
        }

        internal void SetSplitTarget(WindowElement window)
        {
            SplitTarget = window;
        }

        public void ClearSplitTarget(WindowElement window)
        {
            if (SplitTarget == window)
                SplitTarget = null;
        }

        internal bool TryDock()
        {
            var result = Source is not null
                && (TryDockTo() || TryDockAsTab());

            Source = null;
            SplitTarget = null;
            TabTarget = null;

            return result;
        }

        private bool TryDockAsTab()
        {
            if (TabTarget is null)
                return false;

            TabTarget.DockAsTab(Source);

            return true;
        }

        private bool TryDockTo()
        {
            if (SplitTarget is null)
                return false;

            SplitTarget.Dock(Source);

            return true;
        }

        void IElementSolver.Handle(Element element)
        {
            if (element is WindowElement tab)
                tab.AttachSolver(this);
        }

        void IElementSolver.Resolve()
        {
        }
    }
}
