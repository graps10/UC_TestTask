using UnityEngine;

namespace Game.Shared
{
    // Visual-only primitive (MeshFilter + MeshRenderer, no collider) built from Unity's built-in
    // cube mesh. Only PathTrail still needs this — everything else uses hand-authored prefabs.
    public static class RuntimePrimitives
    {
        public static Transform CreateVisualCube(string name, Transform parent, Material material)
        {
            return CreateVisual(name, parent, material, "Cube.fbx");
        }

        private static Transform CreateVisual(string name, Transform parent, Material material, string builtinMeshName)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = Resources.GetBuiltinResource<Mesh>(builtinMeshName);

            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;

            return go.transform;
        }
    }
}
