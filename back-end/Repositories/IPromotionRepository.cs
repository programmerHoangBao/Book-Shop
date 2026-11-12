using back_end.Entities;

namespace back_end.Repositories
{
    public interface IPromotionRepository
    {
        Task<(bool BookExists, List<PromotionEntity> Promotions, int TotalItems)> GetSuggestionsForBookAsync(
            Guid bookId,
            int pageNumber,
            int pageSize,
            DateTime now);
        Task<bool> CodeExistsAsync(string code);
        Task<List<Guid>> GetActiveBookIdsAsync(IReadOnlyCollection<Guid> bookIds);
        Task<List<Guid>> GetActiveCategoryIdsAsync(IReadOnlyCollection<Guid> categoryIds);
        Task<PromotionEntity> CreateAsync(PromotionEntity promotion);
    }
}
