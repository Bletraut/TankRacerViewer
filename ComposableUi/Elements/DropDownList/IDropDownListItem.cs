namespace ComposableUi.Elements.DropDownList
{
    public interface IDropDownListItem<TItem>
    {
        public bool IsSelectable { get; set; }

        public event ElementEventHandler<TItem> Selected;

        public void SetSelected(bool value);

        public TItem CreateEmpty();
        public void Clone(TItem source);
    }
}
