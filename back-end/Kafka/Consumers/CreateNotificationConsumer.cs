using back_end.Data;
using back_end.Entities;
using back_end.Kafka.Messages;
using back_end.Settings;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace back_end.Kafka.Consumers
{
    public class CreateNotificationConsumer : BackgroundService
    {
        private readonly KafkaSetting _setting;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CreateNotificationConsumer> _logger;

        public CreateNotificationConsumer(
            IOptions<KafkaSetting> options,
            IServiceScopeFactory scopeFactory,
            ILogger<CreateNotificationConsumer> logger)
        {
            _setting = options.Value;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();

            var config = new ConsumerConfig
            {
                BootstrapServers = _setting.BootstrapServers,
                GroupId = "create-notification-consumer",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe("create-notification");

            _logger.LogInformation("Subscribed to Kafka topic: create-notification");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = consumer.Consume(stoppingToken);
                        if (string.IsNullOrWhiteSpace(result?.Message?.Value))
                        {
                            continue;
                        }

                        CreateNotificationMessage? message;
                        try
                        {
                            message = JsonSerializer.Deserialize<CreateNotificationMessage>(
                                result.Message.Value);
                        }
                        catch (JsonException exception)
                        {
                            _logger.LogError(exception, "Invalid create-notification message.");
                            consumer.Commit(result);
                            continue;
                        }

                        if (!IsValid(message))
                        {
                            _logger.LogError("Invalid create-notification payload received.");
                            consumer.Commit(result);
                            continue;
                        }

                        using IServiceScope scope = _scopeFactory.CreateScope();
                        DatabaseContext context = scope.ServiceProvider
                            .GetRequiredService<DatabaseContext>();

                        if (await context.Notifications.AnyAsync(
                            notification => notification.Id == message!.NotificationId,
                            stoppingToken))
                        {
                            consumer.Commit(result);
                            continue;
                        }

                        bool userExists = await context.Users.AnyAsync(user =>
                            user.Id == message!.UserId && !user.IsDeleted && user.Enabled,
                            stoppingToken);
                        if (!userExists)
                        {
                            _logger.LogWarning(
                                "Dropping notification {NotificationId}: recipient {UserId} is unavailable.",
                                message!.NotificationId,
                                message.UserId);
                            consumer.Commit(result);
                            continue;
                        }

                        context.Notifications.Add(new NotificationEntity
                        {
                            Id = message!.NotificationId,
                            UserId = message.UserId,
                            Type = message.Type,
                            Title = message.Title,
                            Content = message.Content,
                            Channel = message.Channel
                        });
                        await context.SaveChangesAsync(stoppingToken);
                        consumer.Commit(result);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(exception, "Error while creating notification from Kafka message.");
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Create-notification consumer cancellation requested.");
            }
            finally
            {
                consumer.Close();
                _logger.LogInformation("Create-notification consumer stopped.");
            }
        }

        private static bool IsValid(CreateNotificationMessage? message) =>
            message != null &&
            message.NotificationId != Guid.Empty &&
            message.UserId != Guid.Empty &&
            Enum.IsDefined(message.Type) &&
            Enum.IsDefined(message.Channel) &&
            !string.IsNullOrWhiteSpace(message.Title) &&
            message.Title.Length <= 500 &&
            !string.IsNullOrWhiteSpace(message.Content);
    }
}
