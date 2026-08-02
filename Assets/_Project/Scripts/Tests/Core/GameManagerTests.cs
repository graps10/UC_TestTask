using Game.Core;
using Game.Level;
using Game.Player;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class GameManagerTests
    {
        private GameObject _root;

        [TearDown]
        public void TearDown()
        {
            if (_root != null)
                Object.DestroyImmediate(_root);
        }

        private (GameManager manager, PlayerBall player, ShotController shotController, Door door) BuildScene()
        {
            _root = new GameObject("GameManagerTestRoot");

            var playerGo = new GameObject("Player");
            playerGo.transform.SetParent(_root.transform);
            var player = playerGo.AddComponent<PlayerBall>();

            var shotControllerGo = new GameObject("ShotController");
            shotControllerGo.transform.SetParent(_root.transform);
            var shotController = shotControllerGo.AddComponent<ShotController>();

            var doorGo = new GameObject("Door");
            doorGo.transform.SetParent(_root.transform);
            var door = doorGo.AddComponent<Door>();
            door.Initialize(new Vector3(0f, 0f, 10f), 6f, player.transform);

            var managerGo = new GameObject("GameManager");
            managerGo.transform.SetParent(_root.transform);
            var manager = managerGo.AddComponent<GameManager>();

            return (manager, player, shotController, door);
        }

        private static LevelLayout SimpleSolvableLayout()
        {
            var layout = new LevelLayout { CorridorWidth = 6f, ObstacleRadius = 0.5f, StartZ = 0f, DoorZ = 10f };
            var row = new LevelRow(4f);
            row.ObstacleX.Add(0f);
            layout.Rows.Add(row);
            return layout;
        }

        private static BalanceSettings DefaultBalance()
        {
            return new BalanceSettings
            {
                BaseBlastRadius = 0.3f,
                RadiusPerSize = 1.0f,
                ChainRadius = 1.5f,
                CriticalMinSize = 0.4f,
                GapClearanceFactor = 1.15f,
            };
        }

        [Test]
        public void Initialize_AppliesTwentyPercentBufferToPlayerStartSize()
        {
            var (manager, player, shotController, door) = BuildScene();
            var layout = SimpleSolvableLayout();
            var balance = DefaultBalance();

            manager.Initialize(layout, balance, player, shotController, door);

            float expectedMin = LevelSolver.ComputeMinimumRequiredSize(layout, balance, out _);
            Assert.AreEqual(expectedMin * 1.2f, player.CurrentSize, 0.01f);
        }

        [Test]
        public void PlayerReachingCriticalSize_TriggersLose()
        {
            var (manager, player, shotController, door) = BuildScene();
            manager.Initialize(SimpleSolvableLayout(), DefaultBalance(), player, shotController, door);

            bool loseFired = false;
            manager.OnLose += () => loseFired = true;

            player.SetSize(0.1f); // below CriticalMinSize

            Assert.IsTrue(loseFired);
            Assert.AreEqual(GameState.Lost, manager.State);
            Assert.IsFalse(shotController.enabled);
        }

        [Test]
        public void DoorEntered_TriggersWin()
        {
            var (manager, player, shotController, door) = BuildScene();
            manager.Initialize(SimpleSolvableLayout(), DefaultBalance(), player, shotController, door);

            bool winFired = false;
            manager.OnWin += () => winFired = true;

            door.NotifyTriggerEnter(player.GetComponent<Collider>());

            Assert.IsTrue(winFired);
            Assert.AreEqual(GameState.Won, manager.State);
        }

        [Test]
        public void WinThenLose_OnlyFirstTransitionCounts()
        {
            var (manager, player, shotController, door) = BuildScene();
            manager.Initialize(SimpleSolvableLayout(), DefaultBalance(), player, shotController, door);

            int winCount = 0, loseCount = 0;
            manager.OnWin += () => winCount++;
            manager.OnLose += () => loseCount++;

            door.NotifyTriggerEnter(player.GetComponent<Collider>());
            player.SetSize(0.1f); // must be ignored, already Won

            Assert.AreEqual(1, winCount);
            Assert.AreEqual(0, loseCount);
            Assert.AreEqual(GameState.Won, manager.State);
        }
    }
}
