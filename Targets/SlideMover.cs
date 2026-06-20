using UnityEngine;

namespace DuckShooting
{
    /// <summary>
    /// Trượt mục tiêu theo phương ngang từ mép này sang mép kia. Khi ra khỏi biên
    /// thì despawn (không cộng điểm).
    /// </summary>
    public class SlideMover : TargetMover
    {
        private float _speed;
        private int _direction = 1;   // +1: sang phải, -1: sang trái
        private float _leftX;
        private float _rightX;

        /// <summary>
        /// Cấu hình tuyến trượt. Spawner gọi trước Begin().
        /// </summary>
        public void Configure(Vector3 startPos, int direction, float speed, float leftX, float rightX)
        {
            transform.position = startPos;
            _direction = direction >= 0 ? 1 : -1;
            _speed = speed;
            _leftX = leftX;
            _rightX = rightX;

            // lật sprite theo hướng đi cho tự nhiên
            var s = transform.localScale;
            s.x = Mathf.Abs(s.x) * (_direction >= 0 ? 1 : -1);
            transform.localScale = s;
        }

        public override void Begin()
        {
            enabled = true;
        }

        private void Update()
        {
            transform.position += Vector3.right * (_direction * _speed * Time.deltaTime);

            if ((_direction > 0 && transform.position.x > _rightX) ||
                (_direction < 0 && transform.position.x < _leftX))
            {
                Target.Despawn();
            }
        }
    }
}
