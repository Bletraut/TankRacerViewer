using System;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class LazyListViewElement<TData, TItem> : ContainerElement
        where TItem : Element, ILazyListItem<TData>
    {
        public ColumnLayout ItemColumn { get; }

        private readonly List<TData> _data;
        public IReadOnlyList<TData> Data { get; }

        private readonly List<TItem> _items;
        public IReadOnlyList<TItem> Items { get; }

        private TItem _templateItem;
        private TItem TemplateItem => _templateItem ??= _itemFactory();

        private readonly Func<TItem> _itemFactory;

        private Vector2 _preferredSize;

        public LazyListViewElement(Func<TItem> itemFactory)
        {
            _itemFactory = itemFactory;

            _data = [];
            Data = _data.AsReadOnly();

            _items = [];
            Items = _items.AsReadOnly();

            _itemFactory = itemFactory;

            Pivot = Alignment.TopLeft;

            ItemColumn = new ColumnLayout(sizeMainAxisToContent: true)
            {
                Pivot = Alignment.TopLeft,
            };
            AddChild(ItemColumn);
        }

        public int IndexOf(TData data)
        {
            return _data.IndexOf(data);
        }

        public void AddData(TData data)
        {
            _data.Add(data);
            RefreshPreferredSize(data);

            OnStateChanged();
        }

        public void InsertData(int index, TData data)
        {
            _data.Insert(index, data);
            RefreshPreferredSize(data);

            OnStateChanged();
        }

        public void InsertDataRange(int index, ReadOnlySpan<TData> collection)
        {
            _data.InsertRange(index, collection);

            foreach (var data in collection)
                RefreshPreferredSize(data);

            OnStateChanged();
        }

        public void RemoveData(TData data)
        {
            if (_data.Remove(data))
                RefreshPreferredSize();

            OnStateChanged();
        }

        public void RemoveDataRange(int index, int count)
        {
            _data.RemoveRange(index, count);
            RefreshPreferredSize();

            OnStateChanged();
        }

        public void ClearData()
        {
            _data.Clear();
            _preferredSize = Vector2.Zero;

            OnStateChanged();
        }

        public Rectangle CalculateItemBoundingRectangle(int index)
        {
            var boundingRectangle = BoundingRectangle;
            var itemSize = TemplateItem.CalculatePreferredSize();

            return new Rectangle()
            {
                Location = new Point(boundingRectangle.X, boundingRectangle.Y + (int)(index * itemSize.Y)),
                Size = itemSize.ToPoint()
            };
        }

        private void RefreshPreferredSize()
        {
            foreach (var data in _data)
                RefreshPreferredSize(data);
        }

        private void RefreshPreferredSize(TData data)
        {
            TemplateItem.SetData(data);

            var itemSize = TemplateItem.CalculatePreferredSize();
            _preferredSize = _preferredSize with
            {
                X = MathF.Max(_preferredSize.X, itemSize.X),
                Y = _data.Count * itemSize.Y + ItemColumn.Spacing * (_data.Count - 1)
            };
        }

        private TItem CreateItem()
        {
            var item = _itemFactory();
            _items.Add(item);
            ItemColumn.AddChild(item);

            return item;
        }

        public override Vector2 CalculatePreferredSize()
        {
            return _preferredSize;
        }

        public override void Rebuild(Vector2 size, bool excludeChildren)
        {
            Size = size;

            if (excludeChildren)
                return;

            for (var i = 0; i < ChildCount; i++)
            {
                var child = GetChildAt(i);
                if (!child.IsEnabled)
                    continue;

                var childSize = child.CalculatePreferredSize();

                if (child == ItemColumn)
                {
                    childSize.X = size.X;

                    var boundingRectangle = BoundingRectangle;
                    var visibleRectangle = Rectangle.Intersect(boundingRectangle, ClipMask ?? boundingRectangle);
                    var itemHeight = TemplateItem.CalculatePreferredSize().Y + ItemColumn.Spacing;

                    var offset = (visibleRectangle.Top - boundingRectangle.Top) / itemHeight;
                    var layoutOffset = offset % 1 * itemHeight;

                    var itemIndex = (int)offset;
                    var maxVisibleItemCount = (int)MathF.Ceiling((visibleRectangle.Height + layoutOffset) / itemHeight);
                    var visibleItemCount = Math.Clamp(maxVisibleItemCount, 0, _data.Count);

                    var length = int.Max(visibleItemCount, _items.Count);
                    for (var j = 0; j < length; j++)
                    {
                        var item = j >= _items.Count
                            ? CreateItem()
                            : _items[j];

                        item.IsEnabled = j < visibleItemCount;
                        if (!item.IsEnabled)
                        {
                            item.ClearData();
                            continue;
                        }

                        var dataIndex = itemIndex + j;
                        if (dataIndex < _data.Count)
                            item.SetData(_data[dataIndex]);
                    }

                    ItemColumn.Position = ItemColumn.Position with
                    {
                        Y = visibleRectangle.Top - layoutOffset
                    };
                }
                child.Rebuild(childSize);
            }
        }
    }
}
