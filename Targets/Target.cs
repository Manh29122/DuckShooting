using System;
using System.Collections;
using UnityEngine;

namespace DuckShooting
{
    /// <summary>
    /// Một mục tiêu (vịt hoặc bia) trong scene. Bản thân là một PREFAB cụm gồm hình +
    /// cái cán + Collider2D. Script này đặt ở GỐC prefab; Collider2D có thể ở gốc hoặc
    /// ở object con (phần hình bấm trúng).
    ///
    /// Khi bị bắn trúng: gắn sprite LỖ ĐẠN tại vị trí trúng, rồi XOAY rotation X về 90°
    /// (Mathf.Lerp theo <see cref="rotateDuration"/>) trước khi despawn về pool.
    /// </summary>
    public class Target : MonoBehaviour
    {
        [Header("Hiệu ứng khi trúng")]
        [Tooltip("Sprite lỗ đạn gắn lên chỗ bị bắn trúng.")]
        [SerializeField] private Sprite holeSprite;
        [Tooltip("Sorting Layer của lỗ đạn (nên cao hơn hình target để hiện đè lên).")]
        [SerializeField] private string holeSortingLayer = "Targets";
        [Tooltip("Sorting Order của lỗ đạn.")]
        [SerializeField] private int holeSortingOrder = 50;
        [Tooltip("Tỉ lệ phóng to lỗ đạn.")]
        [SerializeField] private float holeScale = 1f;
        [Tooltip("THỜI GIAN (giây) từ lúc trúng đến khi xoay rotation X đạt 90°.")]
        [SerializeField] private float rotateDuration = 0.5f;

        private Collider2D _col;
        private Action<Target> _onRelease;   // callback trả về pool
        private bool _alive;
        private bool _released;
        private Quaternion _initialRotation;
        private GameObject _hole;

        public TargetType Type { get; private set; }
        public bool IsAlive => _alive;

        private void Awake()
        {
            _col = GetComponentInChildren<Collider2D>(true);
            _initialRotation = transform.localRotation;
        }

        /// <summary>Khởi tạo (gọi bởi spawner mỗi lần lấy ra khỏi pool).</summary>
        public void Init(TargetType type, Action<Target> onRelease)
        {
            Type = type;
            _onRelease = onRelease;

            // reset trạng thái cho lần tái sử dụng từ pool
            StopAllCoroutines();
            _alive = true;
            _released = false;
            transform.localRotation = _initialRotation;
            if (_hole != null) { Destroy(_hole); _hole = null; }

            if (type != null && type.scale > 0f)
                transform.localScale = Vector3.one * type.scale;

            if (_col != null) _col.enabled = true;
        }

        /// <summary>
        /// Bị bắn trúng tại <paramref name="hitPoint"/> (world). Mục tiêu thường: gắn lỗ đạn +
        /// xoay ngã rồi despawn. Mục tiêu CẤM: game over ngay.
        /// </summary>
        public void Hit(Vector3 hitPoint)
        {
            if (!_alive)
            {
                Debug.Log($"[Target] {name}: Hit() bị bỏ qua vì !alive (đã bị bắn rồi?).", this);
                return;
            }
            _alive = false;
            if (_col != null) _col.enabled = false;

            if (Type != null && Type.forbidden)
            {
                Debug.Log($"[Target] {name}: trúng mục tiêu CẤM -> game over.", this);
                GameEvents.RaiseForbiddenHit(transform.position);
                Despawn();
                return;
            }

            int points = Type != null ? Type.points : 10;
            GameEvents.RaiseTargetHit(points, transform.position);  // điểm + popup + combo

            SpawnHole(hitPoint);
            StartCoroutine(FallThenDespawn());
        }

        /// <summary>Tạo sprite lỗ đạn tại vị trí trúng, làm con của target để xoay cùng.</summary>
        private void SpawnHole(Vector3 hitPoint)
        {
            if (holeSprite == null) return;

            _hole = new GameObject("BulletHole");
            _hole.transform.SetParent(transform, true);   // giữ world position
            _hole.transform.position = hitPoint;
            _hole.transform.localScale = Vector3.one * holeScale;

            var sr = _hole.AddComponent<SpriteRenderer>();
            sr.sprite = holeSprite;
            if (!string.IsNullOrEmpty(holeSortingLayer)) sr.sortingLayerName = holeSortingLayer;
            sr.sortingOrder = holeSortingOrder;
        }

        /// <summary>Xoay rotation X từ 0 -> 90 độ bằng Mathf.Lerp trong rotateDuration giây.</summary>
        private IEnumerator FallThenDespawn()
        {
            Quaternion startRot = transform.localRotation;

            if (rotateDuration <= 0f)
            {
                transform.localRotation = startRot * Quaternion.Euler(90f, 0f, 0f);
            }
            else
            {
                float t = 0f;
                while (t < rotateDuration)
                {
                    t += Time.deltaTime;
                    float k = Mathf.Clamp01(t / rotateDuration);
                    float angleX = Mathf.Lerp(0f, 90f, k);     // <-- Mathf.Lerp theo yêu cầu
                    transform.localRotation = startRot * Quaternion.Euler(angleX, 0f, 0f);
                    yield return null;
                }
                transform.localRotation = startRot * Quaternion.Euler(90f, 0f, 0f);
            }

            Despawn();
        }

        /// <summary>Trả về pool, KHÔNG cộng điểm (mục tiêu thoát/tụt xuống hoặc ngã xong).</summary>
        public void Despawn()
        {
            if (_released) return;
            _released = true;
            _alive = false;
            if (_col != null) _col.enabled = false;

            StopAllCoroutines();
            foreach (var mover in GetComponents<TargetMover>())
                mover.StopMoving();

            if (_hole != null) { Destroy(_hole); _hole = null; }
            transform.localRotation = _initialRotation;   // reset cho lần dùng sau

            Debug.Log($"[Target] {name}: despawn.", this);

            if (_onRelease != null) _onRelease(this);
            else gameObject.SetActive(false);
        }
    }
}
