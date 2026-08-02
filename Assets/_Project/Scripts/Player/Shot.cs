using System;
using Game.Obstacles;
using UnityEngine;

namespace Game.Player
{
    // One object, three life phases: grows in place while charging, flies straight
    // once launched, resolves (and destroys itself) on the first live obstacle hit.
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public class Shot : MonoBehaviour
    {
        [Tooltip("Visual sphere child (hand-authored in the Shot prefab).")]
        [SerializeField] private Transform visual;

        [Tooltip("Speed the shot travels at once launched, in units/second.")]
        [SerializeField] private float flightSpeed = 15f;

        [Tooltip("Safety cleanup: seconds after launch before the shot destroys itself if it hasn't hit anything.")]
        [SerializeField] private float maxLifetime = 5f;

        [Tooltip("Keeps the shot visible even at size 0 right at the start of a charge.")]
        [SerializeField] private float minVisualSize = 0.05f;

        private SphereCollider _collider;

        private float _size;
        private Vector3 _direction;
        private bool _flying;
        private float _launchTime;
        private Action<int, float> _onHitObstacle;
        private bool _componentsReady;

        private void Awake()
        {
            EnsureComponentsReady();
        }

        public void BeginCharge(Vector3 position)
        {
            EnsureComponentsReady();
            transform.position = position;
        }

        public void UpdateCharge(float size, Vector3 position)
        {
            _size = size;
            transform.position = position;
            SetVisualSize(size);
        }

        private void EnsureComponentsReady()
        {
            if (_componentsReady)
                return;
            _componentsReady = true;

            var rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            _collider = GetComponent<SphereCollider>();
            _collider.isTrigger = true;

            SetVisualSize(0f);
        }

        public void Launch(Vector3 direction, Action<int, float> onHitObstacle)
        {
            _direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector3.forward;
            _onHitObstacle = onHitObstacle;
            _flying = true;
            _launchTime = Time.time;
        }

        private void Update()
        {
            if (!_flying)
                return;

            transform.position += _direction * (flightSpeed * Time.deltaTime);

            if (Time.time - _launchTime > maxLifetime)
                Destroy(gameObject); // missed everything; safety cleanup
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_flying)
                return;

            var obstacle = other.GetComponent<Obstacle>();
            if (obstacle == null || !obstacle.IsAlive)
                return;

            _flying = false;
            _onHitObstacle?.Invoke(obstacle.Index, _size);
            Destroy(gameObject);
        }
        
        private void SetVisualSize(float size)
        {
            float clamped = Mathf.Max(size, minVisualSize);
            visual.localScale = Vector3.one * clamped;
            visual.localPosition = new Vector3(0f, clamped * 0.5f, 0f);
        }
    }
}
