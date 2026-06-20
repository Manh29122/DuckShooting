using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DuckShooting
{
    /// <summary>
    /// Màn intro trước khi chơi:
    ///  1. Hiện chữ "Tap To Play"; chưa cho bắn/spawn.
    ///  2. Player chạm -> ẩn chữ, lần lượt hiện các hình (Ready, Go) với hiệu ứng "đập"
    ///     (hiện to rồi thu nhỏ về kích cỡ thường ngay), mỗi lần kèm âm thanh.
    ///  3. Xong -> gọi <see cref="GameManager.StartGame"/> để vào màn chơi (bắn + spawn).
    ///
    /// Tự tắt auto-start của GameManager trong Awake nên không cần chỉnh tay.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class GameIntro : MonoBehaviour
    {
        [Header("UI")]
        [Tooltip("Object chữ 'Tap To Play' (hiện lúc đầu, ẩn khi chạm).")]
        [SerializeField] private GameObject tapToPlay;
        [Tooltip("Các hình hiện lần lượt khi bắt đầu (gán: Ready, rồi Go). Để ẩn sẵn cũng được.")]
        [SerializeField] private RectTransform[] sequenceItems;

        [Header("Hiệu ứng 'đập' (punch)")]
        [Tooltip("Kích cỡ lúc vừa hiện (to), rồi thu nhỏ về 1.")]
        [SerializeField] private float punchStartScale = 2f;
        [Tooltip("Thời gian thu nhỏ từ to về kích cỡ thường (giây).")]
        [SerializeField] private float punchDuration = 0.15f;
        [Tooltip("Giữ hình trên màn hình sau khi thu nhỏ (giây) rồi mới qua hình kế.")]
        [SerializeField] private float holdTime = 0.4f;

        [Header("Âm thanh")]
        [Tooltip("Âm phát mỗi khi một hình 'đập' vào màn hình.")]
        [SerializeField] private AudioClip punchClip;
        [Range(0f, 1f)] [SerializeField] private float volume = 1f;

        private AudioSource _audio;
        private bool _started;

        private void Awake()
        {
            _audio = GetComponent<AudioSource>();
            _audio.playOnAwake = false;
            _audio.spatialBlend = 0f;

            // Chặn auto-start để game chưa chạy cho tới khi intro xong.
            var gm = FindAnyObjectByType<GameManager>();
            if (gm != null) gm.DisableAutoStart();

            if (sequenceItems != null)
                foreach (var it in sequenceItems)
                    if (it != null) it.gameObject.SetActive(false);

            if (tapToPlay != null) tapToPlay.SetActive(true);
        }

        private void Update()
        {
            if (_started) return;

            var pointer = Pointer.current;
            if (pointer != null && pointer.press.wasPressedThisFrame)
            {
                _started = true;
                if (tapToPlay != null) tapToPlay.SetActive(false);
                StartCoroutine(PlaySequence());
            }
        }

        private IEnumerator PlaySequence()
        {
            if (sequenceItems != null)
            {
                foreach (var item in sequenceItems)
                {
                    if (item == null) continue;
                    yield return Punch(item);
                }
            }

            // Vào màn chơi: bật bắn + spawn target.
            if (GameManager.Instance != null) GameManager.Instance.StartGame();
            gameObject.SetActive(false);
        }

        private IEnumerator Punch(RectTransform item)
        {
            item.gameObject.SetActive(true);
            item.localScale = Vector3.one * punchStartScale;   // hiện TO
            PlaySound();

            float t = 0f;
            while (t < punchDuration)
            {
                t += Time.deltaTime;
                float k = punchDuration > 0f ? Mathf.Clamp01(t / punchDuration) : 1f;
                float s = Mathf.Lerp(punchStartScale, 1f, k);  // thu nhỏ về 1
                item.localScale = Vector3.one * s;
                yield return null;
            }
            item.localScale = Vector3.one;

            yield return new WaitForSeconds(holdTime);
            item.gameObject.SetActive(false);
        }

        private void PlaySound()
        {
            if (punchClip != null) _audio.PlayOneShot(punchClip, volume);
        }
    }
}
