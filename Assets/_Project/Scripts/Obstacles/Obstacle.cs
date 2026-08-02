using DG.Tweening;
using UnityEngine;

namespace Game.Obstacles
{
    [RequireComponent(typeof(Collider))]
    public class Obstacle : MonoBehaviour
    {
        [Tooltip("Scale-down animation duration when destroyed, in seconds.")]
        [SerializeField] private float destroyDuration = 0.2f;

        public int Index { get; private set; }
        public bool IsAlive { get; private set; } = true;

        private Collider _collider;

        public void Initialize(int index)
        {
            Index = index;
            _collider = GetComponent<Collider>();
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
    }
}
