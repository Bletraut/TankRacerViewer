namespace ComposableUi
{
    public interface IVirtualizingListItem<TData>
    {
        public void SetData(TData data);
        public void ClearData();
    }
}
