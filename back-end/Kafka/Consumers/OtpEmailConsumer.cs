using AutoMapper;
using back_end.DTOs.Emails.Requests;
using back_end.Kafka.Messages;
using back_end.Services;
using back_end.Settings;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace back_end.Kafka.Consumers
{
    public class OtpEmailConsumer : BackgroundService
    {
        private readonly KafkaSetting _setting;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OtpEmailConsumer> _logger;

        public OtpEmailConsumer(
            IOptions<KafkaSetting> options,
            IServiceScopeFactory scopeFactory,
            ILogger<OtpEmailConsumer> logger
        )
        {
            _setting = options.Value;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            // Cho ASP.NET hoàn thành startup trước
            await Task.Yield();

            _logger.LogInformation(
                "OTP email consumer starting. Kafka: {BootstrapServers}",
                _setting.BootstrapServers
            );

            var config = new ConsumerConfig
            {
                BootstrapServers = _setting.BootstrapServers,
                GroupId = "otp-email-consumer",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            using var consumer =
                new ConsumerBuilder<Ignore, string>(config).Build();

            consumer.Subscribe("send-otp-email");

            _logger.LogInformation(
                "Subscribed to Kafka topic: send-otp-email"
            );

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        _logger.LogInformation(
                            "Waiting for OTP email message..."
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

                        SendOtpEmailMessage? message;

                        try
                        {
                            message =
                                JsonSerializer.Deserialize<SendOtpEmailMessage>(
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

                        var emailService =
                            scope.ServiceProvider
                                .GetRequiredService<IEmailService>();

                        var sendOtpRequest = new SendOtpRequest
                        {
                            ToEmail = message.ToEmail,
                            Otp = message.Otp,
                            Name = message.Name,
                            OtpExpirySeconds =
                                message.OtpExpirySeconds
                        };

                        await emailService.SendOtpEmailAsync(
                            sendOtpRequest
                        );

                        consumer.Commit(result);

                        _logger.LogInformation(
                            "OTP email sent successfully to {Email}.",
                            message.ToEmail
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
                            "Error while processing OTP email message."
                        );
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation(
                    "OTP email consumer cancellation requested."
                );
            }
            finally
            {
                consumer.Close();

                _logger.LogInformation(
                    "OTP email consumer stopped."
                );
            }
        }
    }
}
