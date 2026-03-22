namespace ComposableUi
{
    public interface ILazyListItem<TData>
    {
        public void SetData(TData data);
        public void ClearData();
    }
}
