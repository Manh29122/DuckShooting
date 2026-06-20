using UnityEngine;

namespace DuckShooting
{
    /// <summary>
    /// Lắng nghe <see cref="GameEvents.OnComboHit"/> và sinh popup "xN" tại vị trí trúng,
    /// dùng pool. Chỉ hiện khi combo >= ngưỡng.
    /// </summary>
    public class ComboPopupSpawner : MonoBehaviour
    {
        [SerializeField] private ComboPopup prefab;
        [SerializeField] private int prewarm = 4;
        [Tooltip("Chỉ hiện popup khi combo đạt mức này trở lên (1 = hiện ngay từ x1).")]
        [SerializeField] private int minCombo = 1;
        [Tooltip("Dịch popup so với vị trí trúng (để không đè lên popup +điểm).")]
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0.6f, 0f);

        private ComponentPool<ComboPopup> _pool;

        private void Awake()
        {
            if (prefab != null)
                _pool = new ComponentPool<ComboPopup>(prefab, transform, prewarm);
        }

        private void OnEnable() => GameEvents.OnComboHit += Show;
        private void OnDisable() => GameEvents.OnComboHit -= Show;

        private void Show(int combo, Vector3 worldPos)
        {
            if (_pool == null || combo < minCombo) return;
            var popup = _pool.Get();
            popup.transform.SetParent(transform, false);
            popup.Play(combo, worldPos + worldOffset, _pool.Release);
        }
    }
}
