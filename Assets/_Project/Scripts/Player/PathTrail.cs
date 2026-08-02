using Game.Shared;
using UnityEngine;

namespace Game.Player
{
    // Visual-only strip tracking the player's traveled distance and current size.
    public class PathTrail : MonoBehaviour
    {
        [Tooltip("Vertical thickness of the trail strip, in units.")]
        [SerializeField] private float thickness = 0.02f;

        [Tooltip("Small vertical offset above the ground to avoid z-fighting.")]
        [SerializeField] private float groundOffset = 0.01f;

        [Tooltip("Minimum trail width, so it never collapses to a degenerate mesh if the player's size hits 0.")]
        [SerializeField] private float minWidth = 0.01f;

        [Tooltip("Trail color.")]
        [SerializeField] private Color trailColor = new Color(0.95f, 0.35f, 0.55f);

        private PlayerBall _player;
        private float _startZ;
        private Transform _visual;

        public void Initialize(float startZ, PlayerBall player)
        {
            _startZ = startZ;
            _player = player;
            _visual = RuntimePrimitives.CreateVisualCube("Trail", transform, RuntimeMaterials.GetOrCreate(trailColor));
            Refresh();
        }

        private void Update()
        {
            Refresh();
        }
        
        public void Refresh()
        {
            float currentZ = _player.transform.position.z;
            float length = Mathf.Max(0f, currentZ - _startZ);
            float width = Mathf.Max(minWidth, _player.CurrentSize);

            _visual.localScale = new Vector3(width, thickness, length);
            _visual.position = new Vector3(transform.position.x, transform.position.y + groundOffset, _startZ + length * 0.5f);
        }
    }
}
