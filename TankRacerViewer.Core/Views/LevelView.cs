using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using FastFileUnpacker;

using Microsoft.Xna.Framework;

namespace TankRacerViewer.Core
{
    public sealed class LevelView : AssetView
    {
        private const string LevelFormatKey = "FSTPC";

        private const int GridSize = 128;

        private const string FlockModelName = "birdy2";

        // Static.
        private static readonly StringBuilder _dataBuffer = new();

        private static readonly char[] _valueSeparators = [' ', '(', ')'];

        private static readonly char[] _powerupIdOrder = ['a', 'b', 'c', 'd', 'e'];

        private static bool TryParseLevelObjectData(string data,
            AssetViewContainer commonAssetViewContainer,
            AssetViewContainer levelAssetViewContainer,
            out LevelObject levelObject)
        {
            levelObject = null;

            var values = data.Split(_valueSeparators, StringSplitOptions.RemoveEmptyEntries);
            if (values.Length <= 4)
                return false;

            var type = values[0];
            var id = values[1];

            var position = new Vector3()
            {
                X = float.Parse(values[2], CultureInfo.InvariantCulture),
                Y = float.Parse(values[3], CultureInfo.InvariantCulture),
                Z = -float.Parse(values[4], CultureInfo.InvariantCulture)
            } * GridSize;

            var rotation = new Vector3()
            {
                X = MathHelper.ToRadians(float.Parse(values[5], CultureInfo.InvariantCulture)),
                Y = MathHelper.ToRadians(float.Parse(values[6], CultureInfo.InvariantCulture)),
                Z = -MathHelper.ToRadians(float.Parse(values[7], CultureInfo.InvariantCulture))
            };
            if (type == LevelObject.PowerupTypeName)
                rotation = Vector3.Zero;

            var modelName = Path.GetFileNameWithoutExtension(values[8]).ToLowerInvariant();

            var properties = new Dictionary<string, IReadOnlyList<string>>();
            List<string> currentPropertyValues = null;

            foreach (var value in values)
            {
                if (value.StartsWith(':'))
                {
                    currentPropertyValues = [];
                    var currentPropertyName = value[1..];
                    properties[currentPropertyName] = currentPropertyValues.AsReadOnly();

                    continue;
                }

                currentPropertyValues?.Add(value);
            }

            if (modelName == "nil")
            {
                if (type == LevelObject.FlockTypeName)
                {
                    modelName = FlockModelName;
                }
                else if (properties.TryGetValue(LevelObject.MultiCrushPropertyName, out var propertyValues))
                {
                    modelName = Path.GetFileNameWithoutExtension(propertyValues[0]);
                }
            }

            if (!levelAssetViewContainer.ModelAssetViews.TryGetValue(modelName, out var modelAssetView))
            {
                if (!commonAssetViewContainer.ModelAssetViews.TryGetValue(modelName, out modelAssetView))
                    return false;
            }

            levelObject = new LevelObject(type, id, properties.AsReadOnly())
            {
                ModelAssetView = modelAssetView,
                Position = position,
                Rotation = rotation
            };

            return true;
        }

        // Class.
        public IReadOnlyList<LevelObjectContainer> LevelObjectContainers { get; }

        private LevelObjectContainer _currentLevelObjectContainer;
        public LevelObjectContainer CurrentLevelObjectContainer
        {
            get => _currentLevelObjectContainer;
            set
            {
                if (_currentLevelObjectContainer == value)
                    return;

                _currentLevelObjectContainer = value;
                RefreshLevelObjects();
            }
        }

        private int _currentLap = 1;
        public int CurrentLap
        {
            get => _currentLap;
            set
            {
                if (_currentLap == value)
                    return;

                _currentLap = value;
                RefreshLevelObjects();
            }
        }

        public IReadOnlyDictionary<string, BackgroundAssetView> BackgroundAssetViews { get; }
        public BackgroundAssetView BackgroundAssetView { get; set; }

