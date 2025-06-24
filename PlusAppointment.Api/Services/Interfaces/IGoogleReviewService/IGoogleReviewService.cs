namespace PlusAppointment.Services.Interfaces.IGoogleReviewService;

public interface IGoogleReviewService
{
    Task<string> GetReviewsAsync();
    Task<byte[]> GetAvatarAsync(string url);
}