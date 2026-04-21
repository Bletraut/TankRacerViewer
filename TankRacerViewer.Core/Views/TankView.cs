using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using Microsoft.Xna.Framework;

namespace TankRacerViewer.Core
{
    public sealed class TankView : AssetView
    {
        private const float TankScale = 1 / 8f;

        private const string TankDataFilePrefix = "TANK";
        private const string TankNodePostfix = "_node";

        private const string DistanceNodeName = "distance_node";
        private const string FBottomNodeName = "fbottom_node";
        private const string AerialNodeName = "aerial_node";
        private const string TurretNodeName = "turret_node";

        // Static.
        public static bool IsTankData(DataAssetView dataAssetView)
            => dataAssetView.FullName.StartsWith(TankDataFilePrefix);

        private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> _emptyProperties
            = new Dictionary<string, IReadOnlyList<string>>().AsReadOnly();

        // Class.
        public LevelObjectContainer TankNodeContainer { get; }

        public TankView(string fullName, DataAssetView tankDataAssetView,
            AssetViewContainer commonAssetViewContainer,
            AssetViewContainer tankAssetViewContainer)
            : base(fullName)
        {
            var tankNodes = new List<LevelObject>();
            foreach (var line in tankDataAssetView.Text.AsSpan().EnumerateLines())
            {
                if (line.IsEmpty)
                    continue;

                var values = line.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                var propertyName = values[0][..^1];
                if (!propertyName.EndsWith(TankNodePostfix))
                    continue;

                var isColliderNode = propertyName == DistanceNodeName
                    || propertyName == FBottomNodeName;
                if (isColliderNode)
                    continue;

                var modelName = Path.GetFileNameWithoutExtension(values[1].ToLowerInvariant());
                var position = new Vector3()
                {
                    X = float.Parse(values[2], CultureInfo.InvariantCulture),
                    Y = float.Parse(values[3], CultureInfo.InvariantCulture),
                    Z = -float.Parse(values[4], CultureInfo.InvariantCulture)
                } * TankScale;
                if (propertyName == AerialNodeName)
                {
                    var turretNode = tankNodes.Find(node => node.Id == TurretNodeName);
                    if (turretNode is not null)
                        position += turretNode.Position;
                }

                if (!tankAssetViewContainer.ModelAssetViews.TryGetValue(modelName, out var modelAssetView))
                {
                    commonAssetViewContainer.ModelAssetViews.TryGetValue(modelName, out modelAssetView);
                }
                if (modelAssetView is null)
                    continue;

                tankNodes.Add(new LevelObject(propertyName, propertyName,
                    modelAssetView, _emptyProperties)
                {
                    Position = position
                });
            }

            TankNodeContainer = new LevelObjectContainer(tankDataAssetView.FullName,
                tankNodes.AsReadOnly());
        }

        public void Draw(WorldRenderer renderer, Camera camera)
        {
            renderer.Begin(Color.CornflowerBlue);

            if (TankNodeContainer is not null)
                renderer.Draw(TankNodeContainer, camera);

            renderer.End();
        }
    }
}
