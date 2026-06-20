using UnityEngine;

namespace DuckShooting
{
    /// <summary>
    /// Định nghĩa một loại mục tiêu (vịt vàng, vịt xanh, bia đỏ, bia nhiều màu...).
    /// Tạo asset qua menu: Create > DuckShooting > Target Type.
    /// </summary>
    [CreateAssetMenu(menuName = "DuckShooting/Target Type", fileName = "TargetType")]
    public class TargetType : ScriptableObject
    {
        [Tooltip("Tên/ID để gỡ lỗi.")]
        public string id = "duck_yellow";

        [Tooltip("Prefab của mục tiêu này (gồm hình vịt/bia + CÁI CÁN + Collider + script Target).")]
        public Target prefab;

        [Tooltip("Mục tiêu CẤM bắn: bắn trúng là GAME OVER ngay (không cộng điểm).")]
        public bool forbidden = false;

        [Tooltip("Điểm cộng khi bắn trúng (bỏ qua nếu forbidden).")]
        public int points = 10;

        [Tooltip("Tốc độ trượt ngang (unit/giây) khi dùng SlideMover.")]
        public float moveSpeed = 2f;

        [Tooltip("Tỉ lệ scale của sprite trong scene.")]
        public float scale = 1f;

        [Tooltip("Trọng số xuất hiện khi spawn (cao hơn = hay gặp hơn).")]
        [Min(0f)] public float spawnWeight = 1f;
    }
}
