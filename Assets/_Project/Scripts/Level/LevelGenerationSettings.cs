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
        public float CorridorWidth = 6f;

        [Tooltip("Radius of a single obstacle, in units. In-game, GameBootstrap overwrites this " +
                 "from the Obstacle prefab's own CapsuleCollider at startup — editing it here only " +
                 "matters for calling LevelGenerator/LevelSolver directly (e.g. from tests).")]
        public float ObstacleRadius = 0.5f;

        [Tooltip("Player's spawn Z position.")]
        public float StartClearZ = 0f;

        [Tooltip("Z position of the first obstacle row (gives the player room before obstacles start).")]
        public float FirstRowZ = 4f;

        [Tooltip("Distance between consecutive obstacle rows, in units.")]
        public float RowSpacing = 3f;

        [Tooltip("Number of obstacle rows to generate.")]
        public int RowCount = 12;

        [Tooltip("Empty gap between the last obstacle row and the door.")]
        public float DoorApproachClearance = 3f;

        [Tooltip("Minimum obstacles a sparse row can have.")]
        public int MinObstaclesPerRow = 3;

        [Tooltip("Maximum obstacles a dense row can have.")]
        public int MaxObstaclesPerRow = 7;

        [Tooltip("Chance a row is generated as a tightly-packed cluster (chain-reaction showcase) " +
                 "instead of sparse/spread-out obstacles (single-big-shot showcase).")]
        [Range(0f, 1f)]
        public float DenseRowChance = 0.5f;

        [Tooltip("How close together (as a multiple of ObstacleRadius) obstacles in a dense row are allowed to sit.")]
        public float DenseMinSpacingFactor = 1.6f;

        [Tooltip("How far apart (as a multiple of ObstacleRadius) obstacles in a sparse row are forced to sit.")]
        public float SparseMinSpacingFactor = 4f;

        [Tooltip("Dense rows get up to this many fewer obstacles than MaxObstaclesPerRow at minimum.")]
        public int DenseCountReduction = 2;

        [Tooltip("Sparse rows get up to this many more obstacles than MinObstaclesPerRow at most.")]
        public int SparseCountBonus = 2;

        [Tooltip("Keeps obstacles from spawning flush against the corridor walls, as a multiple of ObstacleRadius.")]
        public float EdgeMarginFactor = 1.2f;

        [Tooltip("Maximum natural gap allowed between neighbouring obstacles (or a corridor wall) in a " +
                 "row, as a multiple of ObstacleRadius — a filler obstacle is inserted into any larger " +
                 "gap. Prevents a row from ever being freely walkable, which would otherwise let " +
                 "LevelSolver find a degenerate 'shrink to a tiny ball and sneak through unshot' " +
                 "solution instead of a real one. Keep this comfortably below SparseMinSpacingFactor " +
                 "or it'll fight the sparse-row spacing and pack every row solid.")]
        public float MaxGapFactor = 2.5f;
    }
}
