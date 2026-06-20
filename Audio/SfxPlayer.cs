using UnityEngine;

namespace DuckShooting
{
    /// <summary>
    /// Phát hiệu ứng âm thanh (SFX) theo các sự kiện game. Bắt buộc nhất là clip "bắn trúng".
    /// Gắn vào 1 GameObject (vd "Audio" hoặc "Managers"); script tự thêm AudioSource.
    /// Kéo các AudioClip vào Inspector — clip nào để trống thì bỏ qua.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class SfxPlayer : MonoBehaviour
    {
        [Header("Clip")]
        [Tooltip("Âm khi BẮN TRÚNG mục tiêu.")]
        [SerializeField] private AudioClip hitClip;
        [Tooltip("Âm mỗi phát bắn (tiếng súng) — tùy chọn.")]
        [SerializeField] private AudioClip shootClip;
        [Tooltip("Âm khi bắn trượt — tùy chọn.")]
        [SerializeField] private AudioClip missClip;
        [Tooltip("Âm khi bắn trúng mục tiêu CẤM — tùy chọn.")]
        [SerializeField] private AudioClip forbiddenClip;
        [Tooltip("Âm khi hết giờ / game over — tùy chọn.")]
        [SerializeField] private AudioClip gameOverClip;

        [Header("Tinh chỉnh")]
        [Range(0f, 1f)]
        [SerializeField] private float volume = 1f;
        [Tooltip("Biến thiên cao độ ngẫu nhiên (+/-) cho âm bắn trúng, tránh nghe nhàm.")]
        [Range(0f, 0.5f)]
        [SerializeField] private float hitPitchVariance = 0.05f;

        private AudioSource _src;

        private void Awake()
        {
            _src = GetComponent<AudioSource>();
            _src.playOnAwake = false;
            _src.spatialBlend = 0f; // âm 2D (nghe đều, không theo vị trí)
        }

        private void OnEnable()
        {
            GameEvents.OnTargetHit += OnHit;
            GameEvents.OnShotFired += OnShot;
            GameEvents.OnShotMissed += OnMiss;
            GameEvents.OnForbiddenHit += OnForbidden;
            GameEvents.OnGameOver += OnGameOver;
        }

        private void OnDisable()
        {
            GameEvents.OnTargetHit -= OnHit;
            GameEvents.OnShotFired -= OnShot;
            GameEvents.OnShotMissed -= OnMiss;
            GameEvents.OnForbiddenHit -= OnForbidden;
            GameEvents.OnGameOver -= OnGameOver;
        }

        private void OnHit(int points, Vector3 worldPos)
        {
            if (hitClip == null) return;
            _src.pitch = 1f + Random.Range(-hitPitchVariance, hitPitchVariance);
            _src.PlayOneShot(hitClip, volume);
        }

        private void OnShot() => Play(shootClip);
        private void OnMiss() => Play(missClip);
        private void OnForbidden(Vector3 worldPos) => Play(forbiddenClip);
        private void OnGameOver(int finalScore) => Play(gameOverClip);

        private void Play(AudioClip clip)
        {
            if (clip == null) return;
            _src.pitch = 1f;
            _src.PlayOneShot(clip, volume);
        }
    }
}
