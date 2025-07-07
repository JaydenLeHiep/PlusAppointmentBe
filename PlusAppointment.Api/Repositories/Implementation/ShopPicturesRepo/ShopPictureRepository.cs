using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.ShopPicturesRepo;

namespace PlusAppointment.Repositories.Implementation.ShopPicturesRepo
{
    public class ShopPictureRepository : IShopPictureRepository
    {
        private readonly ApplicationDbContext _context;

        public ShopPictureRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ShopPicture?> GetPictureAsync(int id)
        {
            var picture = await _context.ShopPictures.FindAsync(id);
            if (picture == null)
            {
                throw new KeyNotFoundException($"ShopPicture with ID {id} not found");
            }
            return picture;
        }

        public async Task<IEnumerable<ShopPicture>> GetAllPicturesAsync()
        {
            var pictures = await _context.ShopPictures.ToListAsync();

            return pictures;
        }

        public async Task<IEnumerable<ShopPicture>> GetPicturesByBusinessIdAsync(int businessId)
        {
            var pictures = await _context.ShopPictures
                .Where(p => p.BusinessId == businessId)
                .OrderBy(p => p.ShopPictureId)
                .ToListAsync();

            return pictures;
        }

        public async Task<ShopPicture> AddPictureAsync(ShopPicture picture)
        {
            _context.ShopPictures.Add(picture);
            await _context.SaveChangesAsync();
            return picture;
        }

        public async Task<bool> AddPicturesAsync(IEnumerable<ShopPicture> pictures)
        {
            var shopPictures = pictures as ShopPicture[] ?? pictures.ToArray();
            await _context.ShopPictures.AddRangeAsync(shopPictures);
            var saved = await _context.SaveChangesAsync() > 0;
            
            return saved;
        }

        public async Task<bool> DeletePictureAsync(int id)
        {
            var picture = await GetPictureAsync(id);
            if (picture != null)
            {
                _context.ShopPictures.Remove(picture);
                var result = await _context.SaveChangesAsync() > 0;

                return result;
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
}
