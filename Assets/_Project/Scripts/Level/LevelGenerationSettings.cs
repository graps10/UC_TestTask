using System;
using UnityEngine;

namespace Game.Level
{
    [Serializable]
    public class LevelGenerationSettings
    {
        public int Seed = 12345;

        public float CorridorWidth = 6f;
        public float ObstacleRadius = 0.5f;

        public float StartClearZ = 0f;
        public float FirstRowZ = 4f;
        public float RowSpacing = 3f;
        public int RowCount = 12;
        public float DoorApproachClearance = 3f;

        public int MinObstaclesPerRow = 3;
        public int MaxObstaclesPerRow = 7;

        // Chance a row is generated as a tightly-packed cluster
        [Range(0f, 1f)]
        public float DenseRowChance = 0.5f;
    }
}
