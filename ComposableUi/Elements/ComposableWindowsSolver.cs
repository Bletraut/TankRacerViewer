using System.Diagnostics;

namespace ComposableUi
{
    public sealed class ComposableWindowsSolver : IElementSolver
    {
        public Window2Element Source { get; private set; }
        public Window2Element Target { get; private set; }

        public void SelectSource(Window2Element window)
        {
            Source = window;
        }

        public void ReleaseSource(Window2Element window)
        {
            if (Source == window)
                Source = null;
        }

        public void SelectTarget(Window2Element window)
        {
            Target = window;
        }

        public void ReleaseTarget(Window2Element window)
        {
            if (Target == window)
                Target = null;
        }

        public bool TryAttach()
        {
            var canAttach = Source is not null 
                && Target is not null;
            if (canAttach)
                Target.Attach(Source);

            Source = null;
            Target = null;

            return canAttach;
        }

        void IElementSolver.Handle(Element element)
        {
            if (element is Window2Element tab)
                tab.AttachSolver(this);
        }

        void IElementSolver.Resolve()
        {
        }
    }
}
