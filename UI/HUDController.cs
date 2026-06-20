using TMPro;
using UnityEngine;

namespace DuckShooting
{
    /// <summary>
    /// Hiển thị điểm (icon vịt + số, góc trái) và thời gian còn lại.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text timeText;

        private void OnEnable()
        {
            GameEvents.OnScoreChanged += UpdateScore;
            GameEvents.OnTimeChanged += UpdateTime;
        }

        private void OnDisable()
        {
            GameEvents.OnScoreChanged -= UpdateScore;
            GameEvents.OnTimeChanged -= UpdateTime;
        }

        private void UpdateScore(int total)
        {
            if (scoreText != null) scoreText.text = total.ToString();
        }

        private void UpdateTime(float secondsLeft)
        {
            if (timeText != null) timeText.text = Mathf.CeilToInt(secondsLeft).ToString();
        }
    }
}
