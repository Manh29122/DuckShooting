using UnityEngine;

namespace DuckShooting
{
    /// <summary>
    /// Sinh mây trôi ngang. Mây xuất hiện từ NGOÀI mép màn hình rồi trôi qua, ra mép kia thì huỷ.
    /// Mỗi đám mây random: sprite, độ cao, tốc độ, kích cỡ. Chạy độc lập với gameplay (mây trang trí).
    /// Gắn vào 1 GameObject rỗng bất kỳ trong scene.
    /// </summary>
    public class CloudSpawner : MonoBehaviour
    {
        [Header("Sprite & hiển thị")]
        [Tooltip("Các sprite mây (chọn ngẫu nhiên mỗi lần spawn).")]
        [SerializeField] private Sprite[] cloudSprites;
        [SerializeField] private string sortingLayer = "Background";
        [SerializeField] private int sortingOrder = -10;

        [Header("Spawn")]
        [Tooltip("Khoảng thời gian ngẫu nhiên giữa 2 lần spawn (min, max) giây.")]
        [SerializeField] private Vector2 spawnIntervalRange = new Vector2(2f, 5f);
        [Tooltip("Dải độ cao (Y) cho mây.")]
        [SerializeField] private Vector2 yRange = new Vector2(2f, 4f);
        [Tooltip("Dải tốc độ trôi (unit/giây).")]
        [SerializeField] private Vector2 speedRange = new Vector2(0.5f, 1.5f);
        [Tooltip("Dải tỉ lệ kích cỡ.")]
        [SerializeField] private Vector2 scaleRange = new Vector2(0.8f, 1.4f);

        [Header("Hướng")]
        [Tooltip("Trôi sang phải (tắt = sang trái). Bỏ qua nếu bật Random Direction.")]
        [SerializeField] private bool leftToRight = true;
        [Tooltip("Mỗi đám mây chọn hướng ngẫu nhiên.")]
        [SerializeField] private bool randomDirection = false;

        [Header("Khác")]
        [Tooltip("Khoảng cách spawn/huỷ ngoài mép màn hình.")]
        [SerializeField] private float offscreenMargin = 2f;
        [Tooltip("Số đám mây có sẵn trên màn hình lúc bắt đầu.")]
        [SerializeField] private int prewarmCount = 2;
        [SerializeField] private Camera cam;

        private float _timer;

        private void Awake()
        {
            if (cam == null) cam = Camera.main;
        }

        private void Start()
        {
            for (int i = 0; i < prewarmCount; i++)
                SpawnCloud(onScreen: true);
            _timer = NextInterval();
        }

        private void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                SpawnCloud(onScreen: false);
                _timer = NextInterval();
            }
        }

        private float NextInterval() => Random.Range(spawnIntervalRange.x, spawnIntervalRange.y);

        private void SpawnCloud(bool onScreen)
        {
            if (cloudSprites == null || cloudSprites.Length == 0 || cam == null) return;

            var sprite = cloudSprites[Random.Range(0, cloudSprites.Length)];
            if (sprite == null) return;

            int dir = randomDirection ? (Random.value < 0.5f ? 1 : -1) : (leftToRight ? 1 : -1);
            float speed = Random.Range(speedRange.x, speedRange.y);
            float scale = Random.Range(scaleRange.x, scaleRange.y);
            float y = Random.Range(yRange.x, yRange.y);

            float halfW = cam.orthographicSize * cam.aspect;
            float camX = cam.transform.position.x;
            float leftEdge = camX - halfW;
            float rightEdge = camX + halfW;

            float startX;
            float despawnX;
            if (dir > 0) // trôi sang phải: vào từ trái, ra ở phải
            {
                startX = onScreen ? Random.Range(leftEdge, rightEdge) : leftEdge - offscreenMargin;
                despawnX = rightEdge + offscreenMargin;
            }
            else          // trôi sang trái: vào từ phải, ra ở trái
            {
                startX = onScreen ? Random.Range(leftEdge, rightEdge) : rightEdge + offscreenMargin;
                despawnX = leftEdge - offscreenMargin;
            }

            var go = new GameObject("Cloud");
            go.transform.SetParent(transform, true);
            go.transform.position = new Vector3(startX, y, 0f);
            go.transform.localScale = Vector3.one * scale;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            if (!string.IsNullOrEmpty(sortingLayer)) sr.sortingLayerName = sortingLayer;
            sr.sortingOrder = sortingOrder;

            go.AddComponent<Cloud>().Init(speed, dir, despawnX);
        }
    }
}
