using Microsoft.EntityFrameworkCore;
using PlusAppointment.Data;
using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.ShopPicturesRepo;
using PlusAppointment.Utils.Redis;

namespace PlusAppointment.Repositories.Implementation.ShopPicturesRepo
{
    public class ShopPictureRepository : IShopPictureRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly RedisHelper _redisHelper;

        public ShopPictureRepository(ApplicationDbContext context, RedisHelper redisHelper)
        {
            _context = context;
            _redisHelper = redisHelper;
        }

        public async Task<ShopPicture?> GetPictureAsync(int id)
        {
            string cacheKey = $"shop_picture_{id}";
            var cachedPicture = await _redisHelper.GetCacheAsync<ShopPicture>(cacheKey);
            if (cachedPicture != null)
            {
                return cachedPicture;
            }

            var picture = await _context.ShopPictures.FindAsync(id);
            if (picture == null)
            {
                throw new KeyNotFoundException($"ShopPicture with ID {id} not found");
            }

            await _redisHelper.SetCacheAsync(cacheKey, picture, TimeSpan.FromMinutes(10));
            return picture;
        }

        public async Task<IEnumerable<ShopPicture>> GetAllPicturesAsync()
        {
            const string cacheKey = "all_shop_pictures";
            var cachedPictures = await _redisHelper.GetCacheAsync<List<ShopPicture>>(cacheKey);

            if (cachedPictures != null && cachedPictures.Any())
            {
                return cachedPictures;
            }

            var pictures = await _context.ShopPictures.ToListAsync();
            await _redisHelper.SetCacheAsync(cacheKey, pictures, TimeSpan.FromMinutes(10));

            return pictures;
        }

        public async Task<IEnumerable<ShopPicture>> GetPicturesByBusinessIdAsync(int businessId)
        {
            string cacheKey = $"shop_pictures_business_{businessId}";
            var cachedPictures = await _redisHelper.GetCacheAsync<List<ShopPicture>>(cacheKey);

            if (cachedPictures != null && cachedPictures.Any())
            {
                return cachedPictures.OrderBy(p => p.ShopPictureId);
            }

            var pictures = await _context.ShopPictures
                .Where(p => p.BusinessId == businessId)
                .OrderBy(p => p.ShopPictureId)
                .ToListAsync();

            await _redisHelper.SetCacheAsync(cacheKey, pictures, TimeSpan.FromMinutes(10));

            return pictures;
        }

        public async Task<ShopPicture> AddPictureAsync(ShopPicture picture)
        {
            _context.ShopPictures.Add(picture);
            await _context.SaveChangesAsync();

            // Refresh caches after adding a new picture
            await UpdateShopPictureCacheAsync(picture);
            await RefreshRelatedCachesAsync(picture.BusinessId);

            return picture;
        }

        public async Task<bool> AddPicturesAsync(IEnumerable<ShopPicture> pictures)
        {
            var shopPictures = pictures as ShopPicture[] ?? pictures.ToArray();
            await _context.ShopPictures.AddRangeAsync(shopPictures);
            var saved = await _context.SaveChangesAsync() > 0;

            if (saved)
            {
                foreach (var picture in shopPictures)
                {
                    await UpdateShopPictureCacheAsync(picture);
                }

                var businessId = shopPictures.First().BusinessId;
                await RefreshRelatedCachesAsync(businessId);
            }

            return saved;
        }

        public async Task<bool> DeletePictureAsync(int id)
        {
            var picture = await GetPictureAsync(id);
            if (picture != null)
            {
                _context.ShopPictures.Remove(picture);
                var result = await _context.SaveChangesAsync() > 0;

                if (result)
                {
                    await InvalidateShopPictureCacheAsync(picture);
                    await RefreshRelatedCachesAsync(picture.BusinessId);
                }

                return result;
            }
            return false;
        }

        public async Task<ShopPicture> UpdatePictureAsync(ShopPicture picture)
        {
            _context.ShopPictures.Update(picture);
            await _context.SaveChangesAsync();

            // Refresh caches after updating the picture
            await UpdateShopPictureCacheAsync(picture);
            await RefreshRelatedCachesAsync(picture.BusinessId);

            return picture;
        }

        private async Task UpdateShopPictureCacheAsync(ShopPicture picture)
        {
            var cacheKey = $"shop_picture_{picture.ShopPictureId}";
            await _redisHelper.SetCacheAsync(cacheKey, picture, TimeSpan.FromMinutes(10));

            await _redisHelper.UpdateListCacheAsync<ShopPicture>(
                $"shop_pictures_business_{picture.BusinessId}",
                list =>
                {
                    int index = list.FindIndex(p => p.ShopPictureId == picture.ShopPictureId);
                    if (index != -1)
                    {
                        list[index] = picture;
                    }
                    else
                    {
                        list.Add(picture);
                    }

                    return list.OrderBy(p => p.ShopPictureId).ToList();
                },
                TimeSpan.FromMinutes(10));
        }

        private async Task InvalidateShopPictureCacheAsync(ShopPicture picture)
        {
            var cacheKey = $"shop_picture_{picture.ShopPictureId}";
            await _redisHelper.DeleteCacheAsync(cacheKey);

            await _redisHelper.RemoveFromListCacheAsync<ShopPicture>(
                $"shop_pictures_business_{picture.BusinessId}",
                list =>
                {
                    list.RemoveAll(p => p.ShopPictureId == picture.ShopPictureId);
                    return list;
                },
                TimeSpan.FromMinutes(10));
        }

        private async Task RefreshRelatedCachesAsync(int businessId)
        {
            // Refresh list of all shop pictures for the business
            string businessCacheKey = $"shop_pictures_business_{businessId}";
            var businessPictures = await _context.ShopPictures
                .Where(p => p.BusinessId == businessId)
                .ToListAsync();

            await _redisHelper.SetCacheAsync(businessCacheKey, businessPictures, TimeSpan.FromMinutes(10));

            // Optionally refresh the cache for all shop pictures if required
            const string allPicturesCacheKey = "all_shop_pictures";
            var allPictures = await _context.ShopPictures.ToListAsync();
            await _redisHelper.SetCacheAsync(allPicturesCacheKey, allPictures, TimeSpan.FromMinutes(10));
        }
    }
}
