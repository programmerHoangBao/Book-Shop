using back_end.Enums;
using back_end.Kafka.Messages;
using back_end.Kafka.Producers;
using back_end.Repositories;
using back_end.Settings;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace back_end.Kafka.Consumers
{
    public class PlaceOrderConsumer : BackgroundService
    {
        private readonly KafkaSetting _setting;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IKafkaProducer _kafkaProducer;
        private readonly ILogger<PlaceOrderConsumer> _logger;

        public PlaceOrderConsumer(
            IOptions<KafkaSetting> options,
            IServiceScopeFactory scopeFactory,
            IKafkaProducer kafkaProducer,
            ILogger<PlaceOrderConsumer> logger)
        {
            _setting = options.Value;
            _scopeFactory = scopeFactory;
            _kafkaProducer = kafkaProducer;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();

            var config = new ConsumerConfig
            {
                BootstrapServers = _setting.BootstrapServers,
                GroupId = "place-order-consumer",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe("place-order");
            _logger.LogInformation("Subscribed to Kafka topic: place-order");

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

                        PlaceOrderCommandMessage? command;
                        try
                        {
                            command = JsonSerializer.Deserialize<PlaceOrderCommandMessage>(
                                result.Message.Value);
                        }
                        catch (JsonException exception)
                        {
                            _logger.LogError(exception, "Invalid place-order Kafka message.");
                            consumer.Commit(result);
                            continue;
                        }

                        if (!IsValid(command))
                        {
                            _logger.LogError("Invalid place-order command received.");
                            consumer.Commit(result);
                            continue;
                        }

                        while (!stoppingToken.IsCancellationRequested)
                        {
                            try
                            {
                                await ProcessOrderCommandAsync(command!, stoppingToken);
                                consumer.Commit(result);
                                break;
                            }
                            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                            {
                                throw;
                            }
                            catch (Exception exception)
                            {
                                _logger.LogError(
                                    exception,
                                    "Failed to process place-order message {OrderId}; retrying before reading the next message.",
                                    command!.OrderId);
                                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                            }
                        }
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(exception, "Failed to process place-order Kafka message.");
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Place-order consumer cancellation requested.");
            }
            finally
            {
                consumer.Close();
                _logger.LogInformation("Place-order consumer stopped.");
            }
        }

        private Task PublishFailureNotificationAsync(
            PlaceOrderCommandMessage command,
            PlaceOrderResult result)
        {
            string content = result switch
            {
                PlaceOrderResult.AddressNotFound => "The selected delivery address is unavailable. Please select another address and try again.",
                PlaceOrderResult.CartEmpty => "Your cart is empty, so the order could not be placed.",
                PlaceOrderResult.BookUnavailable => "One or more books in your cart are unavailable. Please review your cart and try again.",
                PlaceOrderResult.InsufficientStock => "There is not enough stock to fulfill your cart. Please review the quantities and try again.",
                _ => "The order could not be placed. Please try again."
            };

            return _kafkaProducer.ProduceAsync("create-notification", new CreateNotificationMessage
            {
                NotificationId = CreateFailureNotificationId(command.OrderId, result),
                UserId = command.UserId,
                Type = NotificationType.Warning,
                Title = "Order could not be placed",
                Content = content,
                Channel = NotificationChannel.InApp
            });
        }

        private async Task ProcessOrderCommandAsync(
            PlaceOrderCommandMessage command,
            CancellationToken cancellationToken)
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            IOrderRepository orderRepository = scope.ServiceProvider
                .GetRequiredService<IOrderRepository>();
            var (placementResult, order) = await orderRepository.PlaceOrderAsync(
                command.UserId,
                command.AddressId,
                command.PaymentMethod,
                command.OrderId);

            if (placementResult is PlaceOrderResult.Created or PlaceOrderResult.Duplicate)
            {
                if (order == null)
                {
                    throw new InvalidOperationException("Order repository returned no order.");
                }

                await _kafkaProducer.ProduceAsync("order-placed", new OrderPlacedMessage
                {
                    OrderId = order.Id,
                    UserId = order.UserId,
                    OrderCode = order.OrderCode,
                    TotalAmount = order.TotalAmount,
                    OrderedAt = order.OrderedAt
                });
                return;
            }

            await PublishFailureNotificationAsync(command, placementResult);
        }

        private static bool IsValid(PlaceOrderCommandMessage? command) =>
            command != null &&
            command.OrderId != Guid.Empty &&
            command.UserId != Guid.Empty &&
            command.AddressId != Guid.Empty &&
            command.PaymentMethod == PaymentMethod.CashOnDelivery;

        private static Guid CreateFailureNotificationId(Guid orderId, PlaceOrderResult result)
        {
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{orderId:D}:{result}"));
            return new Guid(hash.AsSpan(0, 16));
        }
    }
}
