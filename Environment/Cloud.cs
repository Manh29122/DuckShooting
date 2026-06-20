using UnityEngine;

namespace DuckShooting
{
    /// <summary>
    /// Một đám mây trôi ngang. Tự huỷ khi ra khỏi mép màn hình.
    /// Được <see cref="CloudSpawner"/> tạo và cấu hình.
    /// </summary>
    public class Cloud : MonoBehaviour
    {
        private float _speed;
        private int _dir;          // +1 sang phải, -1 sang trái
        private float _despawnX;   // ra khỏi mốc này thì huỷ

        public void Init(float speed, int dir, float despawnX)
        {
            _speed = speed;
            _dir = dir >= 0 ? 1 : -1;
            _despawnX = despawnX;
        }

        private void Update()
        {
            transform.position += Vector3.right * (_dir * _speed * Time.deltaTime);

            if ((_dir > 0 && transform.position.x > _despawnX) ||
                (_dir < 0 && transform.position.x < _despawnX))
            {
                Destroy(gameObject);
            }
        }
    }
}
