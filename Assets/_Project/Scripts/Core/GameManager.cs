using System;
using Game.Level;
using Game.Player;
using UnityEngine;

namespace Game.Core
{
    public enum GameState
    {
        Playing,
        Won,
        Lost,
    }

    // Owns game state and rules only
    public class GameManager : MonoBehaviour
    {
        [Tooltip("Safety margin applied on top of LevelSolver's computed minimum, per the spec's " +
                 "\"must have a 20% buffer from the start\" requirement. 1.2 = +20%.")]
        [SerializeField] private float startSizeBuffer = 1.2f;

        public GameState State { get; private set; } = GameState.Playing;
        public event Action OnWin;
        public event Action OnLose;

        private PlayerBall _player;
        private ShotController _shotController;
        private Door _door;
        
        public void Initialize(
            LevelLayout layout,
            BalanceSettings balance,
            PlayerBall player,
            ShotController shotController,
            Door door)
        {
            _player = player;
            _shotController = shotController;
            _door = door;

            float minRequiredSize = LevelSolver.ComputeMinimumRequiredSize(layout, balance, out bool solvable);
            if (!solvable)
            {
                Debug.LogWarning("GameManager: generated level has no solvable starting size within the search " +
                                  "range; falling back to the physical ceiling as a best effort.");
            }

            _player.Initialize(minRequiredSize * startSizeBuffer, balance);

            _player.OnCriticalSizeReached += HandleLose;
            _door.OnPlayerEntered += HandleWin;
        }

        private void HandleWin()
        {
            if (State != GameState.Playing)
                return;

            State = GameState.Won;
            Freeze();
            OnWin?.Invoke();
        }

        private void HandleLose()
        {
            if (State != GameState.Playing)
                return;

            State = GameState.Lost;
            Freeze();
            OnLose?.Invoke();
        }

        private void Freeze()
        {
            _player.SetCanMove(false);
            _shotController.enabled = false;
        }

        private void OnDestroy()
        {
            if (_player != null)
                _player.OnCriticalSizeReached -= HandleLose;
            if (_door != null)
                _door.OnPlayerEntered -= HandleWin;
        }
    }
}
