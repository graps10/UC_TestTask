using System;
using DG.Tweening;
using Game.Player;
using Game.Shared;
using UnityEngine;

namespace Game.Level
{
    public class Door : MonoBehaviour
    {
        [SerializeField] private float openDistance = 5f;
        [SerializeField] private float openDuration = 0.5f;
        [SerializeField] private float panelWidth = 1.5f;
        [SerializeField] private float panelHeight = 2.5f;
        [SerializeField] private Color doorColor = new Color(0.9f, 0.75f, 0.2f);

        public event Action OnPlayerEntered;

        private Transform _player;
        private Transform _leftPanel;
        private Transform _rightPanel;
        private bool _opened;
        private bool _entered;

        public void Initialize(Vector3 position, float corridorWidth, Transform player)
        {
            transform.position = position;
            _player = player;

            BuildVisual();
            BuildTrigger(corridorWidth);
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
            _leftPanel.DOLocalMoveX(-panelWidth * 1.5f, openDuration).SetEase(Ease.OutQuad);
            _rightPanel.DOLocalMoveX(panelWidth * 1.5f, openDuration).SetEase(Ease.OutQuad);
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

        private void BuildVisual()
        {
            var material = RuntimeMaterials.GetOrCreate(doorColor);

            _leftPanel = RuntimePrimitives.CreateVisualCube("LeftPanel", transform, material);
            _leftPanel.localScale = new Vector3(panelWidth, panelHeight, 0.3f);
            _leftPanel.localPosition = new Vector3(-panelWidth * 0.5f, panelHeight * 0.5f, 0f);

            _rightPanel = RuntimePrimitives.CreateVisualCube("RightPanel", transform, material);
            _rightPanel.localScale = new Vector3(panelWidth, panelHeight, 0.3f);
            _rightPanel.localPosition = new Vector3(panelWidth * 0.5f, panelHeight * 0.5f, 0f);
        }

        private void BuildTrigger(float corridorWidth)
        {
            var trigger = gameObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(corridorWidth, panelHeight * 2f, 2f);
            trigger.center = new Vector3(0f, panelHeight, 0f);
        }
    }
}
