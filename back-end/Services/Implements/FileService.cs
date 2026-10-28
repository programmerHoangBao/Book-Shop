using Amazon.S3;
using Amazon.S3.Model;
using back_end.DTOs.Files.Responses;
using back_end.Exceptions;
using back_end.Records;
using back_end.Settings;
using Microsoft.Extensions.Options;
using System.IO;
using System.Net;

namespace back_end.Services.Implements
{
    public class FileService : IFileService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly S3Setting _s3Setting;
        private readonly FileStorageSetting _fileStorageSetting;
        private readonly ILogger<FileService> _logger;

        public FileService(
            IAmazonS3 s3Client, 
            IOptions<S3Setting> s3Options,
            IOptions<FileStorageSetting> fileStorageOptions, 
            ILogger<FileService> logger
        )
        {
            _s3Client = s3Client;
            _s3Setting = s3Options.Value;
            _fileStorageSetting = fileStorageOptions.Value;
            _logger = logger;
        }

        public async Task<UploadImageResponse> UploadImageAsync(
            IFormFile? file,
            CancellationToken cancellationToken = default
        )
        {
            // Case 01: File null or empty
            if (file == null || file.Length <= 0)
            {
                throw new BusinessException(ErrorRecord.FileEmpty);
            }

            // Case 02: File too large
            long maxFileSizeBytes = checked(_fileStorageSetting.MaxFileSizeMB * 1024L * 1024L);
            if (file.Length > maxFileSizeBytes)
            {
                throw new BusinessException(ErrorRecord.FileTooLarge);
            }

            // Case 03: 3. Validate Content-Type.
            var contentType = file.ContentType?.Trim();
            if (string.IsNullOrWhiteSpace(contentType) ||
                !_fileStorageSetting.AllowedContentTypes.Contains(
                    contentType,
                    StringComparer.OrdinalIgnoreCase))
            {
                throw new BusinessException(ErrorRecord.InvalidFileExtension);
            }
            var extension = GetExtension(contentType);
            var prefix = _fileStorageSetting.ImagePrefix.Trim('/');
            string objectKey = string.IsNullOrWhiteSpace(prefix)
                ? $"{Guid.NewGuid():N}{extension}"
                : $"{prefix}/{Guid.NewGuid():N}{extension}";

            try
            {
                await using var stream = file.OpenReadStream();

                var request = new PutObjectRequest
                {
                    BucketName = _s3Setting.BucketName,
                    Key = objectKey,
                    InputStream = stream,
                    ContentType = contentType,
                    AutoCloseStream = false
                };

                var response = await _s3Client.PutObjectAsync(
                    request,
                    cancellationToken);

                // Case 04: Confirm the upload of the results..
                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    throw new BusinessException(ErrorRecord.UploadFileFailed);
                }

                var imageUrl = GenerateImageUrl(objectKey);

                return new UploadImageResponse(
                    objectKey: objectKey,
                    imageUrl: imageUrl,
                    contentType: contentType,
                    sizeBytes: file.Length
                );
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(
                    ex,
                    "S3 upload failed. ObjectKey: {ObjectKey}, " +
                    "StatusCode: {StatusCode}",
                    objectKey,
                    ex.StatusCode);

                throw new BusinessException(ErrorRecord.UploadFileFailed);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while uploading image. " +
                    "ObjectKey: {ObjectKey}",
                    objectKey);

                throw new BusinessException(ErrorRecord.UploadFileFailed);
            }
        }

        public Task DeleteImageAsync(string objectKey, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        private string GenerateImageUrl(string objectKey)
        {
            if (string.Equals(
                _s3Setting.AccessMode,
                "PresignedUrl",
                StringComparison.OrdinalIgnoreCase))
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _s3Setting.BucketName,
                    Key = objectKey,
                    Verb = HttpVerb.GET,
                    Expires = DateTime.UtcNow.AddMinutes(
                        _s3Setting.PresignedUrlExpirationMinutes)
                };

                return _s3Client.GetPreSignedURL(request);
            }

            if (string.Equals(
                _s3Setting.AccessMode,
                "StableUrl",
                StringComparison.OrdinalIgnoreCase))
            {
                var encodedKey = string.Join(
                    "/",
                    objectKey.Split('/')
                        .Select(Uri.EscapeDataString));

                return
                    $"https://{_s3Setting.BucketName}.s3." +
                    $"{_s3Setting.Region}.amazonaws.com/{encodedKey}";
            }

            throw new BusinessException(ErrorRecord.UploadFileFailed);
        }
        private static string GetExtension(string contentType)
        {
            return contentType.ToLowerInvariant() switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/webp" => ".webp",
                "image/gif" => ".gif",
                _ => throw new ArgumentException(
                    "Unsupported image content type.",
                    nameof(contentType))
            };
        }
    }
}
