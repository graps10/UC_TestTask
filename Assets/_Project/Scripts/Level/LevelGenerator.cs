using UnityEngine;

namespace Game.Level
{
    public static class LevelGenerator
    {
        private const int MaxPlacementAttemptsPerObstacle = 30;

        public static LevelLayout Generate(LevelGenerationSettings settings)
        {
            var rng = new System.Random(settings.Seed);
            var layout = new LevelLayout
            {
                CorridorWidth = settings.CorridorWidth,
                ObstacleRadius = settings.ObstacleRadius,
                StartZ = settings.StartClearZ,
            };

            float halfWidth = settings.CorridorWidth * 0.5f;
            float edgeMargin = settings.ObstacleRadius * settings.EdgeMarginFactor;
            float placeMin = -halfWidth + edgeMargin;
            float placeMax = halfWidth - edgeMargin;

            float lastRowZ = settings.FirstRowZ;
            for (int i = 0; i < settings.RowCount; i++)
            {
                float z = settings.FirstRowZ + i * settings.RowSpacing;
                lastRowZ = z;

                bool dense = rng.NextDouble() < settings.DenseRowChance;
                var row = BuildRow(rng, z, dense, settings, placeMin, placeMax);
                layout.Rows.Add(row);
            }

            layout.DoorZ = lastRowZ + settings.DoorApproachClearance;
            return layout;
        }

        private static LevelRow BuildRow(
            System.Random rng,
            float z,
            bool dense,
            LevelGenerationSettings settings,
            float placeMin,
            float placeMax)
        {
            int minCount = dense
                ? Mathf.Max(settings.MinObstaclesPerRow, settings.MaxObstaclesPerRow - settings.DenseCountReduction)
                : settings.MinObstaclesPerRow;
            int maxCount = dense
                ? settings.MaxObstaclesPerRow
                : Mathf.Min(settings.MaxObstaclesPerRow, settings.MinObstaclesPerRow + settings.SparseCountBonus);
            int count = RandomRange(rng, minCount, maxCount);

            // Dense rows allow obstacles to sit almost touching (chain reaction does the
            // rest); sparse rows force real spacing so each obstacle needs its own shot.
            float minSpacing = dense
                ? settings.ObstacleRadius * settings.DenseMinSpacingFactor
                : settings.ObstacleRadius * settings.SparseMinSpacingFactor;

            var row = new LevelRow(z);
            for (int o = 0; o < count; o++)
            {
                for (int attempt = 0; attempt < MaxPlacementAttemptsPerObstacle; attempt++)
                {
                    float x = Mathf.Lerp(placeMin, placeMax, (float)rng.NextDouble());
                    if (IsFarEnoughFromExisting(row, x, minSpacing))
                    {
                        row.ObstacleX.Add(x);
                        break;
                    }
                }
            }

            return row;
        }

        private static bool IsFarEnoughFromExisting(LevelRow row, float x, float minSpacing)
        {
            foreach (var existingX in row.ObstacleX)
            {
                if (Mathf.Abs(existingX - x) < minSpacing)
                    return false;
            }
            return true;
        }

        private static int RandomRange(System.Random rng, int minInclusive, int maxInclusive)
        {
            return rng.Next(minInclusive, maxInclusive + 1);
        }
    }
}
