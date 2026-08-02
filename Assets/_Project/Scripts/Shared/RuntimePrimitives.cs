using UnityEngine;

namespace Game.Shared
{
    // Visual-only primitives (MeshFilter + MeshRenderer, no collider) built from Unity's
    // built-in meshes.
    public static class RuntimePrimitives
    {
        public static Transform CreateVisualSphere(string name, Transform parent, Material material)
        {
            return CreateVisual(name, parent, material, "Sphere.fbx");
        }

        public static Transform CreateVisualCube(string name, Transform parent, Material material)
        {
            return CreateVisual(name, parent, material, "Cube.fbx");
        }

        public static Transform CreateVisualCapsule(string name, Transform parent, Material material)
        {
            return CreateVisual(name, parent, material, "Capsule.fbx");
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
