using back_end.Services;
using back_end.Settings;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace back_end.Kafka.Consumers
{
    public class DeleteFileConsumer : BackgroundService
    {
        private readonly KafkaSetting _setting;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DeleteFileConsumer> _logger;

        public DeleteFileConsumer(
            IOptions<KafkaSetting> options,
            IServiceScopeFactory scopeFactory,
            ILogger<DeleteFileConsumer> logger
        )
        {
            _setting = options.Value;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            // Allow ASP.NET to complete its startup process first.
            await Task.Yield();

            _logger.LogInformation(
                "Delete file consumer starting. Kafka: {BootstrapServers}",
                _setting.BootstrapServers
            );

            var config = new ConsumerConfig
            {
                BootstrapServers = _setting.BootstrapServers,
                GroupId = "delete-file-consumer",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            using var consumer =
                new ConsumerBuilder<Ignore, string>(config).Build();

            consumer.Subscribe("delete-file");

            _logger.LogInformation(
                "Subscribed to Kafka topic: delete-file-consumer"
            );

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = consumer.Consume(stoppingToken);

                        if (result?.Message?.Value == null)
                        {
                            _logger.LogWarning(
                                "Received empty Kafka message."
                            );

                            continue;
                        }

                        string? imageUrl = JsonSerializer.Deserialize<string>(
                            result.Message.Value
                        );

                        if (string.IsNullOrWhiteSpace(imageUrl))
                        {
                            _logger.LogWarning("Image URL is empty.");
                            consumer.Commit(result);
                            continue;
                        }
                        _logger.LogInformation(
                            "Received Kafka message: {Message}",
                            imageUrl
                        );

                        using var scope =
                            _scopeFactory.CreateScope();

                        var fileService =
                            scope.ServiceProvider
                                .GetRequiredService<IFileService>();

                        await fileService.DeleteImageByUrlAsync(imageUrl: imageUrl);

                        consumer.Commit(result);
                    }
                    catch (OperationCanceledException)
                        when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Error while processing deleted file message."
                        );
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation(
                    "Delete file consumer cancellation requested."
                );
            }
            finally
            {
                consumer.Close();

                _logger.LogInformation(
                    "Delete file consumer stopped."
                );
            }
        }
    }
}
