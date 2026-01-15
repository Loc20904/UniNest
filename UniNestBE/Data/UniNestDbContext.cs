using Microsoft.EntityFrameworkCore;

public class UniNestDbContext : DbContext
{
    public UniNestDbContext(DbContextOptions<UniNestDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Listing> Listings { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<ListingImage> ListingImages { get; set; }

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
    }
}