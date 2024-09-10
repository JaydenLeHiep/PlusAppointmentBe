using PlusAppointment.Models.Classes;

namespace PlusAppointment.Repositories.Interfaces.ShopPicturesRepo;

public interface IShopPictureRepository
{
    Task<ShopPicture?> GetPictureAsync(int id);
    Task<IEnumerable<ShopPicture>> GetAllPicturesAsync();
    Task<IEnumerable<ShopPicture>> GetPicturesByBusinessIdAsync(int businessId);
    Task<ShopPicture> AddPictureAsync(ShopPicture picture);
    Task<bool> AddPicturesAsync(IEnumerable<ShopPicture> pictures);
    Task<bool> DeletePictureAsync(int id);
    Task<ShopPicture> UpdatePictureAsync(ShopPicture picture);
}
