using Amazon.S3;
using Amazon.S3.Model;
using back_end.DTOs.Files.Responses;
using back_end.Exceptions;
using back_end.Records;
using back_end.Settings;
using Microsoft.Extensions.Options;
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

        public async Task<List<UploadImageResponse>> UploadListImagesAsync(
            List<IFormFile> files,
            CancellationToken cancellationToken = default
        )
        {
            // Case 01: Validate files
            if (files == null || files.Count == 0)
            {
                throw new BusinessException(ErrorRecord.FileEmpty);
            }

            try
            {
                // Case 02: Upload all images concurrently
                var uploadTasks = files.Select(file =>
                    UploadImageAsync(file, cancellationToken)
                );

                var results = await Task.WhenAll(uploadTasks);

                return results.ToList();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to upload list of images. FileCount: {FileCount}",
                    files.Count
                );

                throw;
            }
        }

        public async Task DeleteImageAsync(
            string objectKey, 
            CancellationToken cancellationToken = default
        )
        {
            // Case 01: Validate ObjectKey
            if (string.IsNullOrWhiteSpace(objectKey))
            {
                throw new ArgumentException(
                    "ObjectKey is required.",
                    nameof(objectKey));
            }
            objectKey = objectKey.Trim();

            // Case 02: Only allow the deletion of objects belonging to ImagePrefix.
            var prefix = _fileStorageSetting.ImagePrefix.Trim('/');
            if (string.IsNullOrWhiteSpace(prefix))
            {
                throw new InvalidOperationException("ImagePrefix is not configured.");
            }

            var allowedPrefix = prefix + "/";

            if (!objectKey.StartsWith(
                    allowedPrefix,
                    StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "ObjectKey is outside the allowed image prefix.",
                    nameof(objectKey));
            }

            // Case 03: Keys containing path navigation characters are not accepted.
            if (objectKey.Contains('\\') ||
                objectKey.Split('/').Any(segment =>
                    segment is "." or ".."))
            {
                throw new ArgumentException(
                    "ObjectKey is invalid.",
                    nameof(objectKey));
            }

            try
            {
                // Case 04: Delete an object on S3.
                var response = await _s3Client.DeleteObjectAsync(
                    new Amazon.S3.Model.DeleteObjectRequest
                    {
                        BucketName = _s3Setting.BucketName,
                        Key = objectKey
                    },
                    cancellationToken);

                // Case 05: Handle the result. 
                // DeleteObjectAsync typically throws an exception if the request fails. 
                // S3 does not guarantee a 404 response for a non-existent key.
                _logger.LogInformation(
                    "Image deletion request completed. ObjectKey: {ObjectKey}, " +
                    "StatusCode: {StatusCode}",
                    objectKey,
                    response.HttpStatusCode);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Amazon.S3.AmazonS3Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to delete image from S3. ObjectKey: {ObjectKey}, " +
                    "StatusCode: {StatusCode}",
                    objectKey,
                    ex.StatusCode);

                throw new InvalidOperationException(
                    "Failed to delete image from Amazon S3.",
                    ex);
            }
        }

        public async Task DeleteImageByUrlAsync(
            string imageUrl,
            CancellationToken cancellationToken = default
        )
        {
            // Case 01: Validate ImageUrl
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                throw new ArgumentException(
                    "ImageUrl is required.",
                    nameof(imageUrl));
            }

            if (!Uri.TryCreate(imageUrl.Trim(), UriKind.Absolute, out Uri? uri))
            {
                throw new ArgumentException(
                    "ImageUrl is invalid.",
                    nameof(imageUrl));
            }

            // Case 02: Extract ObjectKey from ImageUrl
            string objectKey;

            if (string.Equals(
                _s3Setting.AccessMode,
                "StableUrl",
                StringComparison.OrdinalIgnoreCase))
            {
                string expectedHost =
                    $"{_s3Setting.BucketName}.s3.{_s3Setting.Region}.amazonaws.com";

                if (!string.Equals(
                    uri.Host,
                    expectedHost,
                    StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException(
                        "ImageUrl does not belong to the configured S3 bucket.",
                        nameof(imageUrl));
                }

                objectKey = Uri.UnescapeDataString(
                    uri.AbsolutePath.TrimStart('/'));
            }
            else if (string.Equals(
                _s3Setting.AccessMode,
                "PresignedUrl",
                StringComparison.OrdinalIgnoreCase))
            {
                string expectedHost =
                    $"{_s3Setting.BucketName}.s3.{_s3Setting.Region}.amazonaws.com";

                if (!string.Equals(
                    uri.Host,
                    expectedHost,
                    StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException(
                        "ImageUrl does not belong to the configured S3 bucket.",
                        nameof(imageUrl));
                }

                objectKey = Uri.UnescapeDataString(
                    uri.AbsolutePath.TrimStart('/'));
            }
            else
            {
                throw new InvalidOperationException(
                    "Unsupported S3 AccessMode.");
            }

            // Case 03: Delete image using ObjectKey
            await DeleteImageAsync(objectKey, cancellationToken);
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
