using UnityEngine;

namespace DuckShooting
{
    /// <summary>
    /// Theo dõi tổng điểm. Lắng nghe OnTargetHit để cộng điểm và phát OnScoreChanged.
    /// Reset khi game bắt đầu.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public int Score { get; private set; }

        void OnEnable()
        {
            GameEvents.OnGameStart += ResetScore;
            GameEvents.OnTargetHit += AddPoints;
        }

        void OnDisable()
        {
            GameEvents.OnGameStart -= ResetScore;
            GameEvents.OnTargetHit -= AddPoints;
        }

        void ResetScore()
        {
            Score = 0;
            GameEvents.RaiseScoreChanged(Score);
        }

        void AddPoints(int points, Vector3 _)
        {
            Score += points;
            GameEvents.RaiseScoreChanged(Score);
        }
    }
}
