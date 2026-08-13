using ControleDeGasto.API.Application.DTOs;
using ControleDeGasto.API.Domain.Enums;

namespace ControleDeGasto.API.Application.Interfaces
{
    /// <summary>
    /// Criação, leitura e envio das notificações.
    /// </summary>
    public interface INotificationService
    {
        #region Methods :: GetPagedAsync(), GetUnreadCountAsync(), MarkAsReadAsync(), MarkAllAsReadAsync()

        /// <summary>
        /// Lista as notificações do usuário.
        /// </summary>
        /// <param name="userId">Destinatário.</param>
        /// <param name="onlyUnread">Quando verdadeiro, traz apenas as não lidas.</param>
        /// <param name="page">Página solicitada, iniciando em 1.</param>
        /// <param name="pageSize">Itens por página.</param>
        /// <returns>Página de notificações.</returns>
        Task<PagedResponse<NotificationResponse>> GetPagedAsync(Guid userId, bool onlyUnread, int page, int pageSize);

        /// <summary>
        /// Conta as notificações não lidas.
        /// </summary>
        /// <param name="userId">Destinatário.</param>
        /// <returns>Quantidade de não lidas.</returns>
        Task<int> GetUnreadCountAsync(Guid userId);

        /// <summary>
        /// Marca uma notificação como lida.
        /// </summary>
        /// <param name="userId">Destinatário.</param>
        /// <param name="notificationId">Identificador da notificação.</param>
        /// <returns>True se alguma notificação foi marcada.</returns>
        Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId);

        /// <summary>
        /// Marca todas as notificações do usuário como lidas.
        /// </summary>
        /// <param name="userId">Destinatário.</param>
        /// <returns>Quantidade de notificações marcadas.</returns>
        Task<int> MarkAllAsReadAsync(Guid userId);

        #endregion

        #region Methods :: CreateAsync(), EvaluateForUserAsync(), ProcessPendingEmailsAsync()

        /// <summary>
        /// Cria uma notificação, evitando repetir avisos equivalentes.
        /// </summary>
        /// <param name="userId">Destinatário.</param>
        /// <param name="type">Motivo do aviso.</param>
        /// <param name="title">Título exibido.</param>
        /// <param name="message">Texto do aviso.</param>
        /// <param name="referenceId">Entidade relacionada, quando houver.</param>
        /// <param name="dedupeWindow">
        /// Janela de tempo em que um aviso equivalente impede a criação. Nulo significa
        /// "avisar apenas uma vez", considerando todo o histórico.
        /// </param>
        /// <returns>True quando a notificação foi criada; false quando foi suprimida por duplicidade.</returns>
        Task<bool> CreateAsync(Guid userId, NotificationType type, string title, string message, Guid? referenceId, TimeSpan? dedupeWindow);

        /// <summary>
        /// Avalia as regras de aviso para um usuário e cria as notificações necessárias.
        /// </summary>
        /// <param name="userId">Usuário a avaliar.</param>
        /// <returns>Quantidade de notificações criadas.</returns>
        Task<int> EvaluateForUserAsync(Guid userId);

        /// <summary>
        /// Envia por e-mail as notificações pendentes.
        /// </summary>
        /// <param name="maxCount">Quantidade máxima de envios nesta execução.</param>
        /// <returns>Quantidade de e-mails enviados com sucesso.</returns>
        Task<int> ProcessPendingEmailsAsync(int maxCount);

        #endregion
    }
}
