using System.Linq;
using Game.Level;
using NUnit.Framework;

namespace Game.Tests
{
    public class LevelGeneratorTests
    {
        private static LevelGenerationSettings DefaultSettings()
        {
            return new LevelGenerationSettings
            {
                Seed = 42,
                CorridorWidth = 6f,
                ObstacleRadius = 0.5f,
                StartClearZ = 0f,
                FirstRowZ = 4f,
                RowSpacing = 3f,
                RowCount = 10,
                DoorApproachClearance = 3f,
                MinObstaclesPerRow = 3,
                MaxObstaclesPerRow = 7,
                DenseRowChance = 0.5f,
            };
        }

        [Test]
        public void SameSeed_ProducesIdenticalLayout()
        {
            var a = LevelGenerator.Generate(DefaultSettings());
            var b = LevelGenerator.Generate(DefaultSettings());

            Assert.AreEqual(a.Rows.Count, b.Rows.Count);
            for (int i = 0; i < a.Rows.Count; i++)
            {
                Assert.AreEqual(a.Rows[i].Z, b.Rows[i].Z, 0.0001f);
                CollectionAssert.AreEqual(a.Rows[i].ObstacleX, b.Rows[i].ObstacleX);
            }
        }

        [Test]
        public void DifferentSeed_ProducesDifferentLayout()
        {
            var settingsA = DefaultSettings();
            var settingsB = DefaultSettings();
            settingsB.Seed = 43;

            var a = LevelGenerator.Generate(settingsA);
            var b = LevelGenerator.Generate(settingsB);

            bool anyRowDiffers = false;
            for (int i = 0; i < a.Rows.Count; i++)
            {
                if (!a.Rows[i].ObstacleX.SequenceEqual(b.Rows[i].ObstacleX))
                {
                    anyRowDiffers = true;
                    break;
                }
            }
            Assert.IsTrue(anyRowDiffers, "Different seeds are expected to produce a different layout.");
        }

        [Test]
        public void GeneratesRequestedRowCount()
        {
            var layout = LevelGenerator.Generate(DefaultSettings());
            Assert.AreEqual(10, layout.Rows.Count);
        }

        [Test]
        public void DoorIsPlacedAfterLastRow()
        {
            var layout = LevelGenerator.Generate(DefaultSettings());
            var lastRowZ = layout.Rows[layout.Rows.Count - 1].Z;
            Assert.Greater(layout.DoorZ, lastRowZ);
        }

        [Test]
        public void AllObstaclesStayWithinCorridorBounds()
        {
            var settings = DefaultSettings();
            var layout = LevelGenerator.Generate(settings);
            float halfWidth = settings.CorridorWidth * 0.5f;

            foreach (var row in layout.Rows)
            {
                foreach (var x in row.ObstacleX)
                {
                    Assert.LessOrEqual(x, halfWidth);
                    Assert.GreaterOrEqual(x, -halfWidth);
                }
            }
        }
        
        [Test]
        public void NoRowHasAGapWiderThanMaxGapFactor()
        {
            for (int seed = 0; seed < 20; seed++)
            {
                var settings = DefaultSettings();
                settings.Seed = seed;
                var layout = LevelGenerator.Generate(settings);

                float halfWidth = settings.CorridorWidth * 0.5f;
                float edgeMargin = settings.ObstacleRadius * settings.EdgeMarginFactor;
                float placeMin = -halfWidth + edgeMargin;
                float placeMax = halfWidth - edgeMargin;
                float maxAllowedGap = settings.ObstacleRadius * settings.MaxGapFactor;

                foreach (var row in layout.Rows)
                {
                    var sortedX = row.ObstacleX.OrderBy(x => x).ToList();
                    float prev = placeMin;
                    foreach (var x in sortedX)
                    {
                        Assert.LessOrEqual(x - prev, maxAllowedGap + 0.0001f,
                            $"Seed {seed}, row z={row.Z}: gap before x={x} is walkable.");
                        prev = x;
                    }
                    Assert.LessOrEqual(placeMax - prev, maxAllowedGap + 0.0001f,
                        $"Seed {seed}, row z={row.Z}: trailing gap to the corridor wall is walkable.");
                }
            }
        }
    }
}
