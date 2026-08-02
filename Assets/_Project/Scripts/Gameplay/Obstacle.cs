using DG.Tweening;
using UnityEngine;

namespace Game.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class Obstacle : MonoBehaviour
    {
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
                .DOScale(0f, 0.2f)
                .SetDelay(delay)
                .SetEase(Ease.InBack)
                .OnComplete(() => Destroy(gameObject));
        }
    }
}
