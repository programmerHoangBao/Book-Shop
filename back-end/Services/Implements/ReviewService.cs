using back_end.DTOs;
using back_end.DTOs.Reviews.Requests;
using back_end.DTOs.Reviews.Responses;
using back_end.Entities;
using back_end.Exceptions;
using back_end.Records;
using back_end.Repositories;

namespace back_end.Services.Implements
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly ICurrentUserService _currentUserService;

        public ReviewService(
            IReviewRepository reviewRepository,
            ICurrentUserService currentUserService)
        {
            _reviewRepository = reviewRepository;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<object?>> CreateReviewAsync(
            CreateReviewRequest request)
        {
            Guid? userId = _currentUserService.GetCurrentUserId();
            if (userId == null)
            {
                throw new BusinessException(ErrorRecord.Forbidden);
            }

            if (request.BookId == Guid.Empty || request.Rating > 5)
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            if (
                !await _reviewRepository.HasCompletedPurchaseAsync(userId.Value, request.BookId, request.OrderId)
                )
            {
                throw new BusinessException(ErrorRecord.BookPurchaseRequired);
            }

            if (await _reviewRepository.ReviewExistsAsync(userId.Value, request.BookId, request.OrderId))
            {
                throw new BusinessException(ErrorRecord.ReviewAlreadyExists);
            }

            List<string> imageUrls = (request.ImageUrls ?? new List<string>())
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Select(url => url.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            ReviewEntity review = new()
            {
                UserId = userId.Value,
                BookId = request.BookId,
                OrderId = request.OrderId,
                Ratting = request.Rating,
                Comment = request.Comment?.Trim(),
                Images = imageUrls.Select(url => new ReviewImageEntity { ImageUrl = url }).ToList()
            };

            if (!await _reviewRepository.AddReviewAsync(review))
            {
                throw new BusinessException(ErrorRecord.Failed);
            }

            CreateReviewResponse response = new()
            {
                Id = review.Id,
                BookId = review.BookId,
                OrderId = review.OrderId,
                Rating = review.Ratting,
                Comment = review.Comment,
                ImageUrls = imageUrls,
                CreatedAt = review.CreatedAt
            };

            return ApiResponse<object?>.Response(MessageRecord.CreatedSuccessfully, data: response);
        }
    }
}
