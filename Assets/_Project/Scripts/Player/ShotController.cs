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
        [SerializeField] private float growthRate = 1.0f; // shot size gained per second held

        private PlayerBall _player;
        private ObstacleField _obstacles;
        private Transform _aimTarget;
        private BalanceSettings _balance;

        private Shot _activeShot;
        private float _chargeStartSize;
        private float _currentShotSize;
        private bool _charging;

        public void Initialize(PlayerBall player, ObstacleField obstacles, Transform aimTarget, BalanceSettings balance)
        {
            _player = player;
            _obstacles = obstacles;
            _aimTarget = aimTarget;
            _balance = balance;
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
            else if (_charging && pointer.press.isPressed)
            {
                ContinueCharge();
            }
            else if (_charging && pointer.press.wasReleasedThisFrame)
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
            _currentShotSize += growthRate * Time.deltaTime;
            float remaining = _chargeStartSize - _currentShotSize;

            _player.SetSize(remaining);

            if (remaining <= _balance.CriticalMinSize)
            {
                // Over-held past the critical threshold: PlayerBall already raised
                // OnCriticalSizeReached from SetSize above. Freeze the charge in place
                // rather than launching it — GameManager will take over from here.
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
            var go = new GameObject("Shot");
            return go.AddComponent<Shot>();
        }
    }
}
