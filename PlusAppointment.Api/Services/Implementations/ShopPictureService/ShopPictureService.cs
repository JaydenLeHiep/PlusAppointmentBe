using PlusAppointment.Models.Classes;
using PlusAppointment.Repositories.Interfaces.ShopPicturesRepo;
using PlusAppointment.Services.Interfaces.BusinessService;
using PlusAppointment.Services.Interfaces.ShopPictureService;

namespace PlusAppointment.Services.Implementations.ShopPictureService;

public class ShopPictureService: IShopPictureService
{
    private readonly IShopPictureRepository _repository;
    private readonly S3Service _s3Service;
    private readonly IBusinessService _businessService;

    public ShopPictureService(IShopPictureRepository repository, S3Service s3Service, IBusinessService businessService)
    {
        _repository = repository;
        _s3Service = s3Service;
        _businessService = businessService;
    }

    public async Task<ShopPicture?> GetPictureAsync(int id)
    {
        return await _repository.GetPictureAsync(id);
    }

    public async Task<IEnumerable<ShopPicture>> GetAllPicturesAsync()
    {
        return await _repository.GetAllPicturesAsync();
    }

    public async Task<IEnumerable<ShopPicture>> GetPicturesByBusinessIdAsync(int businessId)
    {
        return await _repository.GetPicturesByBusinessIdAsync(businessId);
    }

    public async Task<ShopPicture> AddPictureAsync(int businessId, IFormFile image)
    {
        var business = await _businessService.GetBusinessByIdAsync(businessId);

        // Ensure the business name is URL-safe by removing or replacing any invalid characters
        string sanitizedBusinessName = business.Name.Replace(" ", "-").Replace("/", "-").Replace("\\", "-");

        // Construct the file path with business name and ID
        string fileName = $"{sanitizedBusinessName}-{businessId}/{image.FileName}";

        // Upload to S3
        string s3Url = await _s3Service.UploadFileAsync(image.OpenReadStream(), fileName);

        // Save to database
        var shopPicture = new ShopPicture
        {
            BusinessId = businessId,
            S3ImageUrl = s3Url
        };

        return await _repository.AddPictureAsync(shopPicture);
    }


    public async Task<bool> AddPicturesAsync(int businessId, IEnumerable<IFormFile> images)
    {
        var business = await _businessService.GetBusinessByIdAsync(businessId);
    
        // Sanitize the business name to be URL-safe
        string sanitizedBusinessName = business.Name.Replace(" ", "-").Replace("/", "-").Replace("\\", "-");

        var shopPictures = new List<ShopPicture>();

        foreach (var image in images)
        {
            // Construct the file path with business name and ID
            string fileName = $"{sanitizedBusinessName}-{businessId}/{image.FileName}";
            string s3Url = await _s3Service.UploadFileAsync(image.OpenReadStream(), fileName);

            shopPictures.Add(new ShopPicture
            {
                BusinessId = businessId,
                S3ImageUrl = s3Url
            });
        }

        return await _repository.AddPicturesAsync(shopPictures);
    }


    public async Task<bool> DeletePictureAsync(int id)
    {
        // You might also want to delete the picture from S3
        return await _repository.DeletePictureAsync(id);
    }

    public async Task<ShopPicture> UpdatePictureAsync(int id, IFormFile image)
    {
        var existingPicture = await _repository.GetPictureAsync(id);

        if (existingPicture == null)
        {
            return null;
        }

        // Upload new image to S3
        string fileName = $"{existingPicture.BusinessId}/{image.FileName}";
        string s3Url = await _s3Service.UploadFileAsync(image.OpenReadStream(), fileName);

        // Update the record in the database
        existingPicture.S3ImageUrl = s3Url;
        return await _repository.UpdatePictureAsync(existingPicture);
    }
}