using System.Collections.Generic;
using Game.Core;
using Game.Obstacles;
using UnityEngine;

namespace Game.Level
{
    // Finds the smallest player StartSize that can clear a
    // generated LevelLayout, using the exact same ChainReactionSolver rules the
    // runtime game uses, so the balance figure can never drift from actual gameplay.
    public static class LevelSolver
    {
        private const int CoarseSteps = 200;
        private const int RefineIterations = 25;
        private const int InnerShotSearchIterations = 40;

        public static float ComputeMinimumRequiredSize(LevelLayout layout, BalanceSettings balance, out bool solvable)
        {
            float low = balance.CriticalMinSize;
            float physicalCeiling = layout.CorridorWidth / balance.GapClearanceFactor;
            float step = (physicalCeiling - low) / CoarseSteps;

            float foundCandidate = float.PositiveInfinity;
            for (int i = 1; i <= CoarseSteps; i++)
            {
                float candidate = low + step * i;
                if (CanClearLevel(layout, balance, candidate))
                {
                    foundCandidate = candidate;
                    break;
                }
            }

            if (float.IsPositiveInfinity(foundCandidate))
            {
                solvable = false;
                return physicalCeiling;
            }

            float windowLow = foundCandidate - step;
            float windowHigh = foundCandidate;
            for (int i = 0; i < RefineIterations; i++)
            {
                float mid = (windowLow + windowHigh) * 0.5f;
                if (CanClearLevel(layout, balance, mid)) windowHigh = mid; else windowLow = mid;
            }

            solvable = true;
            return windowHigh;
        }

        public static bool CanClearLevel(LevelLayout layout, BalanceSettings balance, float startSize)
        {
            float currentSize = startSize;
            foreach (var row in layout.Rows)
            {
                if (row.ObstacleX.Count == 0)
                    continue;
                if (currentSize <= balance.CriticalMinSize)
                    return false;

                float requiredGap = currentSize * balance.GapClearanceFactor;
                float spend = MinimumShotToOpenGap(row, layout, balance, currentSize, requiredGap);
                if (float.IsPositiveInfinity(spend))
                    return false;

                currentSize -= spend;
                if (currentSize <= balance.CriticalMinSize)
                    return false;
            }
            return true;
        }

        private static float MinimumShotToOpenGap(
            LevelRow row, LevelLayout layout, BalanceSettings balance, float currentSize, float requiredGap)
        {
            float maxAffordable = currentSize - balance.CriticalMinSize;
            if (maxAffordable <= 0f)
                return float.PositiveInfinity;

            if (BestGapForShotSize(row, layout, balance, maxAffordable) < requiredGap)
                return float.PositiveInfinity;

            float lo = 0f, hi = maxAffordable;
            for (int i = 0; i < InnerShotSearchIterations; i++)
            {
                float mid = (lo + hi) * 0.5f;
                if (BestGapForShotSize(row, layout, balance, mid) >= requiredGap) hi = mid; else lo = mid;
            }
            return hi;
        }

        // Tries every obstacle in the row as the impact point and returns the widest
        // gap achievable for a given shot size.
        private static float BestGapForShotSize(LevelRow row, LevelLayout layout, BalanceSettings balance, float shotSize)
        {
            int n = row.ObstacleX.Count;
            if (n == 0)
                return float.PositiveInfinity;

            var positions = new List<Vector3>(n);
            var alive = new List<bool>(n);
            for (int i = 0; i < n; i++)
            {
                positions.Add(new Vector3(row.ObstacleX[i], 0f, row.Z));
                alive.Add(true);
            }

            float blastRadius = ChainReactionSolver.ComputeBlastRadius(shotSize, balance.BaseBlastRadius, balance.RadiusPerSize);

            float best = 0f;
            for (int impact = 0; impact < n; impact++)
            {
                var destroyed = ChainReactionSolver.Simulate(positions, alive, impact, blastRadius, balance.ChainRadius);
                float gap = MaxContiguousGap(row, layout, destroyed);
                if (gap > best)
                    best = gap;
            }
            return best;
        }

        private static float MaxContiguousGap(LevelRow row, LevelLayout layout, HashSet<int> destroyed)
        {
            float halfWidth = layout.CorridorWidth * 0.5f;
            float r = layout.ObstacleRadius;

            var survivors = new List<float>();
            for (int i = 0; i < row.ObstacleX.Count; i++)
            {
                if (!destroyed.Contains(i))
                    survivors.Add(row.ObstacleX[i]);
            }
            survivors.Sort();

            float best = 0f;
            float prevEdge = -halfWidth;
            foreach (var x in survivors)
            {
                float leftEdge = x - r;
                best = Mathf.Max(best, leftEdge - prevEdge);
                prevEdge = x + r;
            }
            best = Mathf.Max(best, halfWidth - prevEdge);
            return best;
        }
    }
}
