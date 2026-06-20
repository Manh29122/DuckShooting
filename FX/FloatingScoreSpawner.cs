using UnityEngine;

namespace DuckShooting
{
    /// <summary>
    /// Lắng nghe OnTargetHit và sinh popup "+điểm" tại vị trí trúng, dùng pool.
    /// </summary>
    public class FloatingScoreSpawner : MonoBehaviour
    {
        [SerializeField] private FloatingScore prefab;
        [SerializeField] private int prewarm = 6;

        private ComponentPool<FloatingScore> _pool;

        private void Awake()
        {
            if (prefab != null)
                _pool = new ComponentPool<FloatingScore>(prefab, transform, prewarm);
        }

        private void OnEnable() => GameEvents.OnTargetHit += Spawn;
        private void OnDisable() => GameEvents.OnTargetHit -= Spawn;

        private void Spawn(int points, Vector3 worldPos)
        {
            if (_pool == null) return;
            var fx = _pool.Get();
            fx.transform.SetParent(transform, false);
            fx.Play(points, worldPos, _pool.Release);
        }
    }
}
