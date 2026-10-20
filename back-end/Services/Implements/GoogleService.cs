using back_end.DTOs;
using back_end.DTOs.Google.Responses;
using back_end.Records;
using back_end.Settings;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using Microsoft.Extensions.Options;

namespace back_end.Services.Implements
{
    public class GoogleService : IGoogleService
    {
        private readonly GoogleSetting _googleSetting;
        private readonly DriveService _driveService;
        private readonly ILogger<GoogleService> _logger;

        private static readonly HashSet<string> AllowedImageTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "image/jpeg",
                "image/png",
                "image/gif",
                "image/webp",
                "image/bmp"
            };

        [Obsolete]
        public GoogleService(
            IOptions<GoogleSetting> options,
            ILogger<GoogleService> logger)
        {
            _googleSetting = options.Value;
            _logger = logger;

            if (string.IsNullOrWhiteSpace(
                _googleSetting.CredentialsPath))
            {
                throw new InvalidOperationException(
                    "Google CredentialsPath is not configured.");
            }

            string credentialsPath = Path.IsPathRooted(
                _googleSetting.CredentialsPath)
                ? _googleSetting.CredentialsPath
                : Path.Combine(
                    AppContext.BaseDirectory,
                    _googleSetting.CredentialsPath);

            if (!System.IO.File.Exists(credentialsPath))
            {
                throw new FileNotFoundException(
                    "Google Service Account credentials not found!",
                    credentialsPath);
            }

            var credential = GoogleCredential
                .FromFile(credentialsPath)
                .CreateScoped(DriveService.Scope.Drive);

            _driveService = new DriveService(
                new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = _googleSetting.ApplicationName
                });
        }

        public async Task<GoogleUserInfoResponse?> VerifyTokenAsync(
            string idToken)
        {
            try
            {
                GoogleJsonWebSignature.Payload payload =
                    await GoogleJsonWebSignature.ValidateAsync(
                        idToken,
                        new GoogleJsonWebSignature.ValidationSettings
                        {
                            Audience = new[]
                            {
                                _googleSetting.ClientId
                            }
                        });

                return new GoogleUserInfoResponse
                {
                    GoogleId = payload.Subject,
                    Email = payload.Email,
                    Name = payload.Name,
                    AvatarUrl = payload.Picture,
                    EmailVerified = payload.EmailVerified
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task<ApiResponse<GoogleDriveFileResponse?>>
            UploadImageAsync(
                IFormFile file,
                CancellationToken cancellationToken = default)
        {
            // Case 1: Check input file
            if (file == null || file.Length == 0)
            {
                return ApiResponse<GoogleDriveFileResponse?>.Response(
                    errorRecord: ErrorRecord.FileEmpty,
                    data: null
                );
            }

            // Case 2: Validate configuration and file size
            if (_googleSetting.MaxFileSizeMB <= 0)
            {
                _logger.LogError(
                    "Google MaxFileSizeMB must be greater than zero.");

                return ApiResponse<GoogleDriveFileResponse?>.Response(
                    errorRecord: ErrorRecord.UploadFileFailed,
                    data: null
                );
            }

            long maxFileSize = checked(
                (long)_googleSetting.MaxFileSizeMB * 1024L * 1024L);

            if (file.Length > maxFileSize)
            {
                _logger.LogWarning(
                    "File '{FileName}' exceeds the maximum allowed size. " +
                    "Actual: {ActualSize} bytes, Limit: {LimitSize} bytes.",
                    file.FileName,
                    file.Length,
                    maxFileSize);

                return ApiResponse<GoogleDriveFileResponse?>.Response(
                    errorRecord: ErrorRecord.FileTooLarge,
                    data: null
                );
            }

            // Case 3: Validate image MIME type
            string mimeType = file.ContentType?
                .Split(';')[0]
                .Trim() ?? string.Empty;

            if (!AllowedImageTypes.Contains(mimeType))
            {
                _logger.LogWarning(
                    "Invalid image type for file '{FileName}'. " +
                    "Received content type: '{ContentType}'.",
                    file.FileName,
                    mimeType);

                return ApiResponse<GoogleDriveFileResponse?>.Response(
                    errorRecord: ErrorRecord.InvalidFileExtension,
                    data: null
                );
            }

            // Case 4: Validate Google Drive folder
            if (string.IsNullOrWhiteSpace(_googleSetting.FolderId))
            {
                _logger.LogError(
                    "Google Drive FolderId is not configured.");

                return ApiResponse<GoogleDriveFileResponse?>.Response(
                    errorRecord: ErrorRecord.UploadFileFailed,
                    data: null
                );
            }

            string extension = Path.GetExtension(file.FileName);
            string fileName = $"{Guid.NewGuid():N}{extension}";

            var driveFile = new Google.Apis.Drive.v3.Data.File
            {
                Name = fileName,
                MimeType = mimeType,
                Parents = new List<string>
                {
                    _googleSetting.FolderId
                }
            };

            string? uploadedFileId = null;

            try
            {
                // Case 5: Upload image
                await using var stream = file.OpenReadStream();

                var request = _driveService.Files.Create(
                    driveFile,
                    stream,
                    mimeType);

                request.Fields = "id,name,mimeType,size,webViewLink";

                IUploadProgress progress =
                    await request.UploadAsync(cancellationToken);

                if (progress.Status != UploadStatus.Completed)
                {
                    _logger.LogError(
                        progress.Exception,
                        "Google Drive upload failed. Status: {Status}.",
                        progress.Status);

                    return ApiResponse<GoogleDriveFileResponse?>.Response(
                        errorRecord: ErrorRecord.UploadFileFailed,
                        data: null
                    );
                }

                var uploadedFile = request.ResponseBody;

                if (uploadedFile == null ||
                    string.IsNullOrWhiteSpace(uploadedFile.Id))
                {
                    _logger.LogError(
                        "Google Drive upload returned no file ID.");

                    return ApiResponse<GoogleDriveFileResponse?>.Response(
                        errorRecord: ErrorRecord.UploadFileFailed,
                        data: null
                    );
                }

                uploadedFileId = uploadedFile.Id;

                // Case 6: Grant Viewer permission
                if (_googleSetting.ShareWithAnyone)
                {
                    var permission = new Permission
                    {
                        Type = "anyone",
                        Role = "reader"
                    };

                    var permissionRequest =
                        _driveService.Permissions.Create(
                            permission,
                            uploadedFile.Id);

                    permissionRequest.Fields = "id,type,role";

                    await permissionRequest.ExecuteAsync(
                        cancellationToken);
                }

                // Case 7: Return uploaded file information
                var response = new GoogleDriveFileResponse
                {
                    FileId = uploadedFile.Id,
                    FileName = uploadedFile.Name ?? fileName,
                    MimeType = uploadedFile.MimeType ?? mimeType,
                    Size = uploadedFile.Size ?? file.Length,
                    WebViewLink = uploadedFile.WebViewLink
                        ?? $"https://drive.google.com/file/d/{uploadedFile.Id}/view"
                };

                _logger.LogInformation(
                    "Image uploaded successfully. FileId: {FileId}",
                    uploadedFile.Id);

                return ApiResponse<GoogleDriveFileResponse?>.Response(
                    messageRecord: MessageRecord.Success,
                    data: response
                );
            }
            catch (OperationCanceledException)
            {
                // Clean up if the file was uploaded before cancellation.
                if (!string.IsNullOrWhiteSpace(uploadedFileId))
                {
                    await TryDeleteUploadedFileAsync(uploadedFileId);
                }

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to upload image '{FileName}' to Google Drive.",
                    fileName);

                // If upload succeeded but permission creation failed,
                // try to delete the uploaded file.
                if (!string.IsNullOrWhiteSpace(uploadedFileId))
                {
                    await TryDeleteUploadedFileAsync(uploadedFileId);
                }

                return ApiResponse<GoogleDriveFileResponse?>.Response(
                    errorRecord: ErrorRecord.UploadFileFailed,
                    data: null
                );
            }
        }

        private async Task TryDeleteUploadedFileAsync(string fileId)
        {
            try
            {
                await _driveService.Files
                    .Delete(fileId)
                    .ExecuteAsync(CancellationToken.None);

                _logger.LogInformation(
                    "Cleaned up uploaded file. FileId: {FileId}",
                    fileId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to clean up uploaded file. FileId: {FileId}",
                    fileId);
            }
        }

        public async Task<bool> DeleteImageAsync(
            string uploadedFileId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(uploadedFileId))
            {
                _logger.LogWarning(
                    "Cannot delete Google Drive file: FileId is empty.");

                return false;
            }

            try
            {
                await _driveService.Files
                    .Delete(uploadedFileId)
                    .ExecuteAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully deleted Google Drive file. " +
                    "FileId: {FileId}",
                    uploadedFileId);

                return true;
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to delete Google Drive file. FileId: {FileId}",
                    uploadedFileId);

                return false;
            }
        }
    }
}