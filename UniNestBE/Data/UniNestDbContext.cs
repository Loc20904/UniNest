using Microsoft.EntityFrameworkCore;
using UniNestBE.Entities;

public class UniNestDbContext : DbContext
{
    public UniNestDbContext(DbContextOptions<UniNestDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Listing> Listings { get; set; }
    public DbSet<ListingImage> ListingImages { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<UserBlock> UserBlocks { get; set; }
    public DbSet<LifestyleProfile> LifestyleProfiles { get; set; }
    public DbSet<MatchScore> MatchScores { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<University> Universities { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<Request> Requests { get; set; }
    public DbSet<Amenity> Amenities { get; set; }
    public DbSet<LifestyleHabit> LifestyleHabits { get; set; }
    public DbSet<StudentVerificationRequest> StudentVerificationRequests { get; set; }

    public DbSet<PropertyType> PropertyTypes { get; set; }


    public DbSet<AllowedEmailDomain> AllowedEmailDomains { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Cấu hình quan hệ 1-1: Listing có 1 Address
        modelBuilder.Entity<Listing>()
            .HasOne(l => l.Address)
            .WithOne(a => a.Listing)
            .HasForeignKey<Address>(a => a.ListingId);

        // SEED DATA (Dữ liệu mẫu để test Map ngay lập tức)
        // Tạo 1 User mẫu
        modelBuilder.Entity<User>().HasData(
            new User { UserId = 1, FullName = "Admin Demo", Email = "admin@udn.vn", PasswordHash = "123", Role = "admin" }
        );

        // Tạo 2 Listing mẫu
        modelBuilder.Entity<Listing>().HasData(
            new Listing { ListingId = 1, OwnerId = 1, Title = "Trọ giá rẻ gần Bách Khoa", Price = 1500000, AreaSquareMeters = 20 },
            new Listing { ListingId = 2, OwnerId = 1, Title = "Căn hộ mini Sơn Trà", Price = 3500000, AreaSquareMeters = 40 }
        );

        // Tạo 2 Address mẫu tại Đà Nẵng (Đã check tọa độ thật)
        modelBuilder.Entity<Address>().HasData(
            // Gần ĐH Bách Khoa
            new Address { AddressId = 1, ListingId = 1, FullAddress = "54 Nguyễn Lương Bằng", Latitude = 16.073801m, Longitude = 108.149914m },
            // Gần Cầu Rồng
            new Address { AddressId = 2, ListingId = 2, FullAddress = "Đường Trần Hưng Đạo", Latitude = 16.061735m, Longitude = 108.232372m }
        );

        // Khởi tạo các tiện ích cơ bản (Seed Data cho Amenities)
        modelBuilder.Entity<Amenity>().HasData(
            new Amenity { AmenityId = 1, Name = "Wi-Fi", Icon = "wifi" },
            new Amenity { AmenityId = 2, Name = "Air Conditioning", Icon = "ac_unit" },
            new Amenity { AmenityId = 3, Name = "Private Bath", Icon = "bathtub" },
            new Amenity { AmenityId = 4, Name = "Parking", Icon = "directions_car" },
            new Amenity { AmenityId = 5, Name = "Kitchen", Icon = "kitchen" },
            new Amenity { AmenityId = 6, Name = "Laundry", Icon = "local_laundry_service" }
        );

        // Khởi tạo thói quen sinh hoạt cơ bản (Seed Data cho LifestyleHabits)
        modelBuilder.Entity<LifestyleHabit>().HasData(
            new LifestyleHabit { LifestyleHabitId = 1, Name = "Non-smoker only" },
            new LifestyleHabit { LifestyleHabitId = 2, Name = "Pet friendly" },
            new LifestyleHabit { LifestyleHabitId = 3, Name = "Late-night studying" }
        );

        // Khởi tạo tên miền hợp lệ mặc định (Seed data cho AllowedEmailDomains)
        modelBuilder.Entity<AllowedEmailDomain>().HasData(
            new AllowedEmailDomain { DomainId = 1, DomainName = "edu.vn", Description = "Email Sinh viên Toàn quốc" }
        );

        modelBuilder.Entity<Conversation>()
        .HasOne(c => c.ParticipantOne)
        .WithMany() // Hoặc .WithMany(u => u.ConversationsAsPartOne) nếu bạn đã khai báo trong User
        .HasForeignKey(c => c.ParticipantOneID)
        .OnDelete(DeleteBehavior.Restrict); // <--- QUAN TRỌNG: Không xóa Conversation khi xóa User

        modelBuilder.Entity<Conversation>()
            .HasOne(c => c.ParticipantTwo)
            .WithMany() // Hoặc .WithMany(u => u.ConversationsAsPartTwo)
            .HasForeignKey(c => c.ParticipantTwoID)
            .OnDelete(DeleteBehavior.Restrict); // <--- QUAN TRỌNG

        // 2. Cấu hình cho Message (Tùy chọn, nhưng nên làm)
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderID)
            .OnDelete(DeleteBehavior.Restrict);

        // 3. Cấu hình cho UserBlock
        modelBuilder.Entity<UserBlock>()
            .HasOne(b => b.Blocker)
            .WithMany()
            .HasForeignKey(b => b.BlockerID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserBlock>()
            .HasOne(b => b.Blocked)
            .WithMany()
            .HasForeignKey(b => b.BlockedID)
            .OnDelete(DeleteBehavior.Restrict);

        // Một User vừa là Blocker, vừa là Blocked -> Cần tắt Cascade Delete
        modelBuilder.Entity<UserBlock>()
            .HasOne(ub => ub.Blocker)
            .WithMany(u => u.BlocksInitiated)
            .HasForeignKey(ub => ub.BlockerID)
            .OnDelete(DeleteBehavior.Restrict); // Xóa User không tự xóa UserBlock

        modelBuilder.Entity<UserBlock>()
            .HasOne(ub => ub.Blocked)
            .WithMany(u => u.BlocksReceived)
            .HasForeignKey(ub => ub.BlockedID)
            .OnDelete(DeleteBehavior.Restrict);

        // --- 2. Cấu hình Request ---
        modelBuilder.Entity<Request>()
            .HasOne(r => r.Sender)
            .WithMany(u => u.SentRequests)
            .HasForeignKey(r => r.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Request>()
            .HasOne(r => r.Receiver)
            .WithMany(u => u.ReceivedRequests)
            .HasForeignKey(r => r.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- 3. Cấu hình Review ---
        modelBuilder.Entity<Review>()
            .HasOne(rv => rv.Reviewer)
            .WithMany(u => u.WrittenReviews)
            .HasForeignKey(rv => rv.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Review>()
            .HasOne(rv => rv.TargetUser)
            .WithMany(u => u.ReceivedReviews)
            .HasForeignKey(rv => rv.TargetUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- 4. Cấu hình MatchScore ---
        modelBuilder.Entity<MatchScore>()
            .HasOne(m => m.UserA)
            .WithMany(u => u.MatchesAsUserA)
            .HasForeignKey(m => m.UserAId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MatchScore>()
            .HasOne(m => m.UserB)
            .WithMany(u => u.MatchesAsUserB)
            .HasForeignKey(m => m.UserBId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- 5. Cấu hình Conversation ---
        modelBuilder.Entity<Conversation>()
            .HasOne(c => c.ParticipantOne)
            .WithMany(u => u.ConversationsAsUser1)
            .HasForeignKey(c => c.ParticipantOneID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Conversation>()
            .HasOne(c => c.ParticipantTwo)
            .WithMany(u => u.ConversationsAsUser2)
            .HasForeignKey(c => c.ParticipantTwoID)
            .OnDelete(DeleteBehavior.Restrict);

        // --- 6. Cấu hình Message ---
        // Sender xóa nick -> Tin nhắn giữ lại (hoặc set NULL) để người kia vẫn đọc được lịch sử
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(m => m.SenderID)
            .OnDelete(DeleteBehavior.Restrict);

        // --- 7. Cấu hình Favorite (Tùy chọn) ---
        // Nếu User bị xóa, Favorite của họ cũng nên mất -> Có thể để Cascade
        modelBuilder.Entity<Favorite>()
            .HasOne(f => f.User)
            .WithMany(u => u.Favorites)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- 8. Cấu hình Listing & Address ---
        // Listing xóa -> Address xóa theo (Quan hệ 1-1 hoặc 1-n chặt chẽ)
        modelBuilder.Entity<Listing>()
            .HasOne(l => l.Address)
            .WithOne(a => a.Listing)
            .HasForeignKey<Address>(a => a.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- 9. Cấu hình StudentVerificationRequest ---
        modelBuilder.Entity<StudentVerificationRequest>()
            .HasOne(svr => svr.User)
            .WithMany(u => u.VerificationRequestsAsUser)
            .HasForeignKey(svr => svr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StudentVerificationRequest>()
            .HasOne(svr => svr.Admin)
            .WithMany(u => u.VerificationRequestsReviewed)
            .HasForeignKey(svr => svr.ReviewedByAdminId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}