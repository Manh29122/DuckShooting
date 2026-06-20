using UnityEngine;

namespace DuckShooting
{
    /// <summary>
    /// Đếm số lần bắn trúng LIÊN TIẾP (combo). Mỗi lần trúng tăng combo và phát
    /// <see cref="GameEvents.OnComboHit"/> kèm vị trí trúng để hiện popup "xN".
    /// Bắn trượt -> combo về 0.
    /// </summary>
    public class ComboSystem : MonoBehaviour
    {
        public int Combo { get; private set; }

        private void OnEnable()
        {
            GameEvents.OnGameStart += ResetCombo;
            GameEvents.OnTargetHit += OnHit;
            GameEvents.OnShotMissed += OnMiss;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStart -= ResetCombo;
            GameEvents.OnTargetHit -= OnHit;
            GameEvents.OnShotMissed -= OnMiss;
        }

        private void ResetCombo()
        {
            Combo = 0;
            GameEvents.RaiseComboReset();
        }

        private void OnHit(int points, Vector3 worldPos)
        {
            Combo++;
            GameEvents.RaiseComboHit(Combo, worldPos);
        }

        private void OnMiss()
        {
            if (Combo == 0) return;
            Combo = 0;
            GameEvents.RaiseComboReset();
        }
    }
}
