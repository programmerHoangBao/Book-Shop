using back_end.DTOs.Google.Responses;
using back_end.DTOs;

namespace back_end.Services
{
    public interface IGoogleService
    {
        Task<GoogleUserInfoResponse?> VerifyTokenAsync(string idToken);
        Task<ApiResponse<GoogleDriveFileResponse?>> UploadImageAsync(
            IFormFile file,
            CancellationToken cancellationToken = default
        );
        Task<bool> DeleteImageAsync(
            string uploadedFileId, 
            CancellationToken cancellationToken = default
        );
    }
}
