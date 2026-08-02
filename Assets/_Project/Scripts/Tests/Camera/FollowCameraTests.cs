using Game.CameraControl;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class FollowCameraTests
    {
        private static readonly Vector3 DefaultOffset = new Vector3(0f, 8f, -8f);

        private GameObject _root;

        [TearDown]
        public void TearDown()
        {
            if (_root != null)
                Object.DestroyImmediate(_root);
        }

        private (FollowCamera cam, Transform target) BuildScene()
        {
            _root = new GameObject("FollowCameraTestRoot");

            var targetGo = new GameObject("Target");
            targetGo.transform.SetParent(_root.transform);

            var camGo = new GameObject("Camera");
            camGo.transform.SetParent(_root.transform);
            var cam = camGo.AddComponent<FollowCamera>();

            return (cam, targetGo.transform);
        }

        [Test]
        public void Initialize_SnapsImmediatelyToTargetPlusOffset()
        {
            var (cam, target) = BuildScene();
            target.position = new Vector3(0f, 0f, 5f);

            cam.Initialize(target);

            Assert.AreEqual(target.position + DefaultOffset, cam.transform.position);
        }

        [Test]
        public void FollowTick_MovesCloserToTarget_WithoutOvershooting()
        {
            var (cam, target) = BuildScene();
            target.position = Vector3.zero;
            cam.Initialize(target);

            target.position = new Vector3(0f, 0f, 10f);
            Vector3 desired = target.position + DefaultOffset;
            float distanceBefore = Vector3.Distance(cam.transform.position, desired);

            cam.FollowTick(0.1f);

            float distanceAfter = Vector3.Distance(cam.transform.position, desired);
            Assert.Less(distanceAfter, distanceBefore);
            Assert.GreaterOrEqual(distanceAfter, 0f);
        }

        [Test]
        public void FollowTick_ConvergesToTargetOverManySteps()
        {
            var (cam, target) = BuildScene();
            target.position = Vector3.zero;
            cam.Initialize(target);

            target.position = new Vector3(0f, 0f, 10f);
            for (int i = 0; i < 200; i++)
            {
                cam.FollowTick(0.05f);
            }

            Vector3 desired = target.position + DefaultOffset;
            Assert.Less(Vector3.Distance(cam.transform.position, desired), 0.01f);
        }
    }
}
