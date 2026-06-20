using UnityEngine;

namespace DuckShooting
{
    /// <summary>
    /// Sinh effect (khói/lửa đầu nòng) tại ĐIỂM ĐẦU SÚNG mỗi khi bắn.
    /// Kéo "điểm đầu súng" vào <see cref="muzzlePoint"/> và prefab effect vào
    /// <see cref="effectPrefab"/> trong Inspector.
    /// </summary>
    public class MuzzleEffect : MonoBehaviour
    {
        [Tooltip("Điểm đầu nòng súng — nơi effect xuất hiện (thường là 1 object con của Gun).")]
        [SerializeField] private Transform muzzlePoint;
        [Tooltip("Prefab effect khói (có ParticleSystem là tốt nhất).")]
        [SerializeField] private GameObject effectPrefab;

        [Tooltip("Gắn effect làm CON của điểm đầu súng để nó đi theo súng khi súng di chuyển.")]
        [SerializeField] private bool parentToMuzzle = false;
        [Tooltip("Tự huỷ effect sau (giây). <=0 = tự tính theo ParticleSystem.")]
        [SerializeField] private float lifetime = 2f;

        private void OnEnable() => GameEvents.OnShotFired += Spawn;
        private void OnDisable() => GameEvents.OnShotFired -= Spawn;

        private void Spawn()
        {
            if (effectPrefab == null || muzzlePoint == null) return;

            GameObject fx = Instantiate(
                effectPrefab,
                muzzlePoint.position,
                muzzlePoint.rotation,
                parentToMuzzle ? muzzlePoint : null);

            // Nếu effect có ParticleSystem thì phát và tính thời gian sống.
            float life = lifetime;
            var ps = fx.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                if (life <= 0f)
                {
                    var main = ps.main;
                    life = main.duration + main.startLifetime.constantMax;
                }
            }

            if (life > 0f) Destroy(fx, life);
        }
    }
}
