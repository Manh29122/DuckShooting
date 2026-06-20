using UnityEngine;
using UnityEngine.InputSystem;

namespace DuckShooting
{
    /// <summary>
    /// Xử lý chạm-để-bắn (mobile) qua Input System. Dùng Pointer.current nên hoạt động
    /// với cả Touchscreen và Mouse (tiện test trên PC).
    ///
    /// Điểm bắn LẤY TỪ <see cref="GunController.ShootPoint"/> (nếu gán) — bắn ngay tại đầu
    /// điểm ngắm của khẩu súng. Nếu không gán GunController thì bắn tại vị trí con trỏ.
    ///
    /// KHÔNG còn giới hạn đạn: bắn liên tục, chỉ bị giới hạn bởi thời gian hồi (cooldown)
    /// giữa 2 phát.
    /// </summary>
    public class ShootingController : MonoBehaviour
    {
        [Header("Tham chiếu")]
        [SerializeField] private Camera cam;
        [Tooltip("Layer chứa mục tiêu (đặt là 'Targets').")]
        [SerializeField] private LayerMask targetMask;
        [Tooltip("Khẩu súng điều khiển điểm bắn. Bỏ trống = bắn tại vị trí con trỏ.")]
        [SerializeField] private GunController gun;

        [Header("Gỡ lỗi")]
        [Tooltip("In Debug.Log chi tiết từng bước bắn ra Console.")]
        [SerializeField] private bool debugLog = true;

        [Header("Cách bắn")]
        [Tooltip("Thời gian hồi tối thiểu giữa 2 phát bắn (giây).")]
        [SerializeField] private float fireCooldown = 1f;
        [Tooltip("Phải NHẤN ĐÚP mới bắn (tắt = chạm đơn bắn liên tục theo cooldown).")]
        [SerializeField] private bool requireDoubleTap = false;
        [Tooltip("Thời gian tối đa giữa 2 lần chạm để tính là nhấn đúp (giây).")]
        [SerializeField] private float doubleTapWindow = 0.3f;
        [Tooltip("Khoảng cách tối đa (pixel) giữa 2 lần chạm để tính là nhấn đúp. <=0 = bỏ qua kiểm tra vị trí.")]
        [SerializeField] private float doubleTapMaxDistance = 100f;

        private float _lastFireTime = -999f;

        // theo dõi lần chạm trước để phát hiện nhấn đúp
        private float _lastTapTime = -999f;
        private Vector2 _lastTapPos;

        private void Awake()
        {
            if (cam == null) cam = Camera.main;
        }

        private void OnEnable()
        {
            GameEvents.OnGameStart += ResetOnStart;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStart -= ResetOnStart;
        }

        private void ResetOnStart()
        {
            _lastFireTime = -999f;
            _lastTapTime = -999f;
        }

        private void Update()
        {
            var pointer = Pointer.current;
            if (pointer == null) return;
            if (!pointer.press.wasPressedThisFrame) return;

            Vector2 pos = pointer.position.ReadValue();

            if (GameManager.Instance == null)
            {
                if (debugLog) Debug.LogWarning("[Shoot] Có chạm nhưng KHÔNG có GameManager trong scene.");
                return;
            }
            if (GameManager.Instance.State != GameState.Playing)
            {
                if (debugLog) Debug.Log($"[Shoot] Có chạm nhưng bỏ qua: State = {GameManager.Instance.State} (chưa Playing).");
                return;
            }

            if (!requireDoubleTap)
            {
                if (debugLog) Debug.Log("[Shoot] Chạm đơn (Require Double Tap = off).");
                TryShoot(pos);
                return;
            }

            // --- Phát hiện nhấn đúp ---
            float now = Time.time;
            bool inTime = (now - _lastTapTime) <= doubleTapWindow;
            bool inRange = doubleTapMaxDistance <= 0f
                           || Vector2.Distance(pos, _lastTapPos) <= doubleTapMaxDistance;

            if (inTime && inRange)
            {
                if (debugLog) Debug.Log("[Shoot] Nhận DOUBLE-TAP -> bắn.");
                TryShoot(pos);          // chạm thứ 2 đủ nhanh & đủ gần -> BẮN
                _lastTapTime = -999f;   // reset: cần 2 chạm mới cho phát kế tiếp
            }
            else
            {
                if (debugLog) Debug.Log($"[Shoot] Chạm lần 1 (chờ chạm 2 trong {doubleTapWindow}s để thành double-tap).");
                _lastTapTime = now;     // ghi nhận đây là chạm thứ 1
                _lastTapPos = pos;
            }
        }

        private void TryShoot(Vector2 screenPos)
        {
            // Giới hạn nhịp bắn: chỉ bắn lại sau khi đã hồi đủ thời gian.
            float since = Time.time - _lastFireTime;
            if (since < fireCooldown)
            {
                if (debugLog) Debug.Log($"[Shoot] Bị chặn bởi cooldown: còn {fireCooldown - since:0.00}s.");
                return;
            }
            _lastFireTime = Time.time;

            GameEvents.RaiseShotFired();   // -> MuzzleEffect (khói đầu súng), âm thanh...

            // Ưu tiên bắn tại điểm ngắm của súng để súng & điểm bắn luôn khớp.
            Vector3 world = gun != null ? gun.ShootPoint : ScreenToWorld(screenPos);
            if (debugLog && gun == null)
                Debug.LogWarning("[Shoot] Chưa gán Gun (GunController) -> bắn tại vị trí con trỏ thay vì ShootPoint.");

            Collider2D hit = Physics2D.OverlapPoint(world, targetMask);

            if (debugLog)
            {
                string colInfo = hit != null
                    ? $"'{hit.name}' (layer {LayerMask.LayerToName(hit.gameObject.layer)})"
                    : $"KHÔNG có collider nào tại đây (Target Mask value = {targetMask.value})";
                Debug.Log($"[Shoot] Bắn tại world={world} | OverlapPoint -> {colInfo}");
            }

            Target target = hit != null ? hit.GetComponentInParent<Target>() : null;
            if (target != null)
            {
                if (debugLog) Debug.Log($"[Shoot] >>> BẮN TRÚNG: {target.name}", target);
                target.Hit(world);            // truyền vị trí trúng -> gắn lỗ đạn + xoay ngã
            }
            else
            {
                if (debugLog && hit != null)
                    Debug.LogWarning($"[Shoot] Có collider '{hit.name}' nhưng KHÔNG tìm thấy script Target ở nó/cha -> coi như trượt.", hit);
                GameEvents.RaiseShotMissed();  // -> combo reset
            }
        }

        private Vector3 ScreenToWorld(Vector2 screenPos)
        {
            if (cam == null) return Vector3.zero;
            Vector3 w = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -cam.transform.position.z));
            w.z = 0f;
            return w;
        }
    }
}
