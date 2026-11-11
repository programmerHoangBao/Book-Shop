using back_end.DTOs;
using back_end.DTOs.Orders.Requests;
using back_end.DTOs.Orders.Responses;
using back_end.Enums;
using back_end.Exceptions;
using back_end.Kafka.Messages;
using back_end.Kafka.Producers;
using back_end.Records;
using back_end.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace back_end.Services.Implements
{
    public class OrderService : IOrderService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IKafkaProducer _kafkaProducer;

        public OrderService(
            ICurrentUserService currentUserService,
            IKafkaProducer kafkaProducer)
        {
            _currentUserService = currentUserService;
            _kafkaProducer = kafkaProducer;
        }

        public async Task<ApiResponse<object?>> PlaceOrderAsync(
            PlaceOrderRequest request,
            string? idempotencyKey)
        {
            Guid? userId = _currentUserService.GetCurrentUserId();
            if (!userId.HasValue || userId.Value == Guid.Empty)
            {
                throw new BusinessException(ErrorRecord.Forbidden);
            }

            if (request.AddressId == Guid.Empty ||
                !Enum.IsDefined(request.PaymentMethod) ||
                request.PaymentMethod != PaymentMethod.CashOnDelivery ||
                string.IsNullOrWhiteSpace(idempotencyKey) ||
                idempotencyKey.Length > 128)
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            Guid orderId = CreateIdempotentOrderId(userId.Value, idempotencyKey.Trim());
            await _kafkaProducer.ProduceAsync("place-order", new PlaceOrderCommandMessage
            {
                OrderId = orderId,
                UserId = userId.Value,
                AddressId = request.AddressId,
                PaymentMethod = request.PaymentMethod
            });

            PlaceOrderResponse response = new()
            {
                OrderId = orderId
            };

            return ApiResponse<object?>.Response(
                MessageRecord.OrderQueuedSuccessfully,
                data: response);
        }

        private static Guid CreateIdempotentOrderId(Guid userId, string key)
        {
            byte[] input = Encoding.UTF8.GetBytes($"{userId:D}:{key}");
            byte[] hash = SHA256.HashData(input);
            return new Guid(hash.AsSpan(0, 16));
        }
    }
}
