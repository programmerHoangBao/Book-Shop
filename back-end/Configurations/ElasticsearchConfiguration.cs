using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using back_end.Settings;

namespace back_end.Configurations
{
    public static class ElasticsearchConfiguration
    {
        public static IServiceCollection AddElasticsearch(
                this IServiceCollection services,
                IConfiguration configuration
            )
        {
            var setting = configuration
                .GetSection("Elasticsearch")
                .Get<ElasticsearchSetting>()
                ?? throw new InvalidOperationException(
                    "Elasticsearch configuration is missing."
                );

            var settings = new ElasticsearchClientSettings(
                new Uri(setting.Url)
            )
            .Authentication(
                new BasicAuthentication(
                    setting.Username,
                    setting.Password
                )
            )
            .DefaultIndex(setting.DefaultIndex);

            services.AddSingleton(
                new ElasticsearchClient(settings)
            );

            return services;
        }
    }
}
