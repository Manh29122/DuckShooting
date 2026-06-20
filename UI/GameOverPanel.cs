using AdMobKit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DuckShooting
{
    /// <summary>
    /// Panel game over: ẩn lúc chơi, hiện khi thua. Có 2 nút:
    ///  • Play Again: hiện quảng cáo trung gian (interstitial); hết quảng cáo thì chơi lại.
    ///  • Quit: thoát game.
    /// </summary>
    public class GameOverPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text finalScoreText;
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button quitButton;

        [Tooltip("Hiện quảng cáo trung gian khi bấm Play Again (tắt để test không quảng cáo).")]
        [SerializeField] private bool showAdOnPlayAgain = true;

        private void Awake()
        {
            // Cảnh báo nếu script bị đặt nhầm lên chính panel (sẽ tự tắt theo -> không nghe được sự kiện).
            if (panelRoot == gameObject)
                Debug.LogError("[GameOverPanel] Script đang nằm TRÊN chính Panel Root! Hãy đặt script lên " +
                               "object LUÔN BẬT (vd Canvas) và để Panel Root là object con bị ẩn/hiện.", this);

            if (panelRoot != null) panelRoot.SetActive(false);
            if (playAgainButton != null) playAgainButton.onClick.AddListener(OnPlayAgain);
            if (quitButton != null) quitButton.onClick.AddListener(OnQuit);
        }

        private void OnEnable()
        {
            GameEvents.OnGameOver += Show;
            GameEvents.OnGameStart += Hide;
        }

        private void OnDisable()
        {
            GameEvents.OnGameOver -= Show;
            GameEvents.OnGameStart -= Hide;
        }

        private void Show(int finalScore)
        {
            if (panelRoot != null) panelRoot.SetActive(true);
            if (finalScoreText != null) finalScoreText.text = finalScore.ToString();
            if (playAgainButton != null) playAgainButton.interactable = true;
        }

        private void Hide()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private void OnPlayAgain()
        {
            // chặn bấm nhiều lần trong lúc chờ quảng cáo
            if (playAgainButton != null) playAgainButton.interactable = false;

            var ads = AdMobManager.Instance;
            if (showAdOnPlayAgain && ads != null && ads.IsInterstitialReady)
            {
                // Đăng ký 1 lần: hết quảng cáo -> chơi lại. (Kit tự nạp QC mới sau khi đóng.)
                System.Action handler = null;
                handler = () =>
                {
                    ads.OnInterstitialClosed -= handler;
                    RestartGame();
                };
                ads.OnInterstitialClosed += handler;
                ads.ShowInterstitial();
            }
            else
            {
                RestartGame();   // chưa có quảng cáo sẵn -> chơi lại luôn
            }
        }

        private void RestartGame()
        {
            // Restart -> StartGame -> OnGameStart -> Hide() (panel tự ẩn)
            if (GameManager.Instance != null) GameManager.Instance.Restart();
        }

        private void OnQuit()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
