using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class LifestyleHabit
{
    [Key]
    public int LifestyleHabitId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    // Relationship: A habit can be selected in multiple listings
    public List<Listing> Listings { get; set; } = new List<Listing>();
}
