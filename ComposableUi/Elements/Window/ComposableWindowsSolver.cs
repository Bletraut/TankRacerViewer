namespace ComposableUi
{
    public sealed class ComposableWindowsSolver : IElementSolver
    {
        public Window2Element Source { get; private set; }
        public Window2Element AttachTarget { get; private set; }
        public Window2Element InsertTarget { get; private set; }

        public void SelectSource(Window2Element window)
        {
            Source = window;
        }

        public void SelectInsertTarget(Window2Element window)
        {
            InsertTarget = window;
        }

        public void ReleaseInsertTarget(Window2Element window)
        {
            if (InsertTarget == window)
                InsertTarget = null;
        }

        public void SelectAttachTarget(Window2Element window)
        {
            AttachTarget = window;
        }

        public void ReleaseAttachTarget(Window2Element window)
        {
            if (AttachTarget == window)
                AttachTarget = null;
        }

        public CompositionResult TryCompose()
        {
            var result = Source switch
            {
                not null when TryAttach() => CompositionResult.Attached,
                not null when TryInsert() => CompositionResult.Inserted,
                _ => CompositionResult.None,
            };

            Source = null;
            AttachTarget = null;
            InsertTarget = null;

            return result;
        }

        private bool TryInsert()
        {
            if (InsertTarget is null)
                return false;

            InsertTarget.Insert(Source);

            return true;
        }

        private bool TryAttach()
        {
            if (AttachTarget is null)
                return false;

            AttachTarget.Attach(Source);

            return true;
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

    public enum CompositionResult
    {
        None,

        Attached,
        Inserted
    }
}
