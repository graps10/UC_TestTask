using System.Collections.Generic;

namespace Game.Gameplay
{
    // One band of obstacles spanning the corridor width at a fixed Z.
    public class LevelRow
    {
        public readonly float Z;
        public readonly List<float> ObstacleX = new List<float>();

        public LevelRow(float z)
        {
            Z = z;
        }
    }
    
    public class LevelLayout
    {
        public float CorridorWidth;
        public float ObstacleRadius;
        public float StartZ;
        public float DoorZ;
        public readonly List<LevelRow> Rows = new List<LevelRow>();
    }
}
