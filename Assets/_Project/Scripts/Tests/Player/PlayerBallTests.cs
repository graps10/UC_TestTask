using Game.Core;
using Game.Player;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class PlayerBallTests
    {
        private GameObject _go;

        [TearDown]
        public void TearDown()
        {
            if (_go != null)
                Object.DestroyImmediate(_go);
        }

        private PlayerBall CreatePlayer()
        {
            _go = new GameObject("PlayerBallTest");
            var player = _go.AddComponent<PlayerBall>();
            var visual = new GameObject("Visual").transform;
            visual.SetParent(_go.transform);
            player.SetVisualForTest(visual);
            return player;
        }

        private static BalanceSettings BuildBalance()
        {
            return new BalanceSettings { CriticalMinSize = 0.4f };
        }

        [Test]
        public void Initialize_SetsCurrentSize()
        {
            var player = CreatePlayer();
            player.Initialize(3f, BuildBalance());

            Assert.AreEqual(3f, player.CurrentSize, 0.0001f);
        }

        [Test]
        public void SetSize_UpdatesCurrentSize()
        {
            var player = CreatePlayer();
            player.Initialize(3f, BuildBalance());

            player.SetSize(2f);

            Assert.AreEqual(2f, player.CurrentSize, 0.0001f);
        }

        [Test]
        public void SetSize_AtOrBelowCritical_FiresEventExactlyOnce()
        {
            var player = CreatePlayer();
            player.Initialize(3f, BuildBalance());

            int fireCount = 0;
            player.OnCriticalSizeReached += () => fireCount++;

            player.SetSize(0.5f); // above critical: no fire
            player.SetSize(0.4f); // at threshold: fires
            player.SetSize(0.3f); // still below: must not fire again
            player.SetSize(0.1f);

            Assert.AreEqual(1, fireCount);
        }

        [Test]
        public void SetSize_AboveCritical_NeverFiresEvent()
        {
            var player = CreatePlayer();
            player.Initialize(3f, BuildBalance());

            int fireCount = 0;
            player.OnCriticalSizeReached += () => fireCount++;

            player.SetSize(2f);
            player.SetSize(1f);
            player.SetSize(0.5f);

            Assert.AreEqual(0, fireCount);
        }

        [Test]
        public void SetSize_NeverGoesNegative()
        {
            var player = CreatePlayer();
            player.Initialize(3f, BuildBalance());

            player.SetSize(-5f);

            Assert.AreEqual(0f, player.CurrentSize, 0.0001f);
        }
    }
}
