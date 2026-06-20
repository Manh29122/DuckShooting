using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DuckShooting
{
    /// <summary>
    /// Hiển thị số MẠNG bằng các icon (UI). Lúc bắt đầu, các icon được spawn và "mọc"
    /// (scale 0 -> kích cỡ đích). Khi bắn trúng mục tiêu CẤM (mất 1 mạng), icon cuối cùng
    /// sẽ RUNG NHẸ rồi biến mất. Nghe sự kiện <see cref="GameEvents.OnStrikesChanged"/>.
    ///
    /// Cho phép đặt KHOẢNG CÁCH RIÊNG cho từng cặp icon và KÍCH CỠ RIÊNG cho từng icon
    /// (vd icon trước to hơn icon sau).
    /// </summary>
    public class StrikesView : MonoBehaviour
    {
        [Header("Icon")]
        [Tooltip("Prefab icon mạng (một UI Image có RectTransform).")]
        [SerializeField] private RectTransform iconPrefab;
        [Tooltip("Nơi chứa icon. Để trống = dùng chính object này.")]
        [SerializeField] private RectTransform container;

        [Header("Khoảng cách (pixel)")]
        [Tooltip("Khoảng cách MẶC ĐỊNH giữa các icon — dùng khi không khai báo riêng ở 'Spacings'.")]
        [SerializeField] private float spacing = 90f;
        [Tooltip("Khoảng cách RIÊNG giữa từng cặp icon liền nhau. " +
                 "Phần tử 0 = giữa icon 1 và 2, phần tử 1 = giữa icon 2 và 3, ... Thiếu thì dùng 'Spacing'.")]
        [SerializeField] private float[] spacings;

        [Header("Kích cỡ")]
        [Tooltip("Kích cỡ MẶC ĐỊNH của icon — dùng khi không khai báo riêng ở 'Icon Scales'.")]
        [SerializeField] private float fullScale = 1f;
        [Tooltip("Kích cỡ RIÊNG của từng icon. Phần tử 0 = icon 1, 1 = icon 2, ... " +
                 "Vd 1.3, 1.0, 0.7 để icon trước to hơn icon sau. Thiếu thì dùng 'Full Scale'.")]
        [SerializeField] private float[] iconScales;

        [Header("Hiệu ứng mọc (spawn grow)")]
        [Tooltip("Thời gian icon phóng to từ 0 -> đích (giây).")]
        [SerializeField] private float growDuration = 0.3f;
        [Tooltip("Độ lệch thời điểm mọc giữa các icon (giây) cho hiệu ứng lần lượt.")]
        [SerializeField] private float growStagger = 0.08f;

        [Header("Hiệu ứng mất mạng (shake)")]
        [Tooltip("Thời gian rung icon trước khi mất (giây).")]
        [SerializeField] private float shakeDuration = 0.3f;
        [Tooltip("Biên độ rung (pixel).")]
        [SerializeField] private float shakeMagnitude = 8f;

        private readonly List<RectTransform> _icons = new List<RectTransform>();
        private readonly List<Vector2> _basePos = new List<Vector2>();
        private readonly List<float> _targetScale = new List<float>();
        private int _maxLives = -1;
        private int _currentLives;

        private void Awake()
        {
            if (container == null) container = transform as RectTransform;
        }

        private void OnEnable() => GameEvents.OnStrikesChanged += OnStrikesChanged;
        private void OnDisable() => GameEvents.OnStrikesChanged -= OnStrikesChanged;

        private void OnStrikesChanged(int used, int max)
        {
            int lives = Mathf.Max(0, max - used);

            if (max != _maxLives || _icons.Count != max)
            {
                Build(max);
                _currentLives = max;
            }

            // Mất mạng: bỏ icon từ cuối, kèm rung.
            while (_currentLives > lives)
            {
                _currentLives--;
                if (_currentLives >= 0 && _currentLives < _icons.Count)
                    StartCoroutine(LoseIcon(_icons[_currentLives]));
            }

            // Hồi mạng (nếu có): bật lại icon + mọc.
            while (_currentLives < lives)
            {
                if (_currentLives >= 0 && _currentLives < _icons.Count)
                {
                    var ic = _icons[_currentLives];
                    ic.gameObject.SetActive(true);
                    ic.anchoredPosition = _basePos[_currentLives];
                    StartCoroutine(Grow(ic, 0f, _targetScale[_currentLives]));
                }
                _currentLives++;
            }
        }

        private void Build(int max)
        {
            StopAllCoroutines();
            foreach (var ic in _icons)
                if (ic != null) Destroy(ic.gameObject);
            _icons.Clear();
            _basePos.Clear();
            _targetScale.Clear();

            _maxLives = max;
            if (iconPrefab == null || container == null)
            {
                Debug.LogWarning("[StrikesView] Thiếu Icon Prefab hoặc Container.", this);
                return;
            }

            float x = 0f;
            for (int i = 0; i < max; i++)
            {
                if (i > 0) x += GapBefore(i);   // cộng dồn khoảng cách riêng từng cặp

                var ic = Instantiate(iconPrefab, container);
                ic.gameObject.SetActive(true);
                Vector2 pos = new Vector2(x, 0f);
                ic.anchoredPosition = pos;

                float target = ScaleFor(i);
                _icons.Add(ic);
                _basePos.Add(pos);
                _targetScale.Add(target);

                StartCoroutine(Grow(ic, i * growStagger, target));   // mọc lần lượt
            }
        }

        /// <summary>Khoảng cách giữa icon (i-1) và icon i.</summary>
        private float GapBefore(int i)
        {
            int gap = i - 1; // chỉ số cặp
            if (spacings != null && spacings.Length > 0)
                return gap < spacings.Length ? spacings[gap] : spacings[spacings.Length - 1];
            return spacing;
        }

        /// <summary>Kích cỡ đích của icon thứ i.</summary>
        private float ScaleFor(int i)
        {
            if (iconScales != null && iconScales.Length > 0)
                return i < iconScales.Length ? iconScales[i] : iconScales[iconScales.Length - 1];
            return fullScale;
        }

        private IEnumerator Grow(RectTransform ic, float delay, float targetScale)
        {
            ic.localScale = Vector3.zero;
            if (delay > 0f) yield return new WaitForSeconds(delay);

            float t = 0f;
            while (t < growDuration)
            {
                t += Time.deltaTime;
                float k = growDuration > 0f ? Mathf.Clamp01(t / growDuration) : 1f;
                float s = Mathf.SmoothStep(0f, 1f, k);
                ic.localScale = Vector3.one * (targetScale * s);
                yield return null;
            }
            ic.localScale = Vector3.one * targetScale;
        }

        private IEnumerator LoseIcon(RectTransform ic)
        {
            if (ic == null) yield break;

            int idx = _icons.IndexOf(ic);
            Vector2 basePos = (idx >= 0 && idx < _basePos.Count) ? _basePos[idx] : ic.anchoredPosition;

            float t = 0f;
            while (t < shakeDuration)
            {
                t += Time.deltaTime;
                float r = shakeDuration > 0f ? 1f - (t / shakeDuration) : 0f;
                Vector2 offset = Random.insideUnitCircle * (shakeMagnitude * r);
                ic.anchoredPosition = basePos + offset;
                yield return null;
            }

            ic.anchoredPosition = basePos;
            ic.gameObject.SetActive(false);   // mất mạng
        }
    }
}
