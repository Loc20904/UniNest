public interface IAiMatchingService
{
    /// <summary>
    /// Gợi ý bạn cùng phòng dựa trên: Lối sống (40%) + Vị trí (40%) + Cùng trường (20%)
    /// </summary>
    /// <param name="currentUserId">ID của người đang đi tìm</param>
    /// <returns>Danh sách ứng viên đã sắp xếp theo điểm số</returns>
    Task<List<RoommateRecommendationDto>> GetRoommateRecommendations(int currentUserId);
}