namespace back_end.Repositories
{
    public interface ICategoryRepository
    {
        Task<bool> CategoryExistsAsync(Guid categoryId);
    }
}
