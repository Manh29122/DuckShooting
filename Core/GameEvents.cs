using System;
using UnityEngine;

namespace DuckShooting
{
    /// <summary>
    /// Hub sự kiện toàn cục (event-driven). Các hệ thống đăng ký ở đây thay vì
    /// tham chiếu trực tiếp lẫn nhau, giúp giảm phụ thuộc cứng.
    /// Nhớ HỦY đăng ký trong OnDisable để tránh rò rỉ.
    /// </summary>
    public static class GameEvents
    {
        // --- Vòng chơi ---
        public static event Action OnGameStart;
        public static event Action<int> OnGameOver;        // điểm cuối cùng
        public static event Action<GameState> OnStateChanged;

        // --- Điểm & thời gian ---
        public static event Action<int, Vector3> OnTargetHit;  // điểm cộng, vị trí world (cho FX +10)
        public static event Action<int> OnScoreChanged;        // tổng điểm hiện tại
        public static event Action<float> OnTimeChanged;       // số giây còn lại

        // --- Bắn & Combo ---
        public static event Action OnShotFired;                // mỗi phát bắn (trúng hay trượt) -> FX khói/âm thanh
        public static event Action OnShotMissed;               // bắn ra nhưng không trúng mục tiêu
        public static event Action<int, Vector3> OnComboHit;   // (combo hiện tại, vị trí trúng) -> popup "xN"
        public static event Action OnComboReset;               // chuỗi combo bị ngắt
        public static event Action<Vector3> OnForbiddenHit;    // bắn trúng mục tiêu CẤM
        public static event Action<int, int> OnStrikesChanged; // (số lần trúng cấm, số lần tối đa)

        public static void RaiseGameStart() => OnGameStart?.Invoke();
        public static void RaiseGameOver(int finalScore) => OnGameOver?.Invoke(finalScore);
        public static void RaiseStateChanged(GameState state) => OnStateChanged?.Invoke(state);

        public static void RaiseTargetHit(int points, Vector3 worldPos) => OnTargetHit?.Invoke(points, worldPos);
        public static void RaiseScoreChanged(int total) => OnScoreChanged?.Invoke(total);
        public static void RaiseTimeChanged(float secondsLeft) => OnTimeChanged?.Invoke(secondsLeft);

        public static void RaiseShotFired() => OnShotFired?.Invoke();
        public static void RaiseShotMissed() => OnShotMissed?.Invoke();
        public static void RaiseComboHit(int combo, Vector3 worldPos) => OnComboHit?.Invoke(combo, worldPos);
        public static void RaiseComboReset() => OnComboReset?.Invoke();
        public static void RaiseForbiddenHit(Vector3 worldPos) => OnForbiddenHit?.Invoke(worldPos);
        public static void RaiseStrikesChanged(int used, int max) => OnStrikesChanged?.Invoke(used, max);
    }
}
