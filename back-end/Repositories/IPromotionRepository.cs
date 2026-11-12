using back_end.Entities;

namespace back_end.Repositories
{
    public interface IPromotionRepository
    {
        Task<bool> CodeExistsAsync(string code);
        Task<List<Guid>> GetActiveBookIdsAsync(IReadOnlyCollection<Guid> bookIds);
        Task<List<Guid>> GetActiveCategoryIdsAsync(IReadOnlyCollection<Guid> categoryIds);
        Task<PromotionEntity> CreateAsync(PromotionEntity promotion);
    }
}
