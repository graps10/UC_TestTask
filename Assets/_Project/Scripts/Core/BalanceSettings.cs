using System;
using UnityEngine;

namespace Game.Core
{
    [Serializable]
    public class BalanceSettings
    {
        [Tooltip("Blast radius of a zero-size shot (a shot always destroys at least the obstacle it hits).")]
        public float BaseBlastRadius = 0.3f;

        [Tooltip("How much blast radius each unit of shot size adds: blastRadius = BaseBlastRadius + shotSize * RadiusPerSize.")]
        public float RadiusPerSize = 1.0f;

        [Tooltip("Fixed obstacle-to-obstacle propagation range for the chain reaction, independent of shot size — " +
                 "this is what makes tightly-packed obstacles chain-destroy from a small hit.")]
        public float ChainRadius = 1.5f;

        [Tooltip("Player size threshold: at or below this, the run is lost (over-held a charge, or spent too much across shots).")]
        public float CriticalMinSize = 0.4f;

        [Tooltip("Required gap width at a row = currentSize * GapClearanceFactor (safety margin so the ball visibly fits).")]
        public float GapClearanceFactor = 1.15f;
    }
}
