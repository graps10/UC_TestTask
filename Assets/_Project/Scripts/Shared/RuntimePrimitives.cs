using UnityEngine;

namespace Game.Shared
{
    // Visual-only sphere (MeshFilter + MeshRenderer, no collider) built from Unity's built-in sphere mesh.
    public static class RuntimePrimitives
    {
        public static Transform CreateVisualSphere(string name, Transform parent, Material material)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");

            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;

            return go.transform;
        }
    }
}
