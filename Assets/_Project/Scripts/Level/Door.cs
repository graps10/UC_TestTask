using System;
using DG.Tweening;
using Game.Player;
using UnityEngine;

namespace Game.Level
{
    [RequireComponent(typeof(BoxCollider))]
    public class Door : MonoBehaviour
    {
        [Tooltip("Left door panel (hand-authored in the Door prefab, positioned as 'closed').")]
        [SerializeField] private Transform leftPanel;

        [Tooltip("Right door panel (hand-authored in the Door prefab, positioned as 'closed').")]
        [SerializeField] private Transform rightPanel;

        [Tooltip("Distance from the player at which the door starts opening, in world units (spec: 5m).")]
        [SerializeField] private float openDistance = 5f;

        [Tooltip("How long the opening slide animation takes, in seconds.")]
        [SerializeField] private float openDuration = 0.5f;

        [Tooltip("How far each panel slides out from its authored 'closed' position when opening, in units.")]
        [SerializeField] private float openSlideDistance = 2f;

        public event Action OnPlayerEntered;

        private Transform _player;
        private float _leftClosedX;
        private float _rightClosedX;
        private bool _opened;
        private bool _entered;

        public void Initialize(Vector3 position, Transform player)
        {
            transform.position = position;
            _player = player;

            _leftClosedX = leftPanel.localPosition.x;
            _rightClosedX = rightPanel.localPosition.x;
        }

        private void Update()
        {
            if (_opened || _player == null)
                return;

            if (Vector3.Distance(_player.position, transform.position) <= openDistance)
                Open();
        }

        private void Open()
        {
            _opened = true;
            leftPanel.DOLocalMoveX(_leftClosedX - openSlideDistance, openDuration).SetEase(Ease.OutQuad);
            rightPanel.DOLocalMoveX(_rightClosedX + openSlideDistance, openDuration).SetEase(Ease.OutQuad);
        }

        private void OnTriggerEnter(Collider other)
        {
            NotifyTriggerEnter(other);
        }

        // Split out from OnTriggerEnter so tests can drive the win condition directly
        public void NotifyTriggerEnter(Collider other)
        {
            if (_entered || other.GetComponent<PlayerBall>() == null)
                return;

            _entered = true;
            OnPlayerEntered?.Invoke();
        }
        
        public void SetPanelsForTest(Transform leftPanelRef, Transform rightPanelRef)
        {
            leftPanel = leftPanelRef;
            rightPanel = rightPanelRef;
        }
    }
}
