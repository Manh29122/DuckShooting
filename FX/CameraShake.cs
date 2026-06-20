using UnityEngine;

namespace DuckShooting
{
    /// <summary>
    /// Rung camera khi BẮN TRÚNG mục tiêu. Gắn script này vào Main Camera.
    /// Dịch local position của camera một lượng ngẫu nhiên giảm dần rồi trả về vị trí gốc.
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        [Tooltip("Thời gian rung (giây).")]
        [SerializeField] private float duration = 0.15f;
        [Tooltip("Biên độ rung tối đa (world units).")]
        [SerializeField] private float magnitude = 0.15f;
        [Tooltip("Cũng rung khi bắn trúng mục tiêu CẤM (game over).")]
        [SerializeField] private bool shakeOnForbidden = true;

        private Vector3 _baseLocalPos;
        private float _timer;

        private void Awake() => _baseLocalPos = transform.localPosition;

        private void OnEnable()
        {
            GameEvents.OnTargetHit += OnHit;
            if (shakeOnForbidden) GameEvents.OnForbiddenHit += OnForbidden;
        }

        private void OnDisable()
        {
            GameEvents.OnTargetHit -= OnHit;
            GameEvents.OnForbiddenHit -= OnForbidden;
        }

        private void OnHit(int points, Vector3 worldPos) => Shake();
        private void OnForbidden(Vector3 worldPos) => Shake();

        /// <summary>Bắt đầu (hoặc làm mới) đợt rung.</summary>
        public void Shake() => _timer = duration;

        private void LateUpdate()
        {
            if (_timer > 0f)
            {
                _timer -= Time.deltaTime;
                float r = duration > 0f ? Mathf.Clamp01(_timer / duration) : 0f;
                Vector2 offset = Random.insideUnitCircle * (magnitude * r);
                transform.localPosition = _baseLocalPos + (Vector3)offset;
            }
            else
            {
                transform.localPosition = _baseLocalPos;
            }
        }
    }
}
