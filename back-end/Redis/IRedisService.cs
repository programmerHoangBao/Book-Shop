namespace back_end.Redis
{
    public interface IRedisService
    {
        Task SaveAsync<T>(
            string key,
            T value,
            TimeSpan expiration);

        Task<T?> GetAsync<T>(string key);

        Task DeleteAsync(string key);
    }
}
