using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class ContainerElement : ParentElement
    {
        public override int ChildCount => _children.Count;

        private readonly List<Element> _children = [];

        public ContainerElement(IReadOnlyList<Element> children = default,
            Vector2? size = default,
            Vector2? pivot = default)
        {
            Size = size ?? Vector2.Zero;
            Pivot = pivot ?? Alignment.Center;

            _children = new List<Element>(children?.Count ?? 0);

            if (children is null)
                return;

            foreach (var child in children)
                AddChild(child);
        }

        public int IndexOf(Element child)
        {
            if (child.Parent != this)
                return -1;

            return _children.IndexOf(child);
        }

        public void InsertChild(int index, Element child)
        {
            if (child.Parent == this)
                return;

            if (child == this)
                throw new ArgumentException("An object cannot be a parent of itself.");

            foreach (var parent in GetParentsRecursively())
            {
                if (parent != child)
                    continue;

                parent.Parent?.AddChild(this);
                break;
            }

            child.Parent?.RemoveChild(child);
            child.Parent = this;

            if (index < _children.Count)
            {
                _children.Insert(Math.Max(0, index), child);
            }
            else
            {
                _children.Add(child);
            }
        }

        public void Clear()
        {
            foreach (var child in _children)
                child.Parent = null;

            _children.Clear();
        }

        public void BringToFront(Element child)
        {
            RemoveChild(child);
            AddChild(child);
        }

        public override Element GetChildAt(int index)
            => _children[index];

        public override void Rebuild(Vector2 size, bool excludeChildren)
        {
            Size = size;

            if (excludeChildren)
                return;

            foreach (var child in _children)
            {
                if (!child.IsEnabled)
                    continue;

                var childSize = child.CalculatePreferredSize();
                child.Rebuild(childSize);
            }
        }

        public override void AddChild(Element child)
        {
            InsertChild(_children.Count, child);
        }

        public override void RemoveChild(Element child)
        {
            if (_children.Remove(child))
            {
                child.Parent = null;
                OnStateChanged();
            }
        }
    }
}
