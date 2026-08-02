using System;
using UnityEngine;

namespace Game.Core
{
    [Serializable]
    public class BalanceSettings
    {
        [Tooltip("Blast radius of a zero-size shot — a shot always destroys at least the obstacle it hits.")]
        public float BaseBlastRadius = 0.4f;

        [Tooltip("Blast radius added per unit of shot size: blastRadius = BaseBlastRadius + shotSize * RadiusPerSize.")]
        public float RadiusPerSize = 1.8f;

        [Tooltip("Obstacle-to-obstacle chain propagation range, independent of shot size. Keep between " +
                 "DenseMinSpacingFactor and SparseMinSpacingFactor (x ObstacleRadius) so dense rows chain and sparse ones don't.")]
        public float ChainRadius = 1.7f;

        [Tooltip("Player size at or below which the run is lost.")]
        public float CriticalMinSize = 0.2f;

        [Tooltip("Smallest player start size that still clears the level (hand-tuned by playtesting).")]
        public float MinPlayableSize = 0.8f;

        [Tooltip("Multiplier on MinPlayableSize when ApplyStartSizeBuffer is on. 1.2 = the spec's +20% margin.")]
        public float StartSizeBuffer = 1.2f;

        [Tooltip("Off: start size = MinPlayableSize exactly, for playtesting the true minimum. On: the +20% margin the spec requires.")]
        public bool ApplyStartSizeBuffer = false;
    }
}
