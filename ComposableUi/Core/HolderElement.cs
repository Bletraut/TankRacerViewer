using Microsoft.Xna.Framework;

using System;

namespace ComposableUi
{
    public class HolderElement : ParentElement
    {
        private Element _innerElement;
        public Element InnerElement
        {
            get => _innerElement;
            set
            {
                if (_innerElement == value)
                    return;

                if (_innerElement is not null)
                    _innerElement.Parent = null;

                _innerElement = value;
                if (_innerElement is not null)
                {
                    _innerElement.Parent?.RemoveChild(_innerElement);
                    _innerElement.Parent = this;
                }

                OnStateChanged();
            }
        }

        public bool HasEnabledInnerElement => InnerElement is not null && InnerElement.IsEnabled;

        public override int ChildCount => InnerElement is not null ? 1 : 0;

        public HolderElement(Element innerElement = default,
            Vector2? size = default,
            Vector2? pivot = default)
        {
            InnerElement = innerElement;

            Size = size ?? Vector2.Zero;
            Pivot = pivot ?? Alignment.Center;
        }

        protected internal override void ApplyRoot(RootElement root)
        {
            base.ApplyRoot(root);

            InnerElement?.ApplyRoot(root);
        }

        public override void Rebuild(Vector2 size, bool excludeChildren)
        {
            Size = size;

            if (excludeChildren)
                return;

            if (!HasEnabledInnerElement)
                return;

            var childSize = InnerElement.CalculatePreferredSize();
            InnerElement.Rebuild(childSize);
        }

        public override Element GetChildAt(int index)
        {
            if (InnerElement is null)
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

        protected override void HandleTransformChanged()
        {
            _innerElement?.OnTransformChanged();
        }
    }
}
