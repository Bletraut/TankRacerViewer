using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class Element
    {
        private ParentElement _parent;
        public ParentElement Parent
        {
            get => _parent;
            internal set
            {
                if (_parent == value)
                    return;

                _parent = value;
                ApplyRoot(_parent?.Root);

                OnTransformChanged();
            }
        }

        public RootElement Root { get; private set; }

        private uint _layer;
        public uint Layer
        {
            get => _layer;
            set => SetAndChangeState(ref _layer, value);
        }

        private bool _isEnabled = true;
        public virtual bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (SetAndChangeState(ref _isEnabled, value))
                    OnTransformChanged();
            }
        }

        private Vector2 _size;
        public Vector2 Size
        {
            get => _size;
            set
            {
                if (SetAndChangeState(ref _size, value))
                    OnTransformChanged();
            }
        }

        public Vector2 PivotOffset => Size * Pivot;

        private Vector2 _position;
        public Vector2 Position
        {
            get
            {
                RecalculateGlobalTransformationMatrixIfDirty();
                return _position;
            }
            set
            {
                if (Position == value)
                    return;

                LocalPosition = Parent is not null
                    ? Vector2.Transform(value, Parent.GlobalInverseTransformationMatrix)
                    : value;
            }
        }

        private Vector2 _localPosition;
        public Vector2 LocalPosition
        {
            get => _localPosition;
            set
            {
                if (_localPosition == value) 
                    return;

                _localPosition = value;
                OnTransformChanged();
            }
        }

        private Vector2 _pivot = Vector2.One / 2;
        public Vector2 Pivot
        {
            get => _pivot;
            set
            { 
                if (SetAndChangeState(ref _pivot, value))
                    OnTransformChanged();
            }
        }

        private Rectangle _boundingRectangle;
        public Rectangle BoundingRectangle
        {
            get
            {
                RecalculateBoundingRectangleIfDirty();
                return _boundingRectangle;
            }
        }

        private Rectangle? _clipMask;
        public Rectangle? ClipMask
        {
            get
            {
                if (_isClipMaskDirty)
                {
                    _isClipMaskDirty = false;
                    _clipMask = CalculateClipMask();
                }

                return _clipMask;
            }
        }

        private Matrix _localTransformationMatrix;
        public Matrix LocalTransformationMatrix
        {
            get
            {
                RecalculateLocalTransformationMatrixIfDirty();
                return _localTransformationMatrix;
            }
        }

        private Matrix _globalTransformationMatrix;
        public Matrix GlobalTransformationMatrix
        {
            get
            {
                RecalculateGlobalTransformationMatrixIfDirty();
                return _globalTransformationMatrix;
            }
        }

        private Matrix _globalInverseTransformationMatrix;
        public Matrix GlobalInverseTransformationMatrix
        {
            get
            {
                RecalculateGlobalInverseTransformationMatrixIfDirty();
                return _globalInverseTransformationMatrix;
            }
        }

        private bool _isClipMaskDirty = true;
        private bool _isBoundingRectangleDirty = true;

        private bool _isLocalTransformationMatrixDirty = true;
        private bool _isGlobalTransformationMatrixDirty = true;
        private bool _isGlobalInverseTransformationMatrixDirty = true;

        protected internal virtual Rectangle? CalculateClipMask()
            => Parent?.ClipMask;

        protected internal virtual void ApplyRoot(RootElement root)
        {
            Root = root;
        }

        public IEnumerable<ParentElement> GetParentsRecursively()
        {
            if (Parent is null)
                yield break;

            var parent = Parent;
            while (parent is not null)
            {
                yield return parent;

                parent = parent.Parent;
            }
        }

        protected bool SetAndChangeState<T>(ref T element, T value)
        {
            if (EqualityComparer<T>.Default.Equals(element, value))
                return false;

            element = value;
            OnStateChanged();

            return true;
        }

        private void RecalculateBoundingRectangleIfDirty()
        {
            if (!_isBoundingRectangleDirty)
                return;

            _isBoundingRectangleDirty = false;
            _boundingRectangle = new Rectangle((Position - PivotOffset).ToPoint(), Size.ToPoint());
        }

        private void RecalculateLocalTransformationMatrixIfDirty()
        {
            if (!_isLocalTransformationMatrixDirty) 
                return;

            _isLocalTransformationMatrixDirty = false;
            _localTransformationMatrix = Matrix.CreateTranslation(new Vector3(LocalPosition, 0));
        }

        private void RecalculateGlobalTransformationMatrixIfDirty()
        {
            if (!_isGlobalTransformationMatrixDirty)
                return;

            _isGlobalTransformationMatrixDirty = false;
            _globalTransformationMatrix = LocalTransformationMatrix;

            if (Parent is not null)
                _globalTransformationMatrix *= Parent.GlobalTransformationMatrix;

            _globalTransformationMatrix.Decompose(out _, out _, out var position);
            _position = new Vector2(position.X, position.Y);
        }

        private void RecalculateGlobalInverseTransformationMatrixIfDirty()
        {
            if (!_isGlobalInverseTransformationMatrixDirty)
                return;

            _isGlobalInverseTransformationMatrixDirty = false;
            _globalInverseTransformationMatrix = Matrix.Invert(GlobalTransformationMatrix);
        }

        public virtual Vector2 CalculatePreferredSize() => Size;

        public virtual void Rebuild(Vector2 size)
        {
            Size = size;
        }

        protected virtual void HandleTransformChanged() { }

        internal void OnTransformChanged()
        {
            if (_isLocalTransformationMatrixDirty)
                return;

            _isClipMaskDirty = true;
            _isBoundingRectangleDirty = true;

            _isLocalTransformationMatrixDirty = true;
            _isGlobalTransformationMatrixDirty = true;
            _isGlobalInverseTransformationMatrixDirty = true;

            HandleTransformChanged();

            OnStateChanged();
        }

        protected void OnStateChanged()
        {
            Root?.MarkAsDirty();
        }
    }

    public delegate void ElementEventHandler<TElement>(TElement sender);
    public delegate void ElementEventHandler<TElement, TEventArgs>(TElement sender, TEventArgs arguments);
}
