using back_end.DTOs;
using back_end.DTOs.Invoices.Responses;
using back_end.DTOs.Orders.Requests;
using back_end.DTOs.Orders.Responses;
using back_end.Entities;
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
        private readonly IStoreStaffRepository _storeStaffRepository;

        public OrderService(
            IOrderRepository orderRepository,
            ICurrentUserService currentUserService,
            IKafkaProducer kafkaProducer,
            IStoreStaffRepository storeStaffRepository)
        {
            _orderRepository = orderRepository;
            _currentUserService = currentUserService;
            _kafkaProducer = kafkaProducer;
            _storeStaffRepository = storeStaffRepository;
        }

        public async Task<ApiResponse<PaginationResponse<InvoiceResponse>>> GetInvoicesAsync(
            OrderStatus status,
            int pageNumber,
            int pageSize)
        {
            if (!Enum.IsDefined(status) || pageNumber < 1 || pageSize < 1 || pageSize > 100 ||
                (long)(pageNumber - 1) * pageSize > int.MaxValue)
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            Guid? userId = _currentUserService.GetCurrentUserId();
            if (!userId.HasValue || userId.Value == Guid.Empty ||
                _currentUserService.GetRoleCurrentUser() != RoleUser.Customer)
            {
                throw new BusinessException(ErrorRecord.Forbidden);
            }

            var result = await _orderRepository.GetInvoicesAsync(
                userId.Value,
                status,
                pageNumber,
                pageSize);

            PaginationResponse<InvoiceResponse> page = new()
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalItems = result.TotalItems,
                TotalPages = (int)Math.Ceiling((double)result.TotalItems / pageSize),
                Items = result.Orders.Select(order => new InvoiceResponse
                {
                    InvoiceId = order.Id,
                    OrderCode = order.OrderCode,
                    Subtotal = order.Subtotal,
                    DiscountAmount = order.DiscountAmount,
                    ShippingFee = order.ShippingFee,
                    TotalAmount = order.TotalAmount,
                    PaymentMethod = order.PaymentMethod,
                    PaymentStatus = order.PaymentStatus,
                    OrderStatus = order.OrderStatus,
                    OrderedAt = order.OrderedAt,
                    CompletedAt = order.CompletedAt,
                    Items = order.Items.Select(item => new InvoiceItemResponse
                    {
                        BookId = item.BookId,
                        BookName = item.Book.Name,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        DiscountAmount = item.DiscountAmount,
                        TotalPrice = item.TotalPrice
                    }).ToList()
                }).ToList()
            };

            return ApiResponse<PaginationResponse<InvoiceResponse>>.Response(
                MessageRecord.Success,
                data: page);
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

            return ApiResponse<object?>.Response(MessageRecord.OrderQueuedSuccessfully, data: response);
        }

        public async Task<ApiResponse<object?>> ConfirmAsync(Guid orderId)
        {
            if (orderId == Guid.Empty)
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            var (_, storeId) = await ResolveStoreScopeAsync();
            var (result, order) = await _orderRepository.ConfirmAsync(orderId, storeId);
            switch (result)
            {
                case ConfirmOrderResult.NotFound:
                    throw new BusinessException(ErrorRecord.NotFound);
                case ConfirmOrderResult.NotPending:
                    throw new BusinessException(ErrorRecord.OrderNotPending);
                case ConfirmOrderResult.StoreNotAssociated:
                    throw new BusinessException(ErrorRecord.Forbidden);
            }

            if (order == null)
            {
                throw new BusinessException(ErrorRecord.Failed);
            }

            await _kafkaProducer.ProduceAsync("order-confirmed", new OrderConfirmedMessage
            {
                OrderId = order.Id,
                UserId = order.UserId,
                OrderCode = order.OrderCode,
                ConfirmedAt = order.UpdatedAt ?? DateTime.UtcNow
            });

            ConfirmOrderResponse response = new()
            {
                OrderId = order.Id,
                OrderCode = order.OrderCode,
                OrderStatus = order.OrderStatus,
                WasAlreadyConfirmed = result == ConfirmOrderResult.AlreadyConfirmed
            };

            return ApiResponse<object?>.Response(MessageRecord.Success, data: response);
        }

        public async Task<ApiResponse<object?>> SetInTransitAsync(Guid orderId)
        {
            if (orderId == Guid.Empty)
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            var (_, storeId) = await ResolveStoreScopeAsync();
            var (result, order) = await _orderRepository.SetInTransitAsync(orderId, storeId);
            switch (result)
            {
                case SetOrderInTransitResult.NotFound:
                    throw new BusinessException(ErrorRecord.NotFound);
                case SetOrderInTransitResult.NotConfirmed:
                    throw new BusinessException(ErrorRecord.OrderNotConfirmed);
                case SetOrderInTransitResult.StoreNotAssociated:
                    throw new BusinessException(ErrorRecord.Forbidden);
            }

            if (order == null)
            {
                throw new BusinessException(ErrorRecord.Failed);
            }

            await _kafkaProducer.ProduceAsync("order-in-transit", new OrderInTransitMessage
            {
                OrderId = order.Id,
                UserId = order.UserId,
                OrderCode = order.OrderCode,
                InTransitAt = order.UpdatedAt ?? DateTime.UtcNow
            });

            OrderInTransitResponse response = new()
            {
                OrderId = order.Id,
                OrderCode = order.OrderCode,
                OrderStatus = order.OrderStatus,
                WasAlreadyInTransit = result == SetOrderInTransitResult.AlreadyInTransit
            };

            return ApiResponse<object?>.Response(MessageRecord.Success, data: response);
        }

        public async Task<ApiResponse<object?>> CompleteAsync(Guid orderId)
        {
            if (orderId == Guid.Empty)
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            var (_, storeId) = await ResolveStoreScopeAsync();
            var (result, order) = await _orderRepository.CompleteAsync(orderId, storeId);
            switch (result)
            {
                case CompleteOrderResult.NotFound:
                    throw new BusinessException(ErrorRecord.NotFound);
                case CompleteOrderResult.NotInTransit:
                    throw new BusinessException(ErrorRecord.OrderNotInTransit);
                case CompleteOrderResult.StoreNotAssociated:
                    throw new BusinessException(ErrorRecord.Forbidden);
            }

            if (order == null)
            {
                throw new BusinessException(ErrorRecord.Failed);
            }

            await _kafkaProducer.ProduceAsync("order-completed", new OrderCompletedMessage
            {
                OrderId = order.Id,
                UserId = order.UserId,
                OrderCode = order.OrderCode,
                PaymentStatus = order.PaymentStatus,
                CompletedAt = order.CompletedAt ?? DateTime.UtcNow
            });

            CompleteOrderResponse response = new()
            {
                OrderId = order.Id,
                OrderCode = order.OrderCode,
                OrderStatus = order.OrderStatus,
                PaymentStatus = order.PaymentStatus,
                WasAlreadyCompleted = result == CompleteOrderResult.AlreadyCompleted
            };

            return ApiResponse<object?>.Response(MessageRecord.Success, data: response);
        }

        private async Task<(Guid UserId, Guid? StoreId)> ResolveStoreScopeAsync()
        {
            Guid? userId = _currentUserService.GetCurrentUserId();
            RoleUser? role = _currentUserService.GetRoleCurrentUser();
            if (!userId.HasValue || userId.Value == Guid.Empty)
            {
                throw new BusinessException(ErrorRecord.Forbidden);
            }

            if (role == RoleUser.Admin)
            {
                return (userId.Value, null);
            }

            if (role is not (RoleUser.Manager or RoleUser.Employee))
            {
                throw new BusinessException(ErrorRecord.Forbidden);
            }

            StoreEntity? store = await _storeStaffRepository.GetStoreByUserIdAsync(userId.Value);
            if (store == null || store.IsDeleted)
            {
                throw new BusinessException(ErrorRecord.Forbidden);
            }

            return (userId.Value, store.Id);
        }

        private static Guid CreateIdempotentOrderId(Guid userId, string key)
        {
            byte[] input = Encoding.UTF8.GetBytes($"{userId:D}:{key}");
            byte[] hash = SHA256.HashData(input);
            return new Guid(hash.AsSpan(0, 16));
        }
    }
}
