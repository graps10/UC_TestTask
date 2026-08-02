using Game.Core;
using Game.Obstacles;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Player
{
    // Reads tap/hold/release
    // and drives the PlayerBall/Shot resource transfer while charging
    public class ShotController : MonoBehaviour
    {
        [Tooltip("Shot size gained per second the tap is held; the player shrinks by the same amount.")]
        [SerializeField] private float growthRate = 1.0f;

        [Tooltip("Caps per-frame charge growth so a stalled/hitched frame can't dump a huge jump.")]
        [SerializeField] private float maxChargeDeltaTime = 0.05f;

        private PlayerBall _player;
        private ObstacleField _obstacles;
        private Transform _aimTarget;
        private BalanceSettings _balance;
        private Shot _shotPrefab;

        private Shot _activeShot;
        private float _chargeStartSize;
        private float _currentShotSize;
        private bool _charging;

        public void Initialize(
            PlayerBall player,
            ObstacleField obstacles,
            Transform aimTarget,
            BalanceSettings balance,
            Shot shotPrefab)
        {
            _player = player;
            _obstacles = obstacles;
            _aimTarget = aimTarget;
            _balance = balance;
            _shotPrefab = shotPrefab;
        }

        private void Update()
        {
            var pointer = Pointer.current;
            if (pointer == null)
                return;
            
            if (!_charging && pointer.press.wasPressedThisFrame)
                BeginCharge();

            if (_charging && pointer.press.isPressed)
                ContinueCharge();

            if (_charging && pointer.press.wasReleasedThisFrame)
                ReleaseShot();
        }

        private void BeginCharge()
        {
            _charging = true;
            _chargeStartSize = _player.CurrentSize;
            _currentShotSize = 0f;

            _activeShot = SpawnShot();
            _activeShot.BeginCharge(PlayerVisualPosition());
        }

        private void ContinueCharge()
        {
            float dt = Mathf.Min(Time.deltaTime, maxChargeDeltaTime);
            _currentShotSize += growthRate * dt;
            float remaining = _chargeStartSize - _currentShotSize;

            _player.SetSize(remaining);

            if (remaining <= _balance.CriticalMinSize)
            {
                _charging = false;
                return;
            }

            _activeShot.UpdateCharge(_currentShotSize, PlayerVisualPosition());
        }
        
        private Vector3 PlayerVisualPosition()
        {
            return _player.transform.position + Vector3.up * (_player.CurrentSize * 0.5f);
        }

        // Called on win/lose so a mid-charge shot doesn't hang in the air unresolved.
        public void CancelCharge()
        {
            if (!_charging)
                return;

            _charging = false;
            if (_activeShot != null)
                Destroy(_activeShot.gameObject);
            _activeShot = null;
        }

        private void ReleaseShot()
        {
            _charging = false;

            Vector3 direction = _aimTarget.position - _activeShot.transform.position;
            direction.y = 0f;

            _activeShot.Launch(direction, OnShotHitObstacle);
            _activeShot = null;
        }

        private void OnShotHitObstacle(int obstacleIndex, float shotSize)
        {
            _obstacles.Explode(obstacleIndex, shotSize);
        }

        private Shot SpawnShot()
        {
            return Instantiate(_shotPrefab);
        }
    }
}
