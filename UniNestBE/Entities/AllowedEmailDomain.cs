using System.ComponentModel.DataAnnotations;

namespace UniNestBE.Entities
{
    public class AllowedEmailDomain
    {
        [Key]
        public int DomainId { get; set; }

        [Required]
        [MaxLength(255)]
        public string DomainName { get; set; } = null!;

        [MaxLength(255)]
        public string? Description { get; set; }
    }
}
