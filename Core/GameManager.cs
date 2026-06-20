using UnityEngine;

namespace DuckShooting
{
    /// <summary>
    /// Bộ điều phối trung tâm: giữ state máy trạng thái (Ready -> Playing -> GameOver).
    /// Hỗ trợ chế độ VÔ HẠN (không giới hạn thời gian) — chỉ thua khi bắn trúng mục tiêu
    /// cấm đủ <see cref="GameConfig.forbiddenHitsToEnd"/> lần.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Cấu hình")]
        [SerializeField] private GameConfig config;
        [Tooltip("Tự bắt đầu vòng chơi ngay khi vào scene.")]
        [SerializeField] private bool autoStart = true;

        public GameConfig Config => config;
        public GameState State { get; private set; } = GameState.Ready;

        /// <summary>Thời gian còn lại (chỉ dùng khi KHÔNG vô hạn).</summary>
        public float TimeLeft { get; private set; }
        /// <summary>Thời gian đã chơi (giây).</summary>
        public float Elapsed { get; private set; }
        /// <summary>Số lần đã bắn trúng mục tiêu cấm.</summary>
        public int ForbiddenHits { get; private set; }

        /// <summary>Tiến độ độ khó 0..1 (spawner dùng để tăng nhịp).</summary>
        public float DifficultyProgress01
        {
            get
            {
                if (config == null) return 0f;
                if (config.infinite)
                    return config.difficultyRampSeconds > 0f
                        ? Mathf.Clamp01(Elapsed / config.difficultyRampSeconds)
                        : 1f;
                return config.roundDuration > 0f
                    ? Mathf.Clamp01(1f - TimeLeft / config.roundDuration)
                    : 0f;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            SetState(GameState.Ready);
            if (autoStart) StartGame();
        }

        private void OnEnable()
        {
            GameEvents.OnForbiddenHit += HandleForbiddenHit;
        }

        private void OnDisable()
        {
            GameEvents.OnForbiddenHit -= HandleForbiddenHit;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>Bắt đầu / chơi lại một vòng mới.</summary>
        public void StartGame()
        {
            if (config == null)
            {
                Debug.LogError("[GameManager] Chưa gán GameConfig!", this);
                return;
            }

            Elapsed = 0f;
            ForbiddenHits = 0;
            TimeLeft = config.roundDuration;

            SetState(GameState.Playing);
            GameEvents.RaiseGameStart();
            GameEvents.RaiseStrikesChanged(0, config.forbiddenHitsToEnd);
            GameEvents.RaiseTimeChanged(config.infinite ? 0f : TimeLeft);
        }

        private void Update()
        {
            if (State != GameState.Playing) return;

            if (config.infinite)
            {
                // Vô hạn: chỉ đếm thời gian sống (đếm lên), không tự kết thúc.
                Elapsed += Time.deltaTime;
                GameEvents.RaiseTimeChanged(Elapsed);
                return;
            }

            // Chế độ có giới hạn: đếm ngược.
            TimeLeft -= Time.deltaTime;
            if (TimeLeft <= 0f)
            {
                TimeLeft = 0f;
                GameEvents.RaiseTimeChanged(TimeLeft);
                EndGame();
                return;
            }
            GameEvents.RaiseTimeChanged(TimeLeft);
        }

        // Bắn trúng mục tiêu cấm -> tăng strike; đủ số lần thì thua.
        private void HandleForbiddenHit(Vector3 _)
        {
            if (State != GameState.Playing) return;

            ForbiddenHits++;
            int max = config != null ? config.forbiddenHitsToEnd : 3;
            GameEvents.RaiseStrikesChanged(ForbiddenHits, max);

            if (ForbiddenHits >= max)
                EndGame();
        }

        private void EndGame()
        {
            SetState(GameState.GameOver);
            GameEvents.RaiseGameOver(GetScore());
        }

        /// <summary>Gọi từ nút Restart trên GameOverPanel.</summary>
        public void Restart() => StartGame();

        /// <summary>GameIntro gọi (trong Awake) để chặn auto-start, chạy màn intro trước.</summary>
        public void DisableAutoStart() => autoStart = false;

        private void SetState(GameState next)
        {
            if (State == next) return;
            State = next;
            GameEvents.RaiseStateChanged(next);
        }

        private int GetScore()
        {
            var sm = FindAnyObjectByType<ScoreManager>();
            return sm != null ? sm.Score : 0;
        }
    }
}
