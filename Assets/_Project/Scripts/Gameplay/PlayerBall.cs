using System;
using UnityEngine;

namespace Game.Gameplay
{
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public class PlayerBall : MonoBehaviour
    {
        [SerializeField] private float forwardSpeed = 2f;
        [SerializeField] private float hopHeight = 0.15f;
        [SerializeField] private float hopFrequency = 4f;
        [SerializeField] private Color ballColor = new Color(0.95f, 0.9f, 0.8f);

        public float CurrentSize { get; private set; }
        public event Action OnCriticalSizeReached;

        private Rigidbody _rb;
        private SphereCollider _collider;
        private Transform _visual;
        private float _criticalMinSize;
        private bool _criticalFired;
        private bool _canMove = true;
        private bool _componentsReady;

        private void Awake()
        {
            EnsureComponentsReady();
        }

        public void Initialize(float startSize, BalanceSettings balance)
        {
            EnsureComponentsReady();
            _criticalMinSize = balance.CriticalMinSize;
            _criticalFired = false;
            SetSize(startSize);
        }
        
        private void EnsureComponentsReady()
        {
            if (_componentsReady)
                return;
            _componentsReady = true;

            _rb = GetComponent<Rigidbody>();
            _rb.isKinematic = true;
            _rb.useGravity = false;

            _collider = GetComponent<SphereCollider>();

            _visual = BuildVisual();
        }

        public void SetCanMove(bool canMove)
        {
            _canMove = canMove;
        }
        
        public void SetSize(float newSize)
        {
            CurrentSize = Mathf.Max(0f, newSize);
            _visual.localScale = Vector3.one * CurrentSize;
            _collider.radius = CurrentSize * 0.5f;

            if (!_criticalFired && CurrentSize <= _criticalMinSize)
            {
                _criticalFired = true;
                OnCriticalSizeReached?.Invoke();
            }
        }

        private void FixedUpdate()
        {
            if (!_canMove)
                return;

            Vector3 delta = Vector3.forward * (forwardSpeed * Time.fixedDeltaTime);
            Vector3 targetPos = _rb.position + delta;

            if (!IsBlockedAhead(targetPos))
                _rb.MovePosition(targetPos);
        }

        private void Update()
        {
            float bob = Mathf.Abs(Mathf.Sin(Time.time * hopFrequency)) * hopHeight;
            _visual.localPosition = new Vector3(0f, bob, 0f);
        }

        private bool IsBlockedAhead(Vector3 targetPos)
        {
            float checkRadius = CurrentSize * 0.5f * 0.9f;
            var hits = Physics.OverlapSphere(targetPos, checkRadius);
            foreach (var hit in hits)
            {
                var obstacle = hit.GetComponent<Obstacle>();
                if (obstacle != null && obstacle.IsAlive)
                    return true;
            }
            return false;
        }

        private Transform BuildVisual()
        {
            return RuntimePrimitives.CreateVisualSphere("Visual", transform, RuntimeMaterials.GetOrCreate(ballColor));
        }
    }
}
