namespace PlusAppointment.Models.Classes;

public class ShopPicture
{
    public int ShopPictureId { get; set; } // Primary key
    public int BusinessId { get; set; } // Foreign key to Business
    public string S3ImageUrl { get; set; } // URL of the image in S3
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Timestamp of when the image was added

    // Navigation property to Business
    public Business.Business? Business { get; set; }
}