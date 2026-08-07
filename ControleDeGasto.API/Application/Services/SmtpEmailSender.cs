using ControleDeGasto.API.Application.Configuration;
using ControleDeGasto.API.Application.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ControleDeGasto.API.Application.Services
{
    public class SmtpEmailSender(
        IOptions<EmailSettings> settings,
        ILogger<SmtpEmailSender> logger) : IEmailSender
    {
        #region Properties

        private readonly EmailSettings settings = settings.Value;

        private readonly ILogger<SmtpEmailSender> logger = logger;

        #endregion

        #region Methods :: SendEmailAsync()

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress(this.settings.FromName, this.settings.FromAddress));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using SmtpClient client = new SmtpClient();

            try
            {
                await client.ConnectAsync(this.settings.Host, this.settings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(this.settings.Username, this.settings.Password);
                await client.SendAsync(message);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Falha ao enviar e-mail para {ToEmail}.", toEmail);
                throw;
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }

        #endregion
    }
}
