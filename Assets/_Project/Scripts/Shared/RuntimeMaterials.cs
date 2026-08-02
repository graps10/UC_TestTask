using System.Collections.Generic;
using UnityEngine;

namespace Game.Shared
{
    // Small cache so every system that spawns primitives at runtime
    // shares one material instance per color instead of allocating
    // a new one per object.
    public static class RuntimeMaterials
    {
        private static readonly Dictionary<Color, Material> Cache = new();

        public static Material GetOrCreate(Color color)
        {
            if (Cache.TryGetValue(color, out var material) && material != null)
                return material;

            material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = color };
            Cache[color] = material;
            return material;
        }
    }
}
