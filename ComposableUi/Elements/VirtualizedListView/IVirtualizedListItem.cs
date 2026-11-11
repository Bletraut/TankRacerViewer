namespace ComposableUi
{
    public interface IVirtualizedListItem<TData>
    {
        public void SetData(TData data);
        public void ClearData();
    }
}
