using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DuckShooting
{
    /// <summary>
    /// Sinh mục tiêu theo thời gian, trộn kiểu trượt ngang và pop-up, tăng nhịp dần
    /// về cuối vòng. Dùng <see cref="ComponentPool{T}"/> để tái sử dụng đối tượng.
    /// </summary>
    public class TargetSpawner : MonoBehaviour
    {
        [Header("Tham chiếu")]
        [SerializeField] private GameConfig config;
        [SerializeField] private Camera cam;
        [Tooltip("Cha chứa mục tiêu sinh ra (để gọn Hierarchy). Bỏ trống = dùng chính spawner.")]
        [SerializeField] private Transform container;

        [Header("Khu vực chơi (world units)")]
        [Tooltip("Khoảng Y (thấp..cao) cho các làn mục tiêu.")]
        [SerializeField] private Vector2 laneYRange = new Vector2(-0.5f, 2.0f);
        [Tooltip("Số làn ngang.")]
        [SerializeField] private int laneCount = 3;
        [Tooltip("Khoảng cách spawn ngoài mép màn hình cho mục tiêu trượt.")]
        [SerializeField] private float offscreenMargin = 1.5f;
        [Tooltip("Lề ngang chừa ra ở hai bên khi đặt mục tiêu pop-up.")]
        [SerializeField] private float horizontalPadding = 1.0f;

        [Header("Pop-up")]
        [SerializeField] private float popRiseHeight = 1.5f;
        [SerializeField] private float popRiseTime = 0.25f;
        [SerializeField] private Vector2 popHoldRange = new Vector2(1.0f, 2.0f);

        [Header("Pool")]
        [SerializeField] private int prewarm = 8;

        // Mỗi TargetType có pool riêng vì mỗi loại là một prefab khác nhau.
        private readonly Dictionary<TargetType, ComponentPool<Target>> _pools =
            new Dictionary<TargetType, ComponentPool<Target>>();
        private readonly HashSet<Target> _active = new HashSet<Target>();
        private Coroutine _loop;

        private void Awake()
        {
            if (cam == null) cam = Camera.main;
            if (container == null) container = transform;
        }

        private void OnEnable()
        {
            GameEvents.OnGameStart += HandleGameStart;
            GameEvents.OnGameOver += HandleGameOver;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStart -= HandleGameStart;
            GameEvents.OnGameOver -= HandleGameOver;
        }

        private void HandleGameStart()
        {
            DespawnAll();
            if (_loop != null) StopCoroutine(_loop);
            _loop = StartCoroutine(SpawnLoop());
        }

        private void HandleGameOver(int finalScore)
        {
            if (_loop != null) { StopCoroutine(_loop); _loop = null; }
            DespawnAll();
        }

        private IEnumerator SpawnLoop()
        {
            if (config == null)
            {
                Debug.LogError("[TargetSpawner] Thiếu GameConfig.", this);
                yield break;
            }

            while (GameManager.Instance != null && GameManager.Instance.State == GameState.Playing)
            {
                yield return new WaitForSeconds(NextInterval());

                if (_active.Count < config.maxConcurrentTargets)
                    SpawnOne();
            }
        }

        private float NextInterval()
        {
            float baseInterval = Random.Range(config.spawnIntervalRange.x, config.spawnIntervalRange.y);
            float progress = Progress01();
            float mult = config.difficultyOverTime != null
                ? Mathf.Max(0.05f, config.difficultyOverTime.Evaluate(progress))
                : 1f;
            return baseInterval * mult;
        }

        private float Progress01()
        {
            var gm = GameManager.Instance;
            return gm != null ? gm.DifficultyProgress01 : 0f;
        }

        private void SpawnOne()
        {
            var type = config.PickRandomType();
            if (type == null || type.prefab == null) return;

            var pool = GetPool(type);
            var target = pool.Get();
            target.transform.SetParent(container, false);
            // closure giữ đúng pool để trả về khi despawn
            target.Init(type, t => { _active.Remove(t); pool.Release(t); });
            _active.Add(target);

            float laneY = PickLaneY();
            bool popUp = Random.value < config.popUpChance;

            if (popUp)
            {
                var mover = GetOrAdd<PopUpMover>(target);
                DisableOtherMovers(target, mover);

                float x = Random.Range(LeftInner(), RightInner());
                float hold = Random.Range(popHoldRange.x, popHoldRange.y);
                mover.Configure(new Vector3(x, laneY, 0f), popRiseHeight, popRiseTime, hold);
                mover.Begin();
            }
            else
            {
                var mover = GetOrAdd<SlideMover>(target);
                DisableOtherMovers(target, mover);

                int dir = Random.value < 0.5f ? 1 : -1;
                float startX = dir > 0 ? LeftEdge() - offscreenMargin : RightEdge() + offscreenMargin;
                float leftBound = LeftEdge() - offscreenMargin - 0.5f;
                float rightBound = RightEdge() + offscreenMargin + 0.5f;
                mover.Configure(new Vector3(startX, laneY, 0f), dir, type.moveSpeed, leftBound, rightBound);
                mover.Begin();
            }
        }

        private ComponentPool<Target> GetPool(TargetType type)
        {
            if (!_pools.TryGetValue(type, out var pool))
            {
                pool = new ComponentPool<Target>(type.prefab, container, prewarm);
                _pools[type] = pool;
            }
            return pool;
        }

        private void DespawnAll()
        {
            // sao chép ra mảng vì Despawn sẽ sửa _active
            var snapshot = new List<Target>(_active);
            foreach (var t in snapshot)
                t.Despawn();
            _active.Clear();
        }

        // --- Helpers hình học ---
        private float HalfHeight() => cam != null ? cam.orthographicSize : 5f;
        private float HalfWidth() => HalfHeight() * (cam != null ? cam.aspect : 16f / 9f);
        private float LeftEdge() => (cam != null ? cam.transform.position.x : 0f) - HalfWidth();
        private float RightEdge() => (cam != null ? cam.transform.position.x : 0f) + HalfWidth();
        private float LeftInner() => LeftEdge() + horizontalPadding;
        private float RightInner() => RightEdge() - horizontalPadding;

        private float PickLaneY()
        {
            if (laneCount <= 1) return laneYRange.y;
            int lane = Random.Range(0, laneCount);
            float t = lane / (float)(laneCount - 1);
            return Mathf.Lerp(laneYRange.x, laneYRange.y, t);
        }

        private static T GetOrAdd<T>(Component on) where T : Component
        {
            return on.TryGetComponent<T>(out var c) ? c : on.gameObject.AddComponent<T>();
        }

        private static void DisableOtherMovers(Target target, TargetMover keep)
        {
            foreach (var m in target.GetComponents<TargetMover>())
                if (m != keep) m.StopMoving();
        }
    }
}
