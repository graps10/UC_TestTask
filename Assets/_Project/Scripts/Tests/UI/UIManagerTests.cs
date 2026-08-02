using Game.Core;
using Game.Level;
using Game.Player;
using Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Tests
{
    public class UIManagerTests
    {
        private GameObject _root;

        [TearDown]
        public void TearDown()
        {
            if (_root != null)
                Object.DestroyImmediate(_root);
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

        private static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private (UIManager ui, GameManager manager, PlayerBall player, Image fill, GameObject winPanel, GameObject losePanel) BuildScene()
        {
            _root = new GameObject("UIManagerTestRoot");

            var playerGo = new GameObject("Player");
            playerGo.transform.SetParent(_root.transform);
            var player = playerGo.AddComponent<PlayerBall>();
            var playerVisual = new GameObject("Visual").transform;
            playerVisual.SetParent(playerGo.transform);
            player.SetVisualForTest(playerVisual);

            var shotControllerGo = new GameObject("ShotController");
            shotControllerGo.transform.SetParent(_root.transform);
            var shotController = shotControllerGo.AddComponent<ShotController>();

            var doorGo = new GameObject("Door");
            doorGo.transform.SetParent(_root.transform);
            var door = doorGo.AddComponent<Door>();
            var leftPanel = new GameObject("LeftPanel").transform;
            leftPanel.SetParent(doorGo.transform);
            var rightPanel = new GameObject("RightPanel").transform;
            rightPanel.SetParent(doorGo.transform);
            door.SetPanelsForTest(leftPanel, rightPanel);
            door.Initialize(new Vector3(0f, 0f, 10f), player.transform);

            var managerGo = new GameObject("GameManager");
            managerGo.transform.SetParent(_root.transform);
            var manager = managerGo.AddComponent<GameManager>();

            var layout = new LevelLayout { CorridorWidth = 6f, ObstacleRadius = 0.5f, StartZ = 0f, DoorZ = 10f };
            var row = new LevelRow(4f);
            row.ObstacleX.Add(0f);
            layout.Rows.Add(row);

            manager.Initialize(layout, DefaultBalance(), player, shotController, door);
            
            var uiGo = new GameObject("UIManager");
            uiGo.transform.SetParent(_root.transform);
            var ui = uiGo.AddComponent<UIManager>();

            var fillImage = CreateRect("Fill", uiGo.transform).gameObject.AddComponent<Image>();
            var hintGroup = CreateRect("Hint", uiGo.transform).gameObject.AddComponent<CanvasGroup>();
            var winPanel = CreateRect("WinPanel", uiGo.transform).gameObject;
            var winContent = CreateRect("Content", winPanel.transform);
            var losePanel = CreateRect("LosePanel", uiGo.transform).gameObject;
            var loseContent = CreateRect("Content", losePanel.transform);

            ui.BindViewsForTest(fillImage, hintGroup, winPanel, winContent, losePanel, loseContent);
            ui.Initialize(manager, player, DefaultBalance());

            return (ui, manager, player, fillImage, winPanel, losePanel);
        }

        [Test]
        public void Initialize_PanelsStartInactive()
        {
            var (_, _, _, _, winPanel, losePanel) = BuildScene();

            Assert.IsFalse(winPanel.activeSelf);
            Assert.IsFalse(losePanel.activeSelf);
        }

        [Test]
        public void Initialize_SizeBarStartsFull()
        {
            var (_, _, _, fill, _, _) = BuildScene();

            Assert.AreEqual(1f, fill.fillAmount, 0.0001f);
        }

        [Test]
        public void RefreshSizeBar_TracksPlayerSizeRelativeToStart()
        {
            var (ui, _, player, fill, _, _) = BuildScene();
            float startSize = player.CurrentSize;

            player.SetSize(startSize * 0.5f);
            ui.RefreshSizeBar();

            Assert.AreEqual(0.5f, fill.fillAmount, 0.001f);
        }

        [Test]
        public void PlayerReachingCriticalSize_ActivatesLosePanelOnly()
        {
            var (_, _, player, _, winPanel, losePanel) = BuildScene();

            player.SetSize(0.1f); // below CriticalMinSize -> GameManager.OnLose

            Assert.IsTrue(losePanel.activeSelf);
            Assert.IsFalse(winPanel.activeSelf);
        }

        [Test]
        public void DoorEntered_ActivatesWinPanelOnly()
        {
            var (_, _, player, _, winPanel, losePanel) = BuildScene();
            var door = _root.transform.Find("Door").GetComponent<Door>();

            door.NotifyTriggerEnter(player.GetComponent<Collider>());

            Assert.IsTrue(winPanel.activeSelf);
            Assert.IsFalse(losePanel.activeSelf);
        }
    }
}
