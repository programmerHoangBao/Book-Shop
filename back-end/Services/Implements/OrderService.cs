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
        private readonly IOrderRepository _orderRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IKafkaProducer _kafkaProducer;

        public OrderService(
            IOrderRepository orderRepository,
            ICurrentUserService currentUserService,
            IKafkaProducer kafkaProducer)
        {
            _orderRepository = orderRepository;
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
            var (result, order) = await _orderRepository.PlaceOrderAsync(
                userId.Value,
                request.AddressId,
                request.PaymentMethod,
                orderId);

            switch (result)
            {
                case PlaceOrderResult.AddressNotFound:
                    throw new BusinessException(ErrorRecord.NotFound);
                case PlaceOrderResult.CartEmpty:
                    throw new BusinessException(ErrorRecord.CartEmpty);
                case PlaceOrderResult.BookUnavailable:
                    throw new BusinessException(ErrorRecord.BookUnavailableForOrder);
                case PlaceOrderResult.InsufficientStock:
                    throw new BusinessException(ErrorRecord.InsufficientStock);
            }

            if (order == null)
            {
                throw new BusinessException(ErrorRecord.Failed);
            }

            await _kafkaProducer.ProduceAsync("order-placed", new OrderPlacedMessage
            {
                OrderId = order.Id,
                UserId = order.UserId,
                OrderCode = order.OrderCode,
                TotalAmount = order.TotalAmount,
                OrderedAt = order.OrderedAt
            });

            PlaceOrderResponse response = new()
            {
                OrderId = order.Id,
                OrderCode = order.OrderCode,
                TotalAmount = order.TotalAmount,
                OrderStatus = order.OrderStatus,
                IsDuplicateRequest = result == PlaceOrderResult.Duplicate
            };

            return ApiResponse<object?>.Response(
                result == PlaceOrderResult.Duplicate
                    ? MessageRecord.Success
                    : MessageRecord.CreatedSuccessfully,
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
