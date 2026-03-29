using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class User
{
    [Key]
    public int UserId { get; set; }
    public string Role { get; set; } = "user";
    [Required]
    [MaxLength(100)]
    public string? FullName { get; set; }
    public bool Gender { get; set; }
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public string? ResetPasswordToken { get; set; }
    public DateTime? ResetPasswordTokenExpiry { get; set; }
    public string? PhoneNumber { get; set; }
    public string? StudentAvatar { get; set; }
    public bool IsVerified { get; set; } = false;
    public bool IsOnline { get; set; }
    public DateTime LastActiveAt { get; set; }

    public int? UniversityId { get; set; }
    [ForeignKey("UniversityId")]
    public virtual University? University { get; set; }

    public ICollection<Listing>? Listings { get; set; }

    [InverseProperty("Sender")]
    public virtual ICollection<Message>? SentMessages { get; set; }

    // --- [MỚI] CÁC QUAN HỆ CẦN THÊM ---

    // 1. Lifestyle Profile (1-1)
    public virtual LifestyleProfile? LifestyleProfile { get; set; }

    // 2. Favorites (Các bài đăng user này đã thích)
    public virtual ICollection<Favorite>? Favorites { get; set; }

    // 3. UserBlock (Chặn và Bị chặn)
    [InverseProperty("Blocker")]
    public virtual ICollection<UserBlock>? BlocksInitiated { get; set; } // Danh sách mình chặn người khác

    [InverseProperty("Blocked")]
    public virtual ICollection<UserBlock>? BlocksReceived { get; set; } // Danh sách mình bị người khác chặn

    // 4. Requests (Gửi yêu cầu và Nhận yêu cầu)
    [InverseProperty("Sender")]
    public virtual ICollection<Request>? SentRequests { get; set; }

    [InverseProperty("Receiver")]
    public virtual ICollection<Request>? ReceivedRequests { get; set; }

    // 5. Reviews (Viết đánh giá và Được đánh giá)
    [InverseProperty("Reviewer")]
    public virtual ICollection<Review>? WrittenReviews { get; set; }

    [InverseProperty("TargetUser")]
    public virtual ICollection<Review>? ReceivedReviews { get; set; }

    // 6. MatchScore (Điểm tương thích)
    [InverseProperty("UserA")]
    public virtual ICollection<MatchScore>? MatchesAsUserA { get; set; }

    [InverseProperty("UserB")]
    public virtual ICollection<MatchScore>? MatchesAsUserB { get; set; }

    // 7. Conversation (Tham gia hội thoại)
    [InverseProperty("ParticipantOne")]
    public virtual ICollection<Conversation>? ConversationsAsUser1 { get; set; }

    [InverseProperty("ParticipantTwo")]
    public virtual ICollection<Conversation>? ConversationsAsUser2 { get; set; }

    // 8. StudentVerificationReview (Xác minh sinh viên)
    [InverseProperty("User")]
    public virtual ICollection<StudentVerificationRequest>? VerificationRequestsAsUser { get; set; }

    [InverseProperty("Admin")]
    public virtual ICollection<StudentVerificationRequest>? VerificationRequestsReviewed { get; set; }
}