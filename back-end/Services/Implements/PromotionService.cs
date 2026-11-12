using back_end.DTOs;
using back_end.DTOs.Promotions.Requests;
using back_end.DTOs.Promotions.Responses;
using back_end.Entities;
using back_end.Enums;
using back_end.Exceptions;
using back_end.Records;
using back_end.Repositories;

namespace back_end.Services.Implements
{
    public class PromotionService : IPromotionService
    {
        private readonly IPromotionRepository _promotionRepository;

        public PromotionService(IPromotionRepository promotionRepository)
        {
            _promotionRepository = promotionRepository;
        }

        public async Task<ApiResponse<PromotionResponse>> CreateAsync(CreatePromotionRequest request)
        {
            string name = request.Name?.Trim() ?? string.Empty;
            string? code = string.IsNullOrWhiteSpace(request.Code)
                ? null
                : request.Code.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(name) || name.Length > 255 ||
                (code != null && code.Length > 6) ||
                !Enum.IsDefined(request.Type) ||
                request.DiscountValue < 0 || request.MinimumOrderValue < 0 ||
                request.UsageLimit < 0 || request.StartAt >= request.EndAt ||
                request.BookIds == null || request.CategoryIds == null ||
                request.BookIds.Any(id => id == Guid.Empty) ||
                request.CategoryIds.Any(id => id == Guid.Empty) ||
                request.BookIds.Distinct().Count() != request.BookIds.Count ||
                request.CategoryIds.Distinct().Count() != request.CategoryIds.Count)
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            if ((request.Type == PromotionType.PercentageDiscount &&
                    (request.DiscountValue <= 0 || request.DiscountValue > 100)) ||
                (request.Type == PromotionType.FixedAmountDiscount && request.DiscountValue <= 0) ||
                (request.Type == PromotionType.FreeShipping && request.DiscountValue != 0))
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            if (code != null && await _promotionRepository.CodeExistsAsync(code))
            {
                throw new BusinessException(ErrorRecord.PromotionCodeExists);
            }

            List<Guid> validBookIds = await _promotionRepository.GetActiveBookIdsAsync(request.BookIds);
            List<Guid> validCategoryIds = await _promotionRepository.GetActiveCategoryIdsAsync(request.CategoryIds);
            if (validBookIds.Count != request.BookIds.Count ||
                validCategoryIds.Count != request.CategoryIds.Count)
            {
                throw new BusinessException(ErrorRecord.NotFound);
            }

            PromotionEntity promotion = new()
            {
                Code = code,
                Name = name,
                Type = request.Type,
                DiscountValue = request.DiscountValue,
                MinimumOrderValue = request.MinimumOrderValue,
                StartAt = request.StartAt,
                EndAt = request.EndAt,
                UsageLimit = request.UsageLimit,
                FreeShipping = request.FreeShipping || request.Type == PromotionType.FreeShipping,
                IsActive = request.IsActive,
                PromotionBooks = validBookIds.Select(bookId => new PromotionBookEntity
                {
                    BookId = bookId
                }).ToList(),
                PromotionCatatogies = validCategoryIds.Select(categoryId => new PromotionCatatogyEntity
                {
                    CategoryId = categoryId
                }).ToList()
            };

            await _promotionRepository.CreateAsync(promotion);

            PromotionResponse response = new()
            {
                Id = promotion.Id,
                Code = promotion.Code,
                Name = promotion.Name,
                Type = promotion.Type,
                DiscountValue = promotion.DiscountValue,
                MinimumOrderValue = promotion.MinimumOrderValue,
                StartAt = promotion.StartAt,
                EndAt = promotion.EndAt,
                UsageLimit = promotion.UsageLimit,
                UsageCount = promotion.UsageCount,
                FreeShipping = promotion.FreeShipping,
                IsActive = promotion.IsActive,
                CreatedAt = promotion.CreatedAt,
                BookIds = validBookIds,
                CategoryIds = validCategoryIds
            };

            return ApiResponse<PromotionResponse>.Response(MessageRecord.CreatedSuccessfully, data: response);
        }
    }
}
