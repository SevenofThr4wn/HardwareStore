using HardwareStore.Services.Interfaces;
using HardwareStore.Services.Settings;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace HardwareStore.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public void SendEmail(string to, string subject, string message)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = message,
            };

            email.Body = builder.ToMessageBody();

            using (var smtp = new SmtpClient())
            {
                smtp.Connect(_settings.SmtpServer, _settings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                smtp.Authenticate(_settings.Username, _settings.Password);
                smtp.Send(email);
                smtp.Disconnect(true);
            }
        }
    }
}