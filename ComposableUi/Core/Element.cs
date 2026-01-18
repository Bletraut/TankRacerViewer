using Microsoft.Xna.Framework;

using System.Collections.Generic;

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

                if (_parent != null)
                {
                    _parent.SizeChanged -= OnParentSizeChanged;
                    _parent.TransformChanged -= OnParentTransformChanged;
                }

                _parent = value;
                if (_parent != null)
                {
                    _parent.SizeChanged += OnParentSizeChanged;
                    _parent.TransformChanged += OnParentTransformChanged;

                    OnParentSizeChanged(_parent);
                }

                OnParentTransformChanged(_parent);
            }
        }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetAndChangeState(ref _isEnabled, value);
        }

        private Vector2 _size;
        public Vector2 Size
        {
            get => _size;
            set
            {
                if (_size == value) 
                    return;

                _size = value;

                SizeChanged?.Invoke(this);
                OnStateChanged();
            }
        }

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
                if (_position == value)
                    return;

                LocalPosition = Parent != null
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

        public Vector2 _pivot = Vector2.One / 2;
        public Vector2 Pivot
        {
            get => _pivot;
            set => SetAndChangeState(ref _pivot, value);
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

        public event ElementEventHandler SizeChanged;
        public event ElementEventHandler TransformChanged;
        public event ElementEventHandler StateChanged;

        private bool _isLocalTransformationMatrixDirty = true;
        private bool _isGlobalTransformationMatrixDirty = true;
        private bool _isGlobalInverseTransformationMatrixDirty = true;

        public IEnumerable<ParentElement> GetParentsRecursively()
        {
            if (Parent == null)
                yield break;

            var parent = Parent;
            while (parent != null)
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

            if (Parent != null)
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

        public virtual void ApplySize(Vector2 size)
        {
            Size = size;
        }

        protected void OnStateChanged()
        {
            StateChanged?.Invoke(this);
            Parent?.OnStateChanged();
        }

        private void OnTransformChanged()
        {
            _isLocalTransformationMatrixDirty = true;

            OnParentTransformChanged(Parent);
        }

        private void OnParentTransformChanged(Element sender)
        {
            _isGlobalTransformationMatrixDirty = true;
            _isGlobalInverseTransformationMatrixDirty = true;

            TransformChanged?.Invoke(this);
            OnStateChanged();
        }

        protected virtual void OnParentSizeChanged(Element sender) { }
    }

    public delegate void ElementEventHandler(Element sender);
    public delegate void ElementEventHandler<TEventArgs>(Element sender, TEventArgs e);
}
