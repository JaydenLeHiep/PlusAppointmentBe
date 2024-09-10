using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.ShopPicturesRepo;

namespace PlusAppointment.Repositories.Implementation.ShopPicturesRepo;

public class ShopPictureRepository : IShopPictureRepository
{
    private readonly ApplicationDbContext _context;

    public ShopPictureRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ShopPicture?> GetPictureAsync(int id)
    {
        return await _context.ShopPictures.FindAsync(id);
    }

    public async Task<IEnumerable<ShopPicture>> GetAllPicturesAsync()
    {
        return await _context.ShopPictures.ToListAsync();
    }

    public async Task<IEnumerable<ShopPicture>> GetPicturesByBusinessIdAsync(int businessId)
    {
        return await _context.ShopPictures.Where(p => p.BusinessId == businessId).ToListAsync();
    }

    public async Task<ShopPicture> AddPictureAsync(ShopPicture picture)
    {
        _context.ShopPictures.Add(picture);
        await _context.SaveChangesAsync();
        return picture;
    }

    public async Task<bool> AddPicturesAsync(IEnumerable<ShopPicture> pictures)
    {
        await _context.ShopPictures.AddRangeAsync(pictures);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeletePictureAsync(int id)
    {
        var picture = await GetPictureAsync(id);
        if (picture != null)
        {
            _context.ShopPictures.Remove(picture);
            return await _context.SaveChangesAsync() > 0;
        }
        return false;
    }

    public async Task<ShopPicture> UpdatePictureAsync(ShopPicture picture)
    {
        _context.ShopPictures.Update(picture);
        await _context.SaveChangesAsync();
        return picture;
    }
}