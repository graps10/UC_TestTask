using Game.Core;
using Game.Player;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class PathTrailTests
    {
        private GameObject _root;

        [TearDown]
        public void TearDown()
        {
            if (_root != null)
                Object.DestroyImmediate(_root);
        }

        private (PathTrail trail, PlayerBall player) BuildScene(float startZ)
        {
            _root = new GameObject("PathTrailTestRoot");

            var playerGo = new GameObject("Player");
            playerGo.transform.SetParent(_root.transform);
            var player = playerGo.AddComponent<PlayerBall>();
            var playerVisual = new GameObject("Visual").transform;
            playerVisual.SetParent(playerGo.transform);
            player.SetVisualForTest(playerVisual);
            player.Initialize(2f, new BalanceSettings { CriticalMinSize = 0.4f });

            var trailGo = new GameObject("PathTrail");
            trailGo.transform.SetParent(_root.transform);
            var trail = trailGo.AddComponent<PathTrail>();
            trail.Initialize(startZ, player);

            return (trail, player);
        }

        private static Transform GetVisual(PathTrail trail)
        {
            return trail.transform.Find("Trail");
        }

        [Test]
        public void Refresh_WidthTracksPlayerCurrentSize()
        {
            var (trail, player) = BuildScene(0f);

            player.SetSize(1.5f);
            trail.Refresh();

            Assert.AreEqual(1.5f, GetVisual(trail).localScale.x, 0.0001f);
        }

        [Test]
        public void Refresh_LengthTracksTraveledDistance()
        {
            var (trail, player) = BuildScene(0f);

            player.transform.position = new Vector3(0f, 0f, 6f);
            trail.Refresh();

            Assert.AreEqual(6f, GetVisual(trail).localScale.z, 0.0001f);
        }

        [Test]
        public void Refresh_LengthNeverGoesNegativeBeforeStartZ()
        {
            var (trail, player) = BuildScene(startZ: 4f);

            player.transform.position = new Vector3(0f, 0f, 1f); // hasn't reached startZ yet
            trail.Refresh();

            Assert.AreEqual(0f, GetVisual(trail).localScale.z, 0.0001f);
        }
    }
}
