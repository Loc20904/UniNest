using System.ComponentModel.DataAnnotations;

public class PropertyType
{
    [Key]
    public int PropertyTypeId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
}
