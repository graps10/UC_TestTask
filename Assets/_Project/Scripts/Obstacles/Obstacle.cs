using DG.Tweening;
using UnityEngine;

namespace Game.Obstacles
{
    [RequireComponent(typeof(CapsuleCollider))]
    public class Obstacle : MonoBehaviour
    {
        [Tooltip("Visual capsule child (hand-authored in the Obstacle prefab).")]
        [SerializeField] private Transform visual;

        [Tooltip("Scale-down animation duration when destroyed, in seconds.")]
        [SerializeField] private float destroyDuration = 0.2f;

        public int Index { get; private set; }
        public bool IsAlive { get; private set; } = true;

        private CapsuleCollider _collider;
        private bool _componentsReady;

        private void Awake()
        {
            EnsureComponentsReady();
        }

        public void Initialize(int index)
        {
            EnsureComponentsReady();
            Index = index;
        }
        
        public void ConfigureRadius(float radius)
        {
            EnsureComponentsReady();
            _collider.radius = radius;

            Vector3 scale = visual.localScale;
            visual.localScale = new Vector3(radius * 2f, scale.y, radius * 2f);
        }

        public void Kill(float delay)
        {
            if (!IsAlive)
                return;
            IsAlive = false;
            _collider.enabled = false;

            transform
                .DOScale(0f, destroyDuration)
                .SetDelay(delay)
                .SetEase(Ease.InBack)
                .OnComplete(() => Destroy(gameObject));
        }
        
        private void EnsureComponentsReady()
        {
            if (_componentsReady)
                return;
            _componentsReady = true;

            _collider = GetComponent<CapsuleCollider>();
        }
        
        public void SetVisualForTest(Transform visualRef)
        {
            visual = visualRef;
        }
    }
}
