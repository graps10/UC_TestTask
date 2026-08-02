using System.Collections.Generic;
using Game.Obstacles;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class ChainReactionSolverTests
    {
        [Test]
        public void ComputeBlastRadius_GrowsLinearlyWithShotSize()
        {
            float r0 = ChainReactionSolver.ComputeBlastRadius(0f, baseRadius: 0.3f, radiusPerSize: 1.0f);
            float r1 = ChainReactionSolver.ComputeBlastRadius(2f, baseRadius: 0.3f, radiusPerSize: 1.0f);

            Assert.AreEqual(0.3f, r0, 0.0001f);
            Assert.AreEqual(2.3f, r1, 0.0001f);
        }

        [Test]
        public void DenseCluster_FullyChainsFromSmallBlast()
        {
            // Five obstacles spaced 0.3 apart in a line; chainRadius covers each gap.
            var positions = new List<Vector3>
            {
                new(0f, 0f, 0f),
                new(0.3f, 0f, 0f),
                new(0.6f, 0f, 0f),
                new(0.9f, 0f, 0f),
                new(1.2f, 0f, 0f),
            };
            var alive = new List<bool> { true, true, true, true, true };

            var destroyed = ChainReactionSolver.Simulate(positions, alive, impactIndex: 0, blastRadius: 0.1f, chainRadius: 0.5f);

            Assert.AreEqual(5, destroyed.Count, "A tightly-packed line should chain-destroy fully from a single small hit.");
        }

        [Test]
        public void IsolatedObstacle_NotDestroyedByFarChainOrSmallBlast()
        {
            var positions = new List<Vector3>
            {
                new(0f, 0f, 0f),   // impact
                new(0.3f, 0f, 0f), // in blast/chain range
                new(10f, 0f, 0f),  // far, isolated
            };
            var alive = new List<bool> { true, true, true };

            var destroyed = ChainReactionSolver.Simulate(positions, alive, impactIndex: 0, blastRadius: 0.1f, chainRadius: 0.5f);

            Assert.IsTrue(destroyed.Contains(0));
            Assert.IsTrue(destroyed.Contains(1));
            Assert.IsFalse(destroyed.Contains(2), "An obstacle far from the chain must survive.");
        }

        [Test]
        public void IsolatedObstacle_RequiresDirectBigBlast()
        {
            var positions = new List<Vector3>
            {
                new(0f, 0f, 0f),
                new(5f, 0f, 0f), // isolated, only reachable by a big enough direct blast
            };
            var alive = new List<bool> { true, true };

            var smallShot = ChainReactionSolver.Simulate(positions, alive, impactIndex: 0, blastRadius: 1f, chainRadius: 0.5f);
            Assert.IsFalse(smallShot.Contains(1));

            var bigShot = ChainReactionSolver.Simulate(positions, alive, impactIndex: 0, blastRadius: 6f, chainRadius: 0.5f);
            Assert.IsTrue(bigShot.Contains(1));
        }

        [Test]
        public void DeadObstaclesAreIgnored()
        {
            var positions = new List<Vector3> { Vector3.zero, new(0.2f, 0f, 0f) };
            var alive = new List<bool> { true, false };

            var destroyed = ChainReactionSolver.Simulate(positions, alive, impactIndex: 0, blastRadius: 1f, chainRadius: 1f);

            Assert.AreEqual(1, destroyed.Count);
            Assert.IsTrue(destroyed.Contains(0));
        }

        [Test]
        public void ImpactOnAlreadyDeadObstacle_DestroysNothing()
        {
            var positions = new List<Vector3> { Vector3.zero, new(0.2f, 0f, 0f) };
            var alive = new List<bool> { false, true };

            var destroyed = ChainReactionSolver.Simulate(positions, alive, impactIndex: 0, blastRadius: 5f, chainRadius: 5f);

            Assert.AreEqual(0, destroyed.Count);
        }
    }
}
