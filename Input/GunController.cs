using UnityEngine;
using UnityEngine.InputSystem;

namespace DuckShooting
{
    /// <summary>
    /// Điều khiển khẩu súng theo kiểu LẤY SÚNG LÀM TRUNG TÂM:
    ///  • Ngón tay / con trỏ điều khiển trực tiếp **vị trí khẩu súng** (gun bám theo ngón tay).
    ///  • **Điểm bắn (shootPoint)** đặt LỆCH khỏi súng một khoảng <see cref="shootOffset"/>
    ///    (mặc định phía trên) nên ngón tay KHÔNG che điểm ngắm.
    ///  • Khi bắn, súng **giật nhẹ (recoil)** nhưng điểm ngắm vẫn đứng yên để không lệch nhắm.
    /// </summary>
    public class GunController : MonoBehaviour
    {
        [Header("Tham chiếu")]
        [SerializeField] private Camera cam;
        [Tooltip("Khẩu súng = TRUNG TÂM điều khiển, bám theo ngón tay/con trỏ.")]
        [SerializeField] private Transform gun;
        [Tooltip("Điểm bắn (crosshair). Raycast bắn xuất phát từ đây. Đặt lệch khỏi súng để không bị ngón tay che.")]
        [SerializeField] private Transform shootPoint;

        [Header("Tinh chỉnh")]
        [Tooltip("Độ lệch của ĐIỂM BẮN so với SÚNG (world units). Vd (0, 2) = phía trên đầu súng 2 đơn vị.")]
        [SerializeField] private Vector2 shootOffset = new Vector2(0f, 2f);
        [Tooltip("Độ mượt khi súng bám theo ngón tay (0 = bám tức thì; vd 12 = mượt).")]
        [SerializeField] private float followLerp = 0f;
        [Tooltip("Giữ khẩu súng trong khung nhìn camera.")]
        [SerializeField] private bool clampToScreen = true;

        [Header("Xoay nòng (tùy chọn)")]
        [Tooltip("Xoay nòng súng hướng về điểm bắn (chỉ hữu ích khi shootOffset KHÔNG thẳng đứng).")]
        [SerializeField] private bool aimGunAtShootPoint = false;
        [Tooltip("Bù góc theo hướng sprite súng. Sprite chĩa LÊN -> -90; chĩa PHẢI -> 0.")]
        [SerializeField] private float gunAngleOffset = -90f;

        [Header("Giật khi bắn (recoil)")]
        [Tooltip("Bật rung/giật nhẹ khẩu súng mỗi phát bắn.")]
        [SerializeField] private bool enableRecoil = true;
        [Tooltip("Thời gian hiệu ứng giật (giây).")]
        [SerializeField] private float recoilDuration = 0.1f;
        [Tooltip("Độ dịch ngược (xuống) khi giật (world units).")]
        [SerializeField] private float recoilKickback = 0.12f;
        [Tooltip("Độ rung ngẫu nhiên thêm vào (world units).")]
        [SerializeField] private float recoilShake = 0.04f;
        [Tooltip("Góc nghiêng nòng khi giật (độ).")]
        [SerializeField] private float recoilAngle = 5f;

        private Vector3 _basePos;
        private bool _basePosInit;
        private Quaternion _gunBaseRotation;
        private float _recoilTimer;

        /// <summary>Vị trí world của điểm bắn (cho ShootingController dùng).</summary>
        public Vector3 ShootPoint => shootPoint != null ? shootPoint.position : transform.position;

        private void Awake()
        {
            if (cam == null) cam = Camera.main;
            if (gun != null) _gunBaseRotation = gun.rotation;
        }

        private void OnEnable() => GameEvents.OnShotFired += TriggerRecoil;
        private void OnDisable() => GameEvents.OnShotFired -= TriggerRecoil;

        private void TriggerRecoil() => _recoilTimer = recoilDuration;

        private void Update()
        {
            var pointer = Pointer.current;
            if (pointer == null || cam == null || gun == null) return;

            Vector3 world = ScreenToWorld(pointer.position.ReadValue());
            if (clampToScreen) world = ClampToCamera(world);

            // Vị trí "gốc" của súng (bám ngón tay, có thể mượt) — KHÔNG gồm recoil.
            if (!_basePosInit) { _basePos = world; _basePosInit = true; }
            _basePos = followLerp > 0f
                ? Vector3.Lerp(_basePos, world, followLerp * Time.deltaTime)
                : world;

            // Điểm bắn bám theo vị trí gốc (đứng yên, không rung) để giữ độ chính xác.
            Vector3 sp = _basePos + (Vector3)shootOffset;
            if (shootPoint != null) shootPoint.position = sp;

            // Hướng nòng cơ sở.
            Quaternion baseRot = _gunBaseRotation;
            if (aimGunAtShootPoint)
            {
                Vector3 dir = sp - _basePos;
                if (dir.sqrMagnitude > 0.0001f)
                {
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + gunAngleOffset;
                    baseRot = Quaternion.Euler(0f, 0f, angle);
                }
            }

            // Recoil: chỉ áp lên KHẨU SÚNG (không áp lên điểm bắn).
            Vector3 recoilPos = Vector3.zero;
            Quaternion recoilRot = Quaternion.identity;
            if (enableRecoil && _recoilTimer > 0f)
            {
                _recoilTimer -= Time.deltaTime;
                float r = recoilDuration > 0f ? Mathf.Clamp01(_recoilTimer / recoilDuration) : 0f;
                Vector2 shake = Random.insideUnitCircle * (recoilShake * r);
                recoilPos = Vector3.down * (recoilKickback * r) + (Vector3)shake;
                recoilRot = Quaternion.Euler(0f, 0f, recoilAngle * r);
            }

            gun.position = _basePos + recoilPos;
            gun.rotation = baseRot * recoilRot;
        }

        private Vector3 ScreenToWorld(Vector2 screenPos)
        {
            Vector3 w = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -cam.transform.position.z));
            w.z = 0f;
            return w;
        }

        private Vector3 ClampToCamera(Vector3 world)
        {
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;
            Vector3 c = cam.transform.position;
            world.x = Mathf.Clamp(world.x, c.x - halfW, c.x + halfW);
            world.y = Mathf.Clamp(world.y, c.y - halfH, c.y + halfH);
            return world;
        }
    }
}