        private ModelAssetView MapModelAssetView { get; }

        public LevelView(string fullName, AssetViewContainer commonAssetViewContainer,
            AssetViewContainer levelAssetViewContainer) : base(fullName)
        {
            BackgroundAssetViews = levelAssetViewContainer.BackgroundAssetViews;
            BackgroundAssetView = BackgroundAssetViews.Values.FirstOrDefault();

            foreach (var modelAssetView in levelAssetViewContainer.ModelAssetViews.Values)
            {
                if (modelAssetView.Extension.Equals(".bsm", StringComparison.InvariantCultureIgnoreCase))
                {
                    MapModelAssetView = modelAssetView;
                    break;
                }
            }

            var levelObjectContainers = new List<LevelObjectContainer>();
            LevelObjectContainers = levelObjectContainers.AsReadOnly();

            foreach (var asset in levelAssetViewContainer.FastFile.Assets)
            {
                if (asset is not DataAsset dataAsset)
                    continue;

                if (dataAsset.Text.AsSpan(0, 5).ToString() != LevelFormatKey)
                    continue;

                _dataBuffer.Clear();
                var levelObjects = new List<LevelObject>();

                foreach (var line in dataAsset.Text.AsSpan().EnumerateLines())
                {
                    if (!line.IsEmpty)
                    {
                        if (_dataBuffer.Length > 0)
                            _dataBuffer.Append(' ');
                        _dataBuffer.Append(line);
                        continue;
                    }

                    if (TryParseLevelObjectData(_dataBuffer.ToString(),
                        commonAssetViewContainer, levelAssetViewContainer,
                        out var levelObject))
                    {
                        levelObjects.Add(levelObject);
                    }

                    _dataBuffer.Clear();
                }

                levelObjectContainers.Add(new LevelObjectContainer(asset.FullName,
                    levelObjects.AsReadOnly()));
            }

            if (LevelObjectContainers.Count > 0)
                CurrentLevelObjectContainer = LevelObjectContainers[0];
        }

        public void Draw(WorldRenderer renderer, Camera camera)
        {
            renderer.Begin(Color.Black);

            if (BackgroundAssetView is not null)
                renderer.Draw(BackgroundAssetView, camera);

            if (MapModelAssetView is not null)
                renderer.Draw(MapModelAssetView, Matrix.Identity, camera);

            if (CurrentLevelObjectContainer is not null)
                renderer.Draw(CurrentLevelObjectContainer, camera);

            renderer.End();
        }

        private void RefreshLevelObjects()
        {
            if (CurrentLevelObjectContainer is null)
                return;

            foreach (var levelObject in CurrentLevelObjectContainer.LevelObjects)
            {
                var isEditorType = levelObject.Type == LevelObject.WayPointTypeName
                    || levelObject.Type == LevelObject.TrackcamTypeName;

                var shouldShowOnCurrentLap = true;
                if (levelObject.Properties.TryGetValue(LevelObject.LapToAddPropertyName, out var values))
                    shouldShowOnCurrentLap &= CurrentLap >= int.Parse(values[0]);
                if (levelObject.Properties.TryGetValue(LevelObject.LapToRemovePropertyName, out values))
                    shouldShowOnCurrentLap &= CurrentLap < int.Parse(values[0]);

                if (levelObject.Type == LevelObject.PowerupTypeName)
                {
                    var id = Array.IndexOf(_powerupIdOrder, levelObject.Id[0]) + 1;

                    var shouldShow = id == CurrentLap;
                    if (levelObject.Properties.ContainsKey(LevelObject.IncrementLapForNextPropertyName))
                        shouldShow |= id == CurrentLap - 1;

                    shouldShowOnCurrentLap &= shouldShow;
                }

                levelObject.IsEnabled = !isEditorType && shouldShowOnCurrentLap;
            }
        }
    }
}
