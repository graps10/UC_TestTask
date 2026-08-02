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
        public GameState State { get; private set; } = GameState.Playing;
        public event Action OnWin;
        public event Action OnLose;

        private PlayerBall _player;
        private ShotController _shotController;
        private Door _door;

        public void Initialize(
            BalanceSettings balance,
            PlayerBall player,
            ShotController shotController,
            Door door)
        {
            _player = player;
            _shotController = shotController;
            _door = door;

            float startSize = balance.ApplyStartSizeBuffer
                ? balance.MinPlayableSize * balance.StartSizeBuffer
                : balance.MinPlayableSize;
            _player.Initialize(startSize, balance);

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
            _shotController.CancelCharge();
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
