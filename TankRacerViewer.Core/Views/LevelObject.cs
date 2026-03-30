using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace TankRacerViewer.Core
{
    public sealed class LevelObject(string type, string id,
        ModelAssetView modelAssetView,
        IReadOnlyDictionary<string, IReadOnlyList<string>> properties)
    {
        public const string WayPointTypeName = "WayPoint";
        public const string TrackCameraTypeName = "Trackcam";
        public const string PowerupTypeName = "Powerup";
        public const string FlockTypeName = "Flock";

        public const string MultiCrushPropertyName = "multi_crush";
        public const string LapToAddPropertyName = "lap_to_add";
        public const string LapToRemovePropertyName = "lap_to_remove";
        public const string IncrementLapForNextPropertyName = "incr_lap_for_next";

        public string Type { get; } = type;
        public string Id { get; } = id;

        public ModelAssetView ModelAssetView { get; } = modelAssetView;

        public bool IsEnabled { get; set; } = true;

        public bool IsBoundingBoxEnabled { get; set; }
        public Color BoundingBoxColor { get; set; }

        public bool IsWayPoint => Type == WayPointTypeName;
        public bool IsTrackCamera => Type == TrackCameraTypeName;
        public bool IsEditorType => IsWayPoint || IsTrackCamera;

        private Vector3 _position;
        public Vector3 Position
        {
            get => _position;
            set
            {
                if (_position == value)
                    return;

                _position = value;
                MarkModelMatrixDirty();
            }
        }

        private Vector3 _rotation;
        public Vector3 Rotation
        {
            get => _rotation;
            set
            {
                if (_rotation == value)
                    return;

                _rotation = value;
                MarkModelMatrixDirty();
            }
        }

        private Vector3 _scale = Vector3.One;
        public Vector3 Scale
        {
            get => _scale;
            set
            {
                if (_scale == value)
                    return;

                _scale = value;
                MarkModelMatrixDirty();
            }
        }

        private Matrix _modelMatrix;
        public Matrix ModelMatrix
        {
            get
            {
                RecalculateModelMatrixIfNeeded();
                return _modelMatrix;
            }
        }

        public IReadOnlyDictionary<string, IReadOnlyList<string>> Properties { get; } = properties;

        private bool _isModelMatrixDirty = true;

        private void RecalculateModelMatrixIfNeeded()
        {
            if (!_isModelMatrixDirty)
                return;

            _isModelMatrixDirty = false;
            _modelMatrix = Matrix.CreateScale(Scale)
                * Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z)
                * Matrix.CreateTranslation(Position);
        }

        private void MarkModelMatrixDirty() => _isModelMatrixDirty = true;
    }
}
