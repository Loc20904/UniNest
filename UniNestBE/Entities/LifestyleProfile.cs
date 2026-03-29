using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class LifestyleProfile
{
    [Key]
    public int ProfileId { get; set; }

    public int UserId { get; set; }
    [ForeignKey("UserId")]
    public virtual User User { get; set; }

    [MaxLength(50)]
    public string SleepSchedule { get; set; } = "Normal"; // Early Bird, Night Owl, Normal

    [Range(1, 5)]
    public int CleanlinessLevel { get; set; } // 1: Messy -> 5: Very Clean

    public bool Smoking { get; set; } = false;
    public bool HasPet { get; set; } = false;

    [MaxLength(50)]
    public string CookingHabit { get; set; } // Often, Sometimes, Never

    [MaxLength(255)]
    public string? PersonalityTraits { get; set; } // Tags: Introvert, Extrovert, Quiet...

    [Column(TypeName = "decimal(18, 2)")]
    public decimal BudgetMin { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal BudgetMax { get; set; }

    // Relationship: A profile can possess many lifestyle habits
    public List<LifestyleHabit> LifestyleHabits { get; set; } = new List<LifestyleHabit>();
}