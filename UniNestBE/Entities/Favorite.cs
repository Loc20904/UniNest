using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Favorite
{
    [Key]
    public int FavoriteId { get; set; }

    public int UserId { get; set; }
    [ForeignKey("UserId")]
    public virtual User User { get; set; }

    public int ListingId { get; set; }
    [ForeignKey("ListingId")]
    public virtual Listing Listing { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}