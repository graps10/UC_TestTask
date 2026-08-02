using System;

namespace Game.Gameplay
{
    [Serializable]
    public class BalanceSettings
    {
        public float BaseBlastRadius = 0.3f;
        public float RadiusPerSize = 1.0f;
        public float ChainRadius = 1.5f;
        public float CriticalMinSize = 0.4f;

        // Required gap width at a row = currentSize * GapClearanceFactor.
        public float GapClearanceFactor = 1.15f;
    }
}
