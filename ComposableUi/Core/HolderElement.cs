using System.Collections.Generic;
using Microsoft.Xna.Framework;

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

                _children[0] = _innerElement;
            }
        }

        public bool HasActiveInnerElement => InnerElement != null && InnerElement.IsEnabled;

        private readonly Element[] _children = new Element[1];

        public HolderElement(Element innerElement = null)
        {
            InnerElement = innerElement;
            Children = _children.AsReadOnly();
        }

        public override void ApplySize(Vector2 size)
        {
            base.ApplySize(size);

            if (!HasActiveInnerElement)
                return;

            var childSize = InnerElement.CalculatePreferredSize();
            InnerElement.ApplySize(childSize);
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
