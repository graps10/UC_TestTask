using System.Collections.Generic;
using Game.Core;
using Game.Level;
using Game.Shared;
using UnityEngine;

namespace Game.Obstacles
{
    public class ObstacleField : MonoBehaviour
    {
        [SerializeField] private float capsuleHeight = 1.4f;
        [SerializeField] private Color obstacleColor = new Color(0.2f, 0.55f, 0.2f);

        private readonly List<Obstacle> _obstacles = new List<Obstacle>();
        private readonly List<Vector3> _positions = new List<Vector3>();
        private readonly List<bool> _alive = new List<bool>();

        private BalanceSettings _balance;

        public void Initialize(LevelLayout layout, BalanceSettings balance)
        {
            _balance = balance;

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
            const float rippleSpeed = 0.03f; // seconds of delay per world unit from impact
            foreach (var index in destroyed)
            {
                _alive[index] = false;
                float delay = Vector3.Distance(_positions[index], impactPoint) * rippleSpeed;
                _obstacles[index].Kill(delay);
            }
        }

        private void SpawnObstacle(Vector3 position, float radius)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = $"Obstacle_{_obstacles.Count}";
            go.transform.SetParent(transform, false);
            go.transform.position = position;
            go.transform.localScale = new Vector3(radius * 2f, capsuleHeight * 0.5f, radius * 2f);
            go.GetComponent<MeshRenderer>().sharedMaterial = RuntimeMaterials.GetOrCreate(obstacleColor);

            var obstacle = go.AddComponent<Obstacle>();
            obstacle.Initialize(_obstacles.Count);

            _obstacles.Add(obstacle);
            _positions.Add(position);
            _alive.Add(true);
        }
    }
}
