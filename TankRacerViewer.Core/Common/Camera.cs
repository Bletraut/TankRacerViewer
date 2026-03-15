using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TankRacerViewer.Core
{
    public class Camera
    {
        private Vector3 _position;
        public Vector3 Position
        {
            get => _position;
            set
            {
                if (_position == value)
                    return;

                _position = value;

                _viewMatrix = null;
                _viewProjectionMatrix = null;
            }
        }

        private Quaternion _rotation = Quaternion.Identity;
        public Quaternion Rotation
        {
            get => _rotation;
            set
            {
                if (_rotation == value)
                    return;

                _rotation = value;

                _viewMatrix = null;
                _viewProjectionMatrix = null;
            }
        }

        public Vector3 Forward => Vector3.Transform(Vector3.Forward, Rotation);
        public Vector3 Right => Vector3.Transform(Vector3.Right, Rotation);
        public Vector3 Up => Vector3.Cross(Right, Forward);

        public float Yaw
        {
            get
            {
                var right = Right;
                return MathF.Atan2(-right.Z, right.X);
            }
        }

        public float Pitch
        {
            get
            {
                var forward = Forward;
                return MathF.Atan2(forward.Y, new Vector2(forward.X, forward.Z).LengthSquared());
            }
        }

        private float _fieldOfView = MathHelper.ToRadians(45);
        public float FieldOfView
        {
            get => _fieldOfView;
            set
            {
                if (_fieldOfView == value)
                    return;

                _fieldOfView = value;

                _projectionMatrix = null;
                _viewProjectionMatrix = null;
            }
        }

        private float _nearPlane = 0.1f;
        public float NearPlane
        {
            get => _nearPlane;
            set
            {
                if (_nearPlane == value)
                    return;

                _nearPlane = value;

                _projectionMatrix = null;
                _viewProjectionMatrix = null;
            }
        }

        private float _farPlane = 1_000;
        public float FarPlane
        {
            get => _farPlane;
            set
            {
                if (_farPlane == value)
                    return;

                _farPlane = value;

                _projectionMatrix = null;
                _viewProjectionMatrix = null;
            }
        }

        private Matrix? _viewMatrix;
        public Matrix ViewMatrix
        {
            get
            {
                if (!_viewMatrix.HasValue)
                    _viewMatrix = Matrix.CreateLookAt(Position, Position + Forward, Up);

                return _viewMatrix.Value;
            }
        }

        private Matrix? _projectionMatrix;
        public Matrix ProjectionMatrix
        {
            get
            {
                if (_currentRenderContext is null)
                    return Matrix.Identity;

                if (!_projectionMatrix.HasValue)
                {
                    var aspectRatio = _currentRenderContext.Resolution.X / (float)_currentRenderContext.Resolution.Y;
                    _projectionMatrix = Matrix.CreatePerspectiveFieldOfView(FieldOfView, aspectRatio, NearPlane, FarPlane);
                }

                return _projectionMatrix.Value;
            }
        }

        private Matrix? _viewProjectionMatrix;
        public Matrix ViewProjectionMatrix
        {
            get
            {
                if (!_viewProjectionMatrix.HasValue)
                    _viewProjectionMatrix = Matrix.Multiply(ViewMatrix, ProjectionMatrix);

                return _viewProjectionMatrix.Value;
            }
        }

        private readonly GraphicsDevice _graphicsDevice;

        private IRenderContext _currentRenderContext;

        public Camera(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        public void ApplyRenderContext(IRenderContext context)
        {
            if (_currentRenderContext == context)
                return;

            if (_currentRenderContext is not null)
                _currentRenderContext.ResolutionChanged -= OnRenderContextResolutionChanged;

            _currentRenderContext = context;
            _currentRenderContext.ResolutionChanged += OnRenderContextResolutionChanged;

            _projectionMatrix = null;
        }

        public Vector3 GetRayDirection(Vector2 screenPosition)
        {
            var nearPoint = _graphicsDevice.Viewport.Unproject(new Vector3(screenPosition, 0),
                ProjectionMatrix, ViewMatrix, Matrix.Identity);
            var farPoint = _graphicsDevice.Viewport.Unproject(new Vector3(screenPosition, 1),
                ProjectionMatrix, ViewMatrix, Matrix.Identity);

            var direction = Vector3.Normalize(farPoint - nearPoint);

            return direction;
        }

        private void OnRenderContextResolutionChanged(object sender, Point size)
        {
            _projectionMatrix = null;
            _viewProjectionMatrix = null;
        }
    }
}
