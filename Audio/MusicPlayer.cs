using UnityEngine;

namespace DuckShooting
{
    /// <summary>
    /// Phát NHẠC NỀN (lặp) trong game. Khi THUA (game over): dừng nhạc nền và phát nhạc thua.
    /// Dùng AudioSource RIÊNG của object này nên KHÔNG ảnh hưởng tới SFX (SfxPlayer dùng source khác).
    /// Gắn vào 1 GameObject riêng (vd "Music"); script tự thêm AudioSource.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class MusicPlayer : MonoBehaviour
    {
        [Header("Clip")]
        [Tooltip("Nhạc nền tổng, phát lặp suốt khi chơi.")]
        [SerializeField] private AudioClip backgroundMusic;
        [Tooltip("Nhạc phát khi THUA (game over).")]
        [SerializeField] private AudioClip loseMusic;

        [Header("Âm lượng")]
        [Range(0f, 1f)] [SerializeField] private float bgmVolume = 0.6f;
        [Range(0f, 1f)] [SerializeField] private float loseVolume = 0.8f;
        [Tooltip("Lặp nhạc thua hay chỉ phát 1 lần.")]
        [SerializeField] private bool loopLoseMusic = false;

        private AudioSource _src;

        private void Awake()
        {
            _src = GetComponent<AudioSource>();
            _src.playOnAwake = false;
            _src.spatialBlend = 0f; // 2D
        }

        private void OnEnable()
        {
            GameEvents.OnGameStart += PlayBackground;
            GameEvents.OnGameOver += PlayLose;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStart -= PlayBackground;
            GameEvents.OnGameOver -= PlayLose;
        }

        private void Start()
        {
            // Phòng khi OnGameStart đã phát trước lúc script này kịp đăng ký.
            PlayBackground();
        }

        private void PlayBackground()
        {
            if (backgroundMusic == null) return;
            if (_src.clip == backgroundMusic && _src.isPlaying) return; // đang chạy rồi

            _src.clip = backgroundMusic;
            _src.loop = true;
            _src.volume = bgmVolume;
            _src.Play();
        }

        private void PlayLose(int finalScore)
        {
            _src.Stop();                 // dừng nhạc nền
            if (loseMusic == null) return;

            _src.clip = loseMusic;
            _src.loop = loopLoseMusic;
            _src.volume = loseVolume;
            _src.Play();                 // phát nhạc thua
        }
    }
}
