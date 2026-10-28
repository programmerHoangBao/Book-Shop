using back_end.DTOs.Files.Responses;

namespace back_end.Services
{
    public interface IFileService
    {
        Task<UploadImageResponse> UploadImageAsync(
            IFormFile? file, 
            CancellationToken cancellationToken = default
        );

        Task DeleteImageAsync(
            string objectKey,
            CancellationToken cancellationToken = default
        );
    }
}
