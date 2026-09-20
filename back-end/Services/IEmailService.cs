using back_end.DTOs.Emails.Requests;

namespace back_end.Services
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(SendOtpRequest req);
    }
}
