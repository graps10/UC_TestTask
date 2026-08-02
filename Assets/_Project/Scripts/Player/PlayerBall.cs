using System;
using Game.Core;
using Game.Obstacles;
using UnityEngine;

namespace Game.Player
{
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public class PlayerBall : MonoBehaviour
    {
        [Tooltip("Visual sphere child (hand-authored in the Player prefab).")]
        [SerializeField] private Transform visual;

        [Tooltip("Constant forward movement speed along the corridor, in units/second.")]
        [SerializeField] private float forwardSpeed = 2f;

        [Tooltip("Vertical bob height of the visual sphere while moving, in units.")]
        [SerializeField] private float hopHeight = 0.15f;

        [Tooltip("Vertical bob speed while moving.")]
        [SerializeField] private float hopFrequency = 4f;

        [Tooltip("Shrinks the forward-blocking check slightly below the ball's actual " +
                 "radius so it doesn't snag on obstacles that only just graze its edge.")]
        [SerializeField] private float blockCheckMargin = 0.9f;

        public float CurrentSize { get; private set; }
        public event Action OnCriticalSizeReached;

        private Rigidbody _rb;
        private SphereCollider _collider;
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
        }

        public void SetCanMove(bool canMove)
        {
            _canMove = canMove;
        }
        
        public void SetVisualForTest(Transform visualRef)
        {
            visual = visualRef;
        }

        public void SetSize(float newSize)
        {
            CurrentSize = Mathf.Max(0f, newSize);
            visual.localScale = Vector3.one * CurrentSize;
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
            visual.localPosition = new Vector3(0f, CurrentSize * 0.5f + bob, 0f);
        }

        private bool IsBlockedAhead(Vector3 targetPos)
        {
            float checkRadius = CurrentSize * 0.5f * blockCheckMargin;
            var hits = Physics.OverlapSphere(targetPos, checkRadius);
            foreach (var hit in hits)
            {
                var obstacle = hit.GetComponent<Obstacle>();
                if (obstacle != null && obstacle.IsAlive)
                    return true;
            }
            return false;
        }
    }
}
