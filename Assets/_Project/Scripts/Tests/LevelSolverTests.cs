using Game.Gameplay;
using NUnit.Framework;

namespace Game.Tests
{
    public class LevelSolverTests
    {
        private static LevelGenerationSettings DefaultLevelSettings()
        {
            return new LevelGenerationSettings
            {
                Seed = 7,
                CorridorWidth = 6f,
                ObstacleRadius = 0.5f,
                StartClearZ = 0f,
                FirstRowZ = 4f,
                RowSpacing = 3f,
                RowCount = 8,
                DoorApproachClearance = 3f,
                MinObstaclesPerRow = 3,
                MaxObstaclesPerRow = 7,
                DenseRowChance = 0.5f,
            };
        }

        private static BalanceSettings DefaultBalance()
        {
            return new BalanceSettings
            {
                BaseBlastRadius = 0.3f,
                RadiusPerSize = 1.0f,
                ChainRadius = 1.5f,
                CriticalMinSize = 0.4f,
                GapClearanceFactor = 1.15f,
            };
        }

        [Test]
        public void GeneratedLevel_IsSolvable()
        {
            var layout = LevelGenerator.Generate(DefaultLevelSettings());
            var balance = DefaultBalance();

            float minSize = LevelSolver.ComputeMinimumRequiredSize(layout, balance, out bool solvable);

            Assert.IsTrue(solvable, "Default generation settings should always produce a clearable level.");
            Assert.IsTrue(float.IsFinite(minSize));
            Assert.Greater(minSize, balance.CriticalMinSize);
        }

        [Test]
        public void ComputedMinimumSize_ActuallyClearsTheLevel()
        {
            var layout = LevelGenerator.Generate(DefaultLevelSettings());
            var balance = DefaultBalance();

            float minSize = LevelSolver.ComputeMinimumRequiredSize(layout, balance, out bool solvable);
            Assert.IsTrue(solvable);

            Assert.IsTrue(LevelSolver.CanClearLevel(layout, balance, minSize),
                "The solver's own answer must be self-consistent with CanClearLevel.");
        }

        [Test]
        public void SizeAtOrBelowCriticalMinimum_NeverClearsANonEmptyLevel()
        {
            var layout = LevelGenerator.Generate(DefaultLevelSettings());
            var balance = DefaultBalance();

            Assert.IsFalse(LevelSolver.CanClearLevel(layout, balance, balance.CriticalMinSize));
        }

        [Test]
        public void TwentyPercentBuffer_StillClearsTheLevel()
        {
            var layout = LevelGenerator.Generate(DefaultLevelSettings());
            var balance = DefaultBalance();

            float minSize = LevelSolver.ComputeMinimumRequiredSize(layout, balance, out bool solvable);
            Assert.IsTrue(solvable);

            float bufferedSize = minSize * 1.2f;
            Assert.IsTrue(LevelSolver.CanClearLevel(layout, balance, bufferedSize),
                "The +20% buffer used at runtime must remain safely above the computed minimum.");
        }

        [Test]
        public void ComputeMinimumRequiredSize_IsDeterministic()
        {
            var layout = LevelGenerator.Generate(DefaultLevelSettings());
            var balance = DefaultBalance();

            float first = LevelSolver.ComputeMinimumRequiredSize(layout, balance, out _);
            float second = LevelSolver.ComputeMinimumRequiredSize(layout, balance, out _);

            Assert.AreEqual(first, second, 0.0001f);
        }

        [Test]
        public void EmptyLevel_IsTriviallySolvable()
        {
            var layout = new LevelLayout { CorridorWidth = 6f, ObstacleRadius = 0.5f, StartZ = 0f, DoorZ = 10f };
            var balance = DefaultBalance();

            float minSize = LevelSolver.ComputeMinimumRequiredSize(layout, balance, out bool solvable);

            // With no obstacles at all, CanClearLevel(CriticalMinSize) is trivially true
            // (the loop never runs), so the search can converge arbitrarily close to it —
            // unlike the non-empty case, where clearing a real row is provably impossible
            // right at CriticalMinSize.
            Assert.IsTrue(solvable);
            Assert.GreaterOrEqual(minSize, balance.CriticalMinSize);
        }
    }
}
