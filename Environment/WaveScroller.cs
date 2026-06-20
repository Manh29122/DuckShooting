using System.Collections.Generic;
using UnityEngine;

namespace DuckShooting
{
    /// <summary>
    /// Tạo dải sóng cuộn vô hạn từ MỘT mảnh sprite sóng: ghép nhiều mảnh liền nhau phủ kín
    /// bề ngang màn hình, cho cả dải trôi ngang. Mảnh nào trôi ra khỏi mép sẽ nhảy về đầu
    /// kia (mảnh cuối thành mảnh đầu) → sóng chạy liên tục, liền mạch.
    ///
    /// Gắn vào một GameObject rỗng đặt ở ĐỘ CAO mặt nước mong muốn. Đặt scale = 1, rotation = 0.
    /// </summary>
    public class WaveScroller : MonoBehaviour
    {
        [Header("Sprite & hiển thị")]
        [Tooltip("Một mảnh sóng (ghép liên tiếp sẽ thành sóng hoàn chỉnh).")]
        [SerializeField] private Sprite waveSprite;
        [Tooltip("Tỉ lệ phóng to mỗi mảnh.")]
        [SerializeField] private float segmentScale = 1f;
        [SerializeField] private string sortingLayer = "Water";
        [SerializeField] private int sortingOrder = 0;

        [Tooltip("Chồng mép giữa các mảnh (world units) để khít lại, tránh hở. Tăng nhẹ nếu thấy khe hở.")]
        [SerializeField] private float overlap = 0.02f;

        [Header("Cuộn")]
        [Tooltip("Tốc độ trôi (unit/giây). >0 = sang phải, <0 = sang trái.")]
        [SerializeField] private float speed = 1f;
        [Tooltip("Số mảnh dư ra ngoài 2 mép để không bị hở khi cuộn.")]
        [SerializeField] private int extraSegments = 2;

        [Header("Tham chiếu")]
        [SerializeField] private Camera cam;

        private readonly List<Transform> _segments = new List<Transform>();
        private float _width;     // bề ngang 1 mảnh (đã tính scale)
        private float _step;      // khoảng cách đặt giữa 2 mảnh (= width - overlap)
        private float _totalWidth;

        private void Start()
        {
            if (cam == null) cam = Camera.main;
            Build();
        }

        private void Build()
        {
            if (waveSprite == null || cam == null)
            {
                Debug.LogWarning("[WaveScroller] Thiếu waveSprite hoặc Camera.", this);
                return;
            }

            _width = waveSprite.bounds.size.x * segmentScale;
            if (_width <= 0f) return;

            _step = Mathf.Max(0.0001f, _width - overlap);   // đặt sát nhau hơn 1 chút

            float camHalfW = cam.orthographicSize * cam.aspect;
            float screenW = camHalfW * 2f;

            // số mảnh đủ phủ màn hình + dư 2 bên
            int count = Mathf.CeilToInt(screenW / _step) + Mathf.Max(2, extraSegments);
            _totalWidth = count * _step;

            float startX = cam.transform.position.x - camHalfW - _step; // bắt đầu hơi ngoài mép trái
            float y = transform.position.y;

            for (int i = 0; i < count; i++)
            {
                var go = new GameObject($"WaveSeg_{i}");
                go.transform.SetParent(transform, true);
                go.transform.position = new Vector3(startX + i * _step, y, 0f);
                go.transform.localScale = Vector3.one * segmentScale;

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = waveSprite;
                if (!string.IsNullOrEmpty(sortingLayer)) sr.sortingLayerName = sortingLayer;
                sr.sortingOrder = sortingOrder;

                _segments.Add(go.transform);
            }
        }

        private void Update()
        {
            if (_segments.Count == 0 || cam == null) return;

            float dx = speed * Time.deltaTime;
            float camHalfW = cam.orthographicSize * cam.aspect;
            float leftWrap = cam.transform.position.x - camHalfW - _width;   // ra hẳn mép trái
            float rightWrap = cam.transform.position.x + camHalfW + _width;  // ra hẳn mép phải

            foreach (var t in _segments)
            {
                Vector3 p = t.position;
                p.x += dx;

                // Mảnh ra khỏi mép -> nhảy về đầu kia đúng 1 chu kỳ (giữ liền mạch).
                if (speed < 0f && p.x < leftWrap)
                    p.x += _totalWidth;
                else if (speed > 0f && p.x > rightWrap)
                    p.x -= _totalWidth;

                t.position = p;
            }
        }
    }
}
