using System;
using UnityEngine;

namespace Game.Level
{
    [Serializable]
    public class LevelGenerationSettings
    {
        [Tooltip("Seed for the level's RNG — same seed always produces the same layout.")]
        public int Seed = 12345;

        [Tooltip("Width of the playable corridor, in units.")]
        public float CorridorWidth = 8f;

        [Tooltip("Obstacle radius, in units — drives spacing and physics. Overwrites the prefab's own collider radius at spawn.")]
        public float ObstacleRadius = 0.5f;

        [Tooltip("Player's spawn Z position.")]
        public float StartClearZ = 0f;

        [Tooltip("Z position of the first obstacle row (gives the player room before obstacles start).")]
        public float FirstRowZ = 4f;

        [Tooltip("Distance between consecutive obstacle rows, in units.")]
        public float RowSpacing = 2.2f;

        [Tooltip("Random +/- Z offset per row on top of RowSpacing, so rows don't read as perfectly even walls.")]
        public float RowZJitter = 0.6f;

        [Tooltip("Number of obstacle rows to generate.")]
        public int RowCount = 8;

        [Tooltip("Empty gap between the last obstacle row and the door.")]
        public float DoorApproachClearance = 3f;

        [Tooltip("Minimum obstacles a sparse row can have.")]
        public int MinObstaclesPerRow = 3;

        [Tooltip("Maximum obstacles a dense row can have.")]
        public int MaxObstaclesPerRow = 8;

        [Tooltip("Chance a row is a tightly-packed cluster (chain-reaction showcase) vs. sparse/spread-out (single-shot showcase).")]
        [Range(0f, 1f)]
        public float DenseRowChance = 0.5f;

        [Tooltip("Min spacing in a dense row, as a multiple of ObstacleRadius. Keep >= 2 (touching) to avoid mesh overlap.")]
        public float DenseMinSpacingFactor = 2.1f;

        [Tooltip("Min spacing in a sparse row, as a multiple of ObstacleRadius.")]
        public float SparseMinSpacingFactor = 4f;

        [Tooltip("Dense rows get up to this many fewer obstacles than MaxObstaclesPerRow at minimum.")]
        public int DenseCountReduction = 2;

        [Tooltip("Sparse rows get up to this many more obstacles than MinObstaclesPerRow at most.")]
        public int SparseCountBonus = 2;

        [Tooltip("Keeps obstacles off the corridor walls, as a multiple of ObstacleRadius.")]
        public float EdgeMarginFactor = 1.2f;

        [Tooltip("Every row places one guaranteed obstacle within this distance of the centerline (x=0, where " +
                 "the player and every shot always travel), as a multiple of ObstacleRadius — so every row " +
                 "blocks the player without forcing the rest of the row together.")]
        public float CenterBlockFactor = 1.2f;
    }
}
