namespace DentalNUB.Api.Services
{
    public interface IImageService
    {
        Task<string> UploadAsync(IFormFile file, string folder);
    }
}