using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace DuckShooting
{
    /// <summary>
    /// Popup "+10" bay lên và mờ dần khi bắn trúng. Lấy từ pool, tự trả về khi xong.
    /// Gắn vào prefab world-space có component TextMeshPro (3D).
    /// </summary>
    public class FloatingScore : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private float riseDistance = 1.0f;
        [SerializeField] private float lifetime = 0.7f;

        private Action<FloatingScore> _onRelease;

        private void Awake()
        {
            if (label == null) label = GetComponentInChildren<TMP_Text>();
        }

        public void Play(int points, Vector3 worldPos, Action<FloatingScore> onRelease)
        {
            _onRelease = onRelease;
            transform.position = worldPos;
            if (label != null) label.text = "+" + points;
            StopAllCoroutines();
            StartCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
            Vector3 start = transform.position;
            Vector3 end = start + Vector3.up * riseDistance;
            Color baseColor = label != null ? label.color : Color.white;

            float t = 0f;
            while (t < lifetime)
            {
                t += Time.deltaTime;
                float k = t / lifetime;
                transform.position = Vector3.Lerp(start, end, k);
                if (label != null)
                {
                    baseColor.a = 1f - k;
                    label.color = baseColor;
                }
                yield return null;
            }

            if (label != null) { baseColor.a = 1f; label.color = baseColor; } // khôi phục alpha cho lần dùng sau
            _onRelease?.Invoke(this);
        }
    }
}
