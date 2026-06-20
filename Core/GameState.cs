namespace DuckShooting
{
    /// <summary>Các trạng thái của vòng chơi.</summary>
    public enum GameState
    {
        Ready,    // chưa bắt đầu
        Playing,  // đang chơi, mục tiêu spawn, bắn được
        GameOver  // hết giờ -> hiện "TIME UP!"
    }
}
