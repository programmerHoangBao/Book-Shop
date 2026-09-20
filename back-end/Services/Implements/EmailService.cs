using back_end.DTOs.Emails.Requests;
using back_end.Settings;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace back_end.Services.Implements
{
    public class EmailService : IEmailService
    {
        private readonly EmailSetting _emailSetting;
        private readonly IWebHostEnvironment _environment;

        public EmailService(IOptions<EmailSetting> options, IWebHostEnvironment environment)
        {
            _emailSetting = options.Value;
            _environment = environment;
        }
        public async Task SendOtpEmailAsync(SendOtpRequest req)
        {
            var templatePath = Path.Combine(
                _environment.ContentRootPath, 
                "Templates", 
                "RegisterOtpTemplate.html"
            );
            if (!System.IO.File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Email template not found at path: {templatePath}");
            }
            var emailTemplate = await System.IO.File.ReadAllTextAsync(templatePath);
            var emailBody = emailTemplate
                .Replace("{{Name}}", req.Name)
                .Replace("{{OtpCode}}", req.Otp)
                .Replace("{{OtpExpirationTime}}", req.OtpExpirySeconds.ToString())
                .Replace("{{AppName}}", _emailSetting.AppName);
            using var message = new MailMessage();
            message.From = new MailAddress(_emailSetting.FromEmail, _emailSetting.AppName);
            message.To.Add(new MailAddress(req.ToEmail));
            message.Subject = "Verify Your Email - OTP Code";
            message.Body = emailBody;
            message.IsBodyHtml = true;
            using var client = new SmtpClient(_emailSetting.Host, _emailSetting.Port)
            {
                Credentials = new NetworkCredential(_emailSetting.FromEmail, _emailSetting.Password),
                EnableSsl = _emailSetting.EnableSsl
            };
            await client.SendMailAsync(message);
        }
    }
}
