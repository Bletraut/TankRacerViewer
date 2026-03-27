using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TankRacerViewer.Core
{
    public class CameraController
    {
        public Camera Camera { get; init; }

        public bool IsLookEnabled { get; set; } = true;

        public Vector3 Position
        {
            get => Camera.Position;
            set => Camera.Position = value;
        }

        private Vector3 _eulerAngles;
        public Vector3 EulerAngles
        {
            get => _eulerAngles;
            set
            {
                _eulerAngles = value;
                Camera.Rotation = Quaternion.CreateFromYawPitchRoll(_eulerAngles.Y, _eulerAngles.X, _eulerAngles.Z);
            }
        }

        public float MoveSpeed { get; set; } = 0.35f;
        public float SlowMoveSpeed { get; set; } = 0.1f;
        public float FastMoveSpeed { get; set; } = 2f;
        public float RotationSpeed { get; set; } = MathF.PI / 360f;

        public CameraController(Camera camera)
        {
            Camera = camera;
        }

        public void LookAt(Vector3 target)
        {
            var direction = Vector3.Normalize(Camera.Position - target);

            EulerAngles = new Vector3()
            {
                X = -MathF.Atan2(direction.Y, new Vector2(direction.X, direction.Z).Length()),
                Y = MathF.Atan2(direction.X, direction.Z),
            };
        }

        public void Update(GameTime gameTime)
        {
            if (IsLookEnabled && Input.IsMouseButtonPressed(MouseButton.Left))
            {
                _eulerAngles += new Vector3(-Input.MousePositionDelta.Y, -Input.MousePositionDelta.X, 0) * RotationSpeed;
                _eulerAngles.X = MathHelper.Clamp(_eulerAngles.X, -MathHelper.PiOver2, MathHelper.PiOver2);
                EulerAngles = _eulerAngles;
            }

            var moveSpeed = MoveSpeed;
            if (Input.IsKeyPressed(Keys.LeftControl))
            {
                moveSpeed = SlowMoveSpeed;
            }
            else if (Input.IsKeyPressed(Keys.C))
            {
                moveSpeed = FastMoveSpeed;
            }

            var isShiftPressed = Input.IsKeyPressed(Keys.LeftShift);
            if (Input.IsKeyPressed(Keys.W))
            {
                Camera.Position += (isShiftPressed ? Camera.Up : Camera.Forward) * moveSpeed;
            }
            else if (Input.IsKeyPressed(Keys.S))
            {
                Camera.Position -= (isShiftPressed ? Camera.Up : Camera.Forward) * moveSpeed;
            }
            if (Input.IsKeyPressed(Keys.A))
            {
                Camera.Position -= Camera.Right * moveSpeed;
            }
            else if (Input.IsKeyPressed(Keys.D))
            {
                Camera.Position += Camera.Right * moveSpeed;
            }
        }
    }
}
