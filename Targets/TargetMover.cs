using UnityEngine;

namespace DuckShooting
{
    /// <summary>
    /// Lớp cơ sở cho các kiểu di chuyển mục tiêu. Mỗi prefab Target có thể gắn nhiều
    /// mover; spawner chỉ kích hoạt đúng một mover cho mỗi lần spawn.
    /// </summary>
    [RequireComponent(typeof(Target))]
    public abstract class TargetMover : MonoBehaviour
    {
        protected Target Target;

        protected virtual void Awake()
        {
            Target = GetComponent<Target>();
            enabled = false; // mặc định tắt, spawner bật mover được chọn
        }

        /// <summary>Bắt đầu chuyển động (spawner gọi sau khi đã Configure).</summary>
        public abstract void Begin();

        /// <summary>Dừng chuyển động và dọn coroutine (Target gọi khi despawn).</summary>
        public virtual void StopMoving()
        {
            StopAllCoroutines();
            enabled = false;
        }
    }
}
