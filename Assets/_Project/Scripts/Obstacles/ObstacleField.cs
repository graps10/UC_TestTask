using System.Collections.Generic;
using Game.Core;
using Game.Level;
using UnityEngine;

namespace Game.Obstacles
{
    public class ObstacleField : MonoBehaviour
    {
        [Tooltip("Seconds of Kill() delay per world unit of distance from the impact point, " +
                 "so a chain reaction visibly ripples outward instead of popping all at once.")]
        [SerializeField] private float rippleDelayPerUnit = 0.03f;

        private readonly List<Obstacle> _obstacles = new();
        private readonly List<Vector3> _positions = new();
        private readonly List<bool> _alive = new();

        private BalanceSettings _balance;
        private Obstacle _obstaclePrefab;

        public void Initialize(LevelLayout layout, BalanceSettings balance, Obstacle obstaclePrefab)
        {
            _balance = balance;
            _obstaclePrefab = obstaclePrefab;

            foreach (var row in layout.Rows)
            {
                foreach (var x in row.ObstacleX)
                {
                    SpawnObstacle(new Vector3(x, 0f, row.Z), layout.ObstacleRadius);
                }
            }
        }

        public void Explode(int impactIndex, float shotSize)
        {
            if (impactIndex < 0 || impactIndex >= _alive.Count || !_alive[impactIndex])
                return;

            float blastRadius = ChainReactionSolver.ComputeBlastRadius(shotSize, _balance.BaseBlastRadius, _balance.RadiusPerSize);
            var destroyed = ChainReactionSolver.Simulate(_positions, _alive, impactIndex, blastRadius, _balance.ChainRadius);

            Vector3 impactPoint = _positions[impactIndex];
            foreach (var index in destroyed)
            {
                _alive[index] = false;
                float delay = Vector3.Distance(_positions[index], impactPoint) * rippleDelayPerUnit;
                _obstacles[index].Kill(delay);
            }
        }

        private void SpawnObstacle(Vector3 position, float radius)
        {
            var obstacle = Instantiate(_obstaclePrefab, position, Quaternion.identity, transform);
            obstacle.name = $"Obstacle_{_obstacles.Count}";
            obstacle.ConfigureRadius(radius);
            obstacle.Initialize(_obstacles.Count);

            _obstacles.Add(obstacle);
            _positions.Add(position);
            _alive.Add(true);
        }
    }
}
