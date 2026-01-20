using Microsoft.Xna.Framework;

using System;

namespace ComposableUi
{
    public class HolderElement : ParentElement
    {
        public Element _innerElement;
        public Element InnerElement
        {
            get => _innerElement;
            set
            {
                if (_innerElement == value)
                    return;

                if (_innerElement != null)
                    _innerElement.Parent = null;

                _innerElement = value;
                if (_innerElement != null)
                {
                    _innerElement.Parent = this;
                }
                else
                {
                    OnStateChanged();
                }
            }
        }

        public bool HasActiveInnerElement => InnerElement != null && InnerElement.IsEnabled;

        public override int ChildCount => InnerElement != null ? 1 : 0;

        public HolderElement(Element innerElement = null)
        {
            InnerElement = innerElement;
        }

        public override void ApplySize(Vector2 size)
        {
            base.ApplySize(size);

            if (!HasActiveInnerElement)
                return;

            var childSize = InnerElement.CalculatePreferredSize();
            InnerElement.ApplySize(childSize);
        }

        public override Element GetChildAt(int index)
        {
            if (InnerElement == null)
                throw new IndexOutOfRangeException();

            if (index != 0)
                throw new IndexOutOfRangeException();

            return InnerElement;
        }

        public override void AddChild(Element child)
        {
            InnerElement = child;
        }

        public override void RemoveChild(Element child)
        {
            if (InnerElement == child)
                InnerElement = null;
        }
    }
}
