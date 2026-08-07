namespace ControleDeGasto.API.Application.Interfaces
{
    public interface IEmailSender
    {
        /// <summary>
        /// Envia um e-mail para o destinatário informado
        /// </summary>
        /// <param name="toEmail">Endereço do e-mail do destinatário</param>
        /// <param name="subject">Assunto do e-mail</param>
        /// <param name="htmlBody">Corpo do e-mail em HTML</param>
        /// <returns></returns>
        Task SendEmailAsync(string toEmail, string subject, string htmlBody);
    }
}
