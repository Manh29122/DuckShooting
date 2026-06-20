using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DuckShooting
{
    /// <summary>
    /// Popup combo "xN" ghép từ các sprite trong spritesheet HUD (Kenney): 1 sprite "x"
    /// (tùy chọn) + các sprite chữ số 0–9. Tự dàn hàng ngang, bay lên & mờ dần, rồi
    /// trả về pool. Là world-space (dùng SpriteRenderer).
    /// </summary>
    public class ComboPopup : MonoBehaviour
    {
        [Header("Sprite glyph")]
        [Tooltip("10 sprite chữ số theo thứ tự: phần tử 0 = số '0', ... phần tử 9 = số '9'.")]
        [SerializeField] private Sprite[] digitSprites = new Sprite[10];
        [Tooltip("Sprite ký hiệu 'x' đứng trước số (tùy chọn — để trống thì chỉ hiện số).")]
        [SerializeField] private Sprite multiplierSprite;

        [Header("Hiển thị")]
        [SerializeField] private string sortingLayer = "Crosshair";
        [SerializeField] private int sortingOrder = 100;
        [Tooltip("Khoảng cách giữa các glyph (world units).")]
        [SerializeField] private float spacing = 0.05f;
        [Tooltip("Tỉ lệ phóng to mỗi glyph.")]
        [SerializeField] private float glyphScale = 1f;

        [Header("Hoạt ảnh")]
        [SerializeField] private float riseDistance = 1.2f;
        [SerializeField] private float lifetime = 0.8f;

        private readonly List<SpriteRenderer> _renderers = new List<SpriteRenderer>();
        private readonly List<Sprite> _buffer = new List<Sprite>();
        private Action<ComboPopup> _onRelease;

        public void Play(int combo, Vector3 worldPos, Action<ComboPopup> onRelease)
        {
            _onRelease = onRelease;
            transform.position = worldPos;
            BuildGlyphs(combo);
            StopAllCoroutines();
            StartCoroutine(Animate());
        }

        private void BuildGlyphs(int combo)
        {
            _buffer.Clear();
            if (multiplierSprite != null) _buffer.Add(multiplierSprite);

            foreach (char c in combo.ToString())
            {
                int d = c - '0';
                if (d >= 0 && d < 10 && digitSprites[d] != null)
                    _buffer.Add(digitSprites[d]);
            }

            // đảm bảo đủ SpriteRenderer con
            while (_renderers.Count < _buffer.Count)
                _renderers.Add(CreateRenderer());

            // tính tổng bề rộng để căn giữa
            float totalWidth = 0f;
            for (int i = 0; i < _buffer.Count; i++)
            {
                totalWidth += GlyphWidth(_buffer[i]);
                if (i > 0) totalWidth += spacing;
            }

            float cursor = -totalWidth * 0.5f;
            for (int i = 0; i < _renderers.Count; i++)
            {
                var sr = _renderers[i];
                if (i < _buffer.Count)
                {
                    float w = GlyphWidth(_buffer[i]);
                    sr.gameObject.SetActive(true);
                    sr.sprite = _buffer[i];
                    sr.color = Color.white;
                    sr.transform.localScale = Vector3.one * glyphScale;
                    sr.transform.localPosition = new Vector3(cursor + w * 0.5f, 0f, 0f);
                    cursor += w + spacing;
                }
                else
                {
                    sr.gameObject.SetActive(false);
                }
            }
        }

        private float GlyphWidth(Sprite s) => s.bounds.size.x * glyphScale;

        private SpriteRenderer CreateRenderer()
        {
            var go = new GameObject("glyph");
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = sortingLayer;
            sr.sortingOrder = sortingOrder;
            return sr;
        }

        private IEnumerator Animate()
        {
            Vector3 start = transform.position;
            Vector3 end = start + Vector3.up * riseDistance;

            float t = 0f;
            while (t < lifetime)
            {
                t += Time.deltaTime;
                float k = t / lifetime;
                transform.position = Vector3.Lerp(start, end, k);

                float a = 1f - k;
                for (int i = 0; i < _renderers.Count; i++)
                {
                    var sr = _renderers[i];
                    if (sr.gameObject.activeSelf)
                    {
                        var col = sr.color; col.a = a; sr.color = col;
                    }
                }
                yield return null;
            }

            _onRelease?.Invoke(this);
        }
    }
}
