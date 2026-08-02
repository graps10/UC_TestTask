using System.Collections.Generic;
using Game.Core;
using Game.Level;
using Game.Obstacles;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class ObstacleFieldTests
    {
        private GameObject _root;
        private GameObject _obstaclePrefabGo;

        [TearDown]
        public void TearDown()
        {
            if (_root != null)
                Object.DestroyImmediate(_root);
            if (_obstaclePrefabGo != null)
                Object.DestroyImmediate(_obstaclePrefabGo);
        }

        private ObstacleField CreateField()
        {
            _root = new GameObject("ObstacleFieldTestRoot");
            return _root.AddComponent<ObstacleField>();
        }
        
        private Obstacle CreateObstaclePrefab()
        {
            _obstaclePrefabGo = new GameObject("ObstaclePrefabTemplate");
            _obstaclePrefabGo.AddComponent<CapsuleCollider>();
            var obstacle = _obstaclePrefabGo.AddComponent<Obstacle>();

            var visual = new GameObject("Visual").transform;
            visual.SetParent(_obstaclePrefabGo.transform);
            obstacle.SetVisualForTest(visual);

            return obstacle;
        }

        // Index 0 = x:-1, index 1 = x:-0.7 (close to 0), index 2 = x:2 (far/isolated).
        private static LevelLayout BuildSimpleLayout()
        {
            var layout = new LevelLayout { CorridorWidth = 6f, ObstacleRadius = 0.5f, StartZ = 0f, DoorZ = 10f };
            var row = new LevelRow(4f);
            row.ObstacleX.Add(-1f);
            row.ObstacleX.Add(-0.7f);
            row.ObstacleX.Add(2f);
            layout.Rows.Add(row);
            return layout;
        }

        private static BalanceSettings BuildBalance()
        {
            return new BalanceSettings
            {
                BaseBlastRadius = 0.1f,
                RadiusPerSize = 1.0f,
                ChainRadius = 0.5f,
                CriticalMinSize = 0.2f,
                GapClearanceFactor = 1.15f,
            };
        }

        private static List<Obstacle> ObstaclesByIndex(GameObject root)
        {
            var obstacles = new List<Obstacle>(root.GetComponentsInChildren<Obstacle>());
            obstacles.Sort((a, b) => a.Index.CompareTo(b.Index));
            return obstacles;
        }

        [Test]
        public void Initialize_SpawnsOneObstaclePerLayoutEntry()
        {
            var field = CreateField();
            field.Initialize(BuildSimpleLayout(), BuildBalance(), CreateObstaclePrefab());

            Assert.AreEqual(3, ObstaclesByIndex(_root).Count);
        }

        [Test]
        public void Explode_DestroysImpactAndCloseNeighbour_ButNotFarObstacle()
        {
            var field = CreateField();
            field.Initialize(BuildSimpleLayout(), BuildBalance(), CreateObstaclePrefab());
            var obstacles = ObstaclesByIndex(_root);

            field.Explode(impactIndex: 0, shotSize: 0f);

            Assert.IsFalse(obstacles[0].IsAlive);
            Assert.IsFalse(obstacles[1].IsAlive, "Close neighbour should be destroyed via chain propagation.");
            Assert.IsTrue(obstacles[2].IsAlive, "Far obstacle must survive an unrelated small blast.");
        }

        [Test]
        public void Explode_DisablesColliderImmediately()
        {
            var field = CreateField();
            field.Initialize(BuildSimpleLayout(), BuildBalance(), CreateObstaclePrefab());
            var obstacles = ObstaclesByIndex(_root);

            field.Explode(impactIndex: 0, shotSize: 0f);

            Assert.IsFalse(obstacles[0].GetComponent<Collider>().enabled);
        }

        [Test]
        public void Explode_OnAlreadyDeadImpact_IsNoOpAndDoesNotThrow()
        {
            var field = CreateField();
            field.Initialize(BuildSimpleLayout(), BuildBalance(), CreateObstaclePrefab());
            var obstacles = ObstaclesByIndex(_root);

            field.Explode(impactIndex: 0, shotSize: 0f);
            Assert.DoesNotThrow(() => field.Explode(impactIndex: 0, shotSize: 5f));

            Assert.IsTrue(obstacles[2].IsAlive, "A no-op explode on a dead impact must not affect unrelated obstacles.");
        }
    }
}
