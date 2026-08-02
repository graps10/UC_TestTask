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

        [Tooltip("Caps the deltaTime used for charge growth, so a single hitched/stalled frame " +
                 "(e.g. right after a heavy scene load) can't instantly dump a huge chunk of growth.")]
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
            {
                BeginCharge();
            }

            if (_charging && pointer.press.isPressed)
            {
                ContinueCharge();
            }

            if (_charging && pointer.press.wasReleasedThisFrame)
            {
                ReleaseShot();
            }
        }

        private void BeginCharge()
        {
            _charging = true;
            _chargeStartSize = _player.CurrentSize;
            _currentShotSize = 0f;

            _activeShot = SpawnShot();
            _activeShot.BeginCharge(_player.transform.position);
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

            _activeShot.UpdateCharge(_currentShotSize, _player.transform.position);
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
