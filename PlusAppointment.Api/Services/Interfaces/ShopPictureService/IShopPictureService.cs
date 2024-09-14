using PlusAppointment.Models.Classes;

namespace PlusAppointment.Services.Interfaces.ShopPictureService;

public interface IShopPictureService
{
    Task<ShopPicture?> GetPictureAsync(int id);
    Task<IEnumerable<ShopPicture>> GetAllPicturesAsync();
    Task<IEnumerable<ShopPicture>> GetPicturesByBusinessIdAsync(int businessId);
    Task<ShopPicture> AddPictureAsync(int businessId, IFormFile image);
    Task<bool> AddPicturesAsync(int businessId, IEnumerable<IFormFile> images);
    Task<bool> DeletePictureAsync(int id);
    Task<ShopPicture> UpdatePictureAsync(int id, IFormFile image);
}