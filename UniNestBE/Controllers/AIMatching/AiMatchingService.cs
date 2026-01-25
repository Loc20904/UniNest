using Microsoft.EntityFrameworkCore;

public class AiMatchingService : IAiMatchingService
{
    private readonly UniNestDbContext _context;

    public AiMatchingService(UniNestDbContext context)
    {
        _context = context;
    }

    public async Task<List<RoommateRecommendationDto>> GetRoommateRecommendations(int currentUserId)
    {
        // 1. Lấy thông tin người tìm (Seeker)
        var seeker = await _context.Users.AsNoTracking()
            .Include(u => u.LifestyleProfile)
            .Include(u => u.University)
                .ThenInclude(uni => uni!.Addresses)
            .FirstOrDefaultAsync(u => u.UserId == currentUserId);

        // Kiểm tra null an toàn cho cả User và Profile
        if (seeker == null || seeker.LifestyleProfile == null)
            return new List<RoommateRecommendationDto>();

        // Lấy tọa độ trường (Sử dụng ?. để tránh null reference)
        var seekerSchoolLoc = seeker.University?.Addresses?.FirstOrDefault();

        var candidates = await _context.Users.AsNoTracking()
            .Include(u => u.LifestyleProfile)
            .Include(u => u.University)
            .Include(u => u.Listings!)
                .ThenInclude(l => l.Address)
            .Where(u => u.UserId != currentUserId
                   && u.LifestyleProfile != null
                   && u.IsVerified)
            .ToListAsync();

        var recommendations = new List<RoommateRecommendationDto>();

        foreach (var candidate in candidates)
        {
            // Dù đã Where != null, static analysis đôi khi vẫn báo warning
            // Dòng này giúp compiler hiểu candidate.LifestyleProfile chắc chắn có dữ liệu
            if (candidate.LifestyleProfile == null) continue;

            // Tính điểm
            var result = CalculateTotalScore(seeker, candidate, seekerSchoolLoc);

            if (result.TotalScore >= 50)
            {
                // Lấy phòng trọ active (Safe navigation cho Listings)
                var activeListing = candidate.Listings?.FirstOrDefault(l => l.IsAvailable);

                recommendations.Add(new RoommateRecommendationDto
                {
                    UserId = candidate.UserId,
                    FullName = candidate.FullName ?? "Người dùng", // Fallback nếu tên null
                    AvatarUrl = candidate.StudentAvatar ?? "",
                    UniversityName = candidate.University?.UniName ?? "N/A",

                    TotalMatchScore = Math.Round(result.TotalScore, 0),
                    MatchTags = result.Tags,

                    MinPrice = activeListing?.Price,
                    ListingAddress = activeListing?.Address?.District ?? "Đang tìm phòng"
                });
            }
        }

        return recommendations.OrderByDescending(x => x.TotalMatchScore).Take(20).ToList();
    }

    // --- HÀM TÍNH ĐIỂM TỔNG HỢP ---
    private (double TotalScore, List<string> Tags) CalculateTotalScore(User seeker, User candidate, Address? seekerSchoolLoc)
    {
        double totalPoints = 0;
        var tags = new List<string>();

        // ---------------------------------------------------------
        // A. LIFESTYLE (40%)
        // Sử dụng toán tử ! (Null-forgiving) vì ta đã kiểm tra null ở hàm cha rồi
        // ---------------------------------------------------------
        var lifestyleResult = CalculateLifestyle(seeker.LifestyleProfile!, candidate.LifestyleProfile!);
        totalPoints += lifestyleResult.Score * 0.4;
        tags.AddRange(lifestyleResult.Tags);

        // ---------------------------------------------------------
        // B. UNIVERSITY AFFILIATION (20%)
        // ---------------------------------------------------------
        if (seeker.UniversityId != null
            && candidate.UniversityId != null
            && seeker.UniversityId == candidate.UniversityId)
        {
            totalPoints += 20;
            // Dùng ?. để an toàn nếu University chưa load kịp (dù đã Include)
            tags.Add($"Đồng môn {seeker.University?.UniName ?? "trường ĐH"}");
        }

        // ---------------------------------------------------------
        // C. PROXIMITY TO CAMPUS (40%)
        // ---------------------------------------------------------
        // Safe navigation cho Listings và Address
        var candidateRoom = candidate.Listings?.FirstOrDefault(l => l.IsAvailable && l.Address != null);

        if (seekerSchoolLoc != null && candidateRoom?.Address != null)
        {
            // Ép kiểu decimal -> double an toàn
            double distance = GeoCalculator.CalculateDistance(
                (double)seekerSchoolLoc.Latitude, (double)seekerSchoolLoc.Longitude,
                (double)candidateRoom.Address.Latitude, (double)candidateRoom.Address.Longitude
            );

            if (distance <= 2.0)
            {
                totalPoints += 40;
                tags.Add($"Gần trường bạn ({Math.Round(distance, 1)}km)");
            }
            else if (distance <= 5.0)
            {
                totalPoints += 20;
            }
        }

        // Chuẩn hóa điểm (Max 100)
        if (totalPoints > 100) totalPoints = 100;

        return (totalPoints, tags);
    }

    // --- LOGIC LIFESTYLE ---
    private (double Score, List<string> Tags) CalculateLifestyle(LifestyleProfile p1, LifestyleProfile p2)
    {
        double pts = 0;
        var tags = new List<string>();

        // 1. Giờ giấc
        if (p1.SleepSchedule == p2.SleepSchedule)
        {
            pts += 30;
            tags.Add("Hợp giờ sinh hoạt");
        }

        // 2. Sạch sẽ
        int diffClean = Math.Abs(p1.CleanlinessLevel - p2.CleanlinessLevel);
        if (diffClean == 0) pts += 30;
        else if (diffClean == 1) pts += 15;

        // 3. Hút thuốc (Dealbreaker)
        if (p1.Smoking != p2.Smoking) pts -= 20;
        else pts += 20;

        // 4. Ngân sách
        bool budgetOverlap = (p1.BudgetMin <= p2.BudgetMax) && (p1.BudgetMax >= p2.BudgetMin);
        if (budgetOverlap)
        {
            pts += 20;
            tags.Add("Hợp ví tiền");
        }

        // Normalize
        if (pts < 0) pts = 0;
        if (pts > 100) pts = 100;

        return (pts, tags);
    }
}