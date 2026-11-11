using back_end.DTOs;
using back_end.DTOs.Notifications.Requests;
using back_end.DTOs.Notifications.Responses;
using back_end.Entities;
using back_end.Enums;
using back_end.Exceptions;
using back_end.Kafka.Messages;
using back_end.Kafka.Producers;
using back_end.Records;
using back_end.Repositories;

namespace back_end.Services.Implements
{
    public class NotificationService : INotificationService
    {
        private readonly IUserRepository _userRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IKafkaProducer _kafkaProducer;
        private readonly ICurrentUserService _currentUserService;

        public NotificationService(
            IUserRepository userRepository,
            INotificationRepository notificationRepository,
            IKafkaProducer kafkaProducer,
            ICurrentUserService currentUserService)
        {
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _kafkaProducer = kafkaProducer;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<object?>> CreateNotificationAsync(
            CreateNotificationRequest request)
        {
            if (request.UserId == Guid.Empty ||
                string.IsNullOrWhiteSpace(request.Title) ||
                request.Title.Trim().Length > 500 ||
                string.IsNullOrWhiteSpace(request.Content) ||
                !Enum.IsDefined(request.Type) ||
                !Enum.IsDefined(request.Channel))
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            var user = await _userRepository.GetUserByIdAsync(request.UserId);
            if (user == null || user.IsDeleted || !user.Enabled)
            {
                throw new BusinessException(ErrorRecord.NotFound);
            }

            Guid notificationId = Guid.CreateVersion7();
            CreateNotificationMessage message = new()
            {
                NotificationId = notificationId,
                UserId = request.UserId,
                Type = request.Type,
                Title = request.Title.Trim(),
                Content = request.Content.Trim(),
                Channel = request.Channel
            };

            await _kafkaProducer.ProduceAsync("create-notification", message);

            CreateNotificationResponse response = new()
            {
                NotificationId = notificationId
            };

            return ApiResponse<object?>.Response(
                MessageRecord.QueuedSuccessfully,
                data: response);
        }

        public async Task<ApiResponse<PaginationResponse<NotificationResponse>>> GetForUserAsync(
            Guid userId,
            int pageNumber,
            int pageSize)
        {
            if (userId == Guid.Empty || pageNumber < 1 || pageSize < 1 || pageSize > 100 ||
                (long)(pageNumber - 1) * pageSize > int.MaxValue)
            {
                throw new BusinessException(ErrorRecord.RequestInvalid);
            }

            Guid? currentUserId = _currentUserService.GetCurrentUserId();
            if (!currentUserId.HasValue || currentUserId.Value == Guid.Empty)
            {
                throw new BusinessException(ErrorRecord.Forbidden);
            }

            if (_currentUserService.GetRoleCurrentUser() != RoleUser.Admin &&
                currentUserId.Value != userId)
            {
                throw new BusinessException(ErrorRecord.Forbidden);
            }

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                throw new BusinessException(ErrorRecord.NotFound);
            }

            var result = await _notificationRepository.GetForUserAsync(userId, pageNumber, pageSize);
            PaginationResponse<NotificationResponse> page = new()
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalItems = result.TotalItems,
                TotalPages = (int)Math.Ceiling((double)result.TotalItems / pageSize),
                Items = result.Notifications.Select(notification => new NotificationResponse
                {
                    Id = notification.Id,
                    UserId = notification.UserId,
                    Type = notification.Type,
                    Title = notification.Title,
                    Content = notification.Content,
                    Channel = notification.Channel,
                    IsRead = notification.IsRead,
                    SentAt = notification.SentAt,
                    CreatedAt = notification.CreatedAt
                }).ToList()
            };

            return ApiResponse<PaginationResponse<NotificationResponse>>.Response(
                MessageRecord.Success,
                data: page);
        }
    }
}
