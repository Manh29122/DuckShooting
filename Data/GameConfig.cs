using System.Collections.Generic;
using UnityEngine;

namespace DuckShooting
{
    /// <summary>
    /// Cấu hình tổng cho một vòng chơi. Tạo asset qua: Create > DuckShooting > Game Config.
    /// </summary>
    [CreateAssetMenu(menuName = "DuckShooting/Game Config", fileName = "GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("Vòng chơi")]
        [Tooltip("Chơi VÔ HẠN, không giới hạn thời gian. Chỉ thua khi bắn trúng mục tiêu cấm đủ số lần.")]
        public bool infinite = true;
        [Tooltip("(Chỉ khi KHÔNG vô hạn) Thời lượng mỗi vòng (giây).")]
        public float roundDuration = 60f;
        [Tooltip("(Chỉ khi vô hạn) Số giây để độ khó tăng từ thấp -> tối đa.")]
        public float difficultyRampSeconds = 120f;

        [Header("Thua cuộc")]
        [Tooltip("Số lần bắn trúng mục tiêu CẤM thì game over.")]
        [Min(1)] public int forbiddenHitsToEnd = 3;

        [Header("Spawn mục tiêu")]
        [Tooltip("Khoảng cách thời gian giữa 2 lần spawn (min, max) lúc ĐẦU vòng.")]
        public Vector2 spawnIntervalRange = new Vector2(0.9f, 1.6f);

        [Tooltip("Hệ số nhân interval theo tiến độ vòng (x trục 0..1 = % thời gian đã trôi). " +
                 "Giá trị <1 = spawn nhanh hơn về cuối -> khó dần.")]
        public AnimationCurve difficultyOverTime = AnimationCurve.Linear(0f, 1f, 1f, 0.5f);

        [Tooltip("Số mục tiêu tối đa tồn tại cùng lúc.")]
        public int maxConcurrentTargets = 6;

        [Tooltip("Xác suất một mục tiêu mới dùng kiểu pop-up (nhô lên/tụt) thay vì trượt ngang.")]
        [Range(0f, 1f)] public float popUpChance = 0.4f;

        [Header("Các loại mục tiêu")]
        public List<TargetType> targetTypes = new List<TargetType>();

        /// <summary>Chọn ngẫu nhiên một TargetType theo trọng số spawnWeight.</summary>
        public TargetType PickRandomType()
        {
            if (targetTypes == null || targetTypes.Count == 0) return null;

            float total = 0f;
            foreach (var t in targetTypes)
                if (t != null) total += Mathf.Max(0f, t.spawnWeight);

            if (total <= 0f) return targetTypes[0];

            float r = Random.value * total;
            foreach (var t in targetTypes)
            {
                if (t == null) continue;
                r -= Mathf.Max(0f, t.spawnWeight);
                if (r <= 0f) return t;
            }
            return targetTypes[targetTypes.Count - 1];
        }
    }
}
