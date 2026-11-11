using back_end.DTOs;
using back_end.DTOs.Notifications.Requests;
using back_end.DTOs.Notifications.Responses;
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
        private readonly IKafkaProducer _kafkaProducer;

        public NotificationService(IUserRepository userRepository, IKafkaProducer kafkaProducer)
        {
            _userRepository = userRepository;
            _kafkaProducer = kafkaProducer;
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
    }
}
