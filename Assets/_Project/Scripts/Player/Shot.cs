using System;
using Game.Obstacles;
using Game.Shared;
using UnityEngine;

namespace Game.Player
{
    // One object, three life phases: grows in place while charging, flies straight
    // once launched, resolves (and destroys itself) on the first live obstacle hit.
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public class Shot : MonoBehaviour
    {
        [SerializeField] private float flightSpeed = 15f;
        [SerializeField] private float maxLifetime = 5f;
        [SerializeField] private Color shotColor = new Color(0.95f, 0.6f, 0.15f);

        private Transform _visual;
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

            _visual = BuildVisual();
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
            _visual.localScale = Vector3.one * Mathf.Max(size, 0.05f);
        }

        private Transform BuildVisual()
        {
            return RuntimePrimitives.CreateVisualSphere("Visual", transform, RuntimeMaterials.GetOrCreate(shotColor));
        }
    }
}
