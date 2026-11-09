using AutoMapper;
using back_end.DTOs.Emails.Requests;
using back_end.Entities;
using back_end.Kafka.Messages;
using back_end.Services;
using back_end.Settings;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace back_end.Kafka.Consumers
{
    public class IndexBookElasticsearchConsumer : BackgroundService
    {
        private readonly KafkaSetting _setting;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMapper _mapper;
        private readonly ILogger<IndexBookElasticsearchConsumer> _logger;

        public IndexBookElasticsearchConsumer(
            IOptions<KafkaSetting> options,
            IServiceScopeFactory scopeFactory, 
            IMapper mapper,
            ILogger<IndexBookElasticsearchConsumer> logger
        )
        {
            _setting = options.Value;
            _scopeFactory = scopeFactory;
            _mapper = mapper;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Allow ASP.NET to complete its startup process first.
            await Task.Yield();

            _logger.LogInformation(
                "Create index for book entity on Elasticsearch " +
                "consumer starting. Kafka: {BootstrapServers}",
                _setting.BootstrapServers
            );

            var config = new ConsumerConfig
            {
                BootstrapServers = _setting.BootstrapServers,
                GroupId = "index-book-elasticsearch-consumer",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            using var consumer =
                new ConsumerBuilder<Ignore, string>(config).Build();

            consumer.Subscribe("index-book-elasticsearch");

            _logger.LogInformation(
                "Subscribed to Kafka topic: index-book-elasticsearch-consumer"
            );

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        _logger.LogInformation(
                            "Waiting for index " +
                            "for book entity on Elasticsearch message..."
                        );

                        var result = consumer.Consume(stoppingToken);

                        if (result?.Message?.Value == null)
                        {
                            _logger.LogWarning(
                                "Received empty Kafka message."
                            );

                            continue;
                        }

                        _logger.LogInformation(
                            "Received Kafka message: {Message}",
                            result.Message.Value
                        );

                        BookDocumentMessage? message;

                        try
                        {
                            message =
                                JsonSerializer.Deserialize<BookDocumentMessage>(
                                    result.Message.Value
                                );
                        }
                        catch (JsonException ex)
                        {
                            _logger.LogError(
                                ex,
                                "Invalid JSON message: {Message}",
                                result.Message.Value
                            );

                            consumer.Commit(result);

                            continue;
                        }

                        if (message == null)
                        {
                            _logger.LogError(
                                "Kafka message could not be deserialized."
                            );

                            consumer.Commit(result);

                            continue;
                        }

                        using var scope =
                            _scopeFactory.CreateScope();

                        var bookElasticsearchService =
                            scope.ServiceProvider
                                .GetRequiredService<IBookElasticsearchService>();

                        BookEntity bookEntity = _mapper.Map<BookEntity>(message);

                        await bookElasticsearchService.IndexBookAsync(bookEntity);

                        consumer.Commit(result);

                        _logger.LogInformation(
                            "Index for book entity on Elasticsearch successfully to bookId={Id}.",
                            message.Id
                        );
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
                            "Error while processing index for book entity on Elasticsearch."
                        );
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation(
                    "Index for book entity on Elasticsearch consumer cancellation requested."
                );
            }
            finally
            {
                consumer.Close();

                _logger.LogInformation(
                    "Index for book entity on Elasticsearch consumer stopped."
                );
            }
        }
    }
}
