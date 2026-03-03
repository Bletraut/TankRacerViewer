namespace ComposableUi
{
    public sealed class ComposableWindows3Solver : IElementSolver
    {
        public Window3Element Source { get; private set; }
        public Window3Element SplitTarget { get; private set; }
        public Window3Element TabTarget { get; private set; }

        internal void SetSource(Window3Element window)
        {
            Source = window;
        }

        internal void SetTabTarget(Window3Element window)
        {
            TabTarget = window;
        }

        internal void ClearTabTarget(Window3Element window)
        {
            if (TabTarget == window)
                TabTarget = null;
        }

        internal void SetSplitTarget(Window3Element window)
        {
            SplitTarget = window;
        }

        public void ClearSplitTarget(Window3Element window)
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
            if (element is Window3Element tab)
                tab.AttachSolver(this);
        }

        void IElementSolver.Resolve()
        {
        }
    }
}
