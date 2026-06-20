using System.Collections;
using UnityEngine;

namespace DuckShooting
{
    /// <summary>
    /// Mục tiêu nhô lên từ vị trí ẩn (sau bục), giữ một lúc rồi tụt xuống và despawn
    /// (kiểu whack-a-mole). Nếu bị bắn trúng trong lúc hiện thì Target.Hit() xử lý despawn.
    /// </summary>
    public class PopUpMover : TargetMover
    {
        private Vector3 _hiddenPos;
        private Vector3 _shownPos;
        private float _riseTime = 0.25f;
        private float _holdTime = 1.5f;

        /// <summary>
        /// Cấu hình. anchorPos là vị trí hiện rõ; mục tiêu sẽ bắt đầu thấp hơn rỗ một khoảng.
        /// </summary>
        public void Configure(Vector3 shownPos, float riseHeight, float riseTime, float holdTime)
        {
            _shownPos = shownPos;
            _hiddenPos = shownPos + Vector3.down * Mathf.Abs(riseHeight);
            _riseTime = Mathf.Max(0.01f, riseTime);
            _holdTime = Mathf.Max(0f, holdTime);
            transform.position = _hiddenPos;
        }

        public override void Begin()
        {
            enabled = true;
            StartCoroutine(PopRoutine());
        }

        private IEnumerator PopRoutine()
        {
            yield return Move(_hiddenPos, _shownPos, _riseTime);   // nhô lên
            yield return new WaitForSeconds(_holdTime);            // giữ
            yield return Move(_shownPos, _hiddenPos, _riseTime);   // tụt xuống
            Target.Despawn();
        }

        private IEnumerator Move(Vector3 from, Vector3 to, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, t / duration);
                transform.position = Vector3.Lerp(from, to, k);
                yield return null;
            }
            transform.position = to;
        }
    }
}
