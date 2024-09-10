using Amazon.S3;
using Amazon.S3.Model;

namespace PlusAppointment.Services.Implementations.ShopPictureService;

public class S3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName = "plus-appointment-shop-pictures"; // Replace with your bucket name

    public S3Service(IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
    {
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = fileName,  // The file path in the S3 bucket
            InputStream = fileStream,
            ContentType = "image/jpeg",  // Adjust as needed
            CannedACL = S3CannedACL.PublicRead // Make it publicly accessible
        };

        var response = await _s3Client.PutObjectAsync(request);

        // Return the S3 URL of the uploaded file
        return $"https://{_bucketName}.s3.amazonaws.com/{fileName}";
    }

    public async Task<bool> DeleteFileAsync(string fileName)
    {
        var deleteObjectRequest = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = fileName
        };

        var response = await _s3Client.DeleteObjectAsync(deleteObjectRequest);
        return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
    }
}