using System.Collections.Generic;
using UnityEngine;

namespace Game.Obstacles
{
    // Pure obstacle-destruction simulation
    public static class ChainReactionSolver
    {
        public static float ComputeBlastRadius(float shotSize, float baseRadius, float radiusPerSize)
        {
            return baseRadius + Mathf.Max(0f, shotSize) * radiusPerSize;
        }

        // Destroys everything within blastRadius of the impact obstacle, then propagates
        // transitively to alive neighbours within the smaller, size-independent chainRadius.
        public static HashSet<int> Simulate(
            IReadOnlyList<Vector3> positions,
            IReadOnlyList<bool> alive,
            int impactIndex,
            float blastRadius,
            float chainRadius)
        {
            var destroyed = new HashSet<int>();
            if (impactIndex < 0 || impactIndex >= positions.Count || !alive[impactIndex])
                return destroyed;

            var queue = new Queue<int>();
            Vector3 impactPoint = positions[impactIndex];

            for (int i = 0; i < positions.Count; i++)
            {
                if (!alive[i] || destroyed.Contains(i))
                    continue;
                if (Vector3.Distance(positions[i], impactPoint) <= blastRadius)
                {
                    destroyed.Add(i);
                    queue.Enqueue(i);
                }
            }

            while (queue.Count > 0)
            {
                int source = queue.Dequeue();
                Vector3 sourcePos = positions[source];
                for (int i = 0; i < positions.Count; i++)
                {
                    if (!alive[i] || destroyed.Contains(i))
                        continue;
                    if (Vector3.Distance(positions[i], sourcePos) <= chainRadius)
                    {
                        destroyed.Add(i);
                        queue.Enqueue(i);
                    }
                }
            }

            return destroyed;
        }
    }
}
