using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;
using ControleDeGasto.API.Domain.ReadModels;

namespace ControleDeGasto.API.Domain.Interfaces
{
    /// <summary>
    /// Acesso a notificações. Todo método de leitura do usuário filtra pelo dono.
    /// </summary>
    public interface INotificationRepository
    {
        #region Methods :: GetPagedAsync(), GetUnreadCountAsync(), ExistsRecentAsync(), CreateAsync(), CreateRangeAsync(), MarkAsReadAsync(), MarkAllAsReadAsync()

        /// <summary>
        /// Lista notificações paginadas.
        /// </summary>
        /// <param name="userId">Destinatário.</param>
        /// <param name="onlyUnread">Quando verdadeiro, traz apenas as não lidas.</param>
        /// <param name="page">Página solicitada, iniciando em 1.</param>
        /// <param name="pageSize">Itens por página.</param>
        /// <returns>Página de notificações e total de registros filtrados.</returns>
        Task<PagedResult<Notification>> GetPagedAsync(Guid userId, bool onlyUnread, int page, int pageSize);

        /// <summary>
        /// Conta as notificações não lidas.
        /// </summary>
        /// <param name="userId">Destinatário.</param>
        /// <returns>Quantidade de não lidas.</returns>
        Task<int> GetUnreadCountAsync(Guid userId);

        /// <summary>
        /// Verifica se já existe notificação equivalente criada a partir de um momento.
        /// </summary>
        /// <param name="userId">Destinatário.</param>
        /// <param name="type">Motivo da notificação.</param>
        /// <param name="referenceId">Entidade relacionada. Nulo compara com notificações sem referência.</param>
        /// <param name="since">Momento a partir do qual procurar, em UTC.</param>
        /// <returns>True se já existir, evitando avisar a mesma coisa repetidamente.</returns>
        Task<bool> ExistsRecentAsync(Guid userId, NotificationType type, Guid? referenceId, DateTime since);

        /// <summary>
        /// Persiste uma notificação nova.
        /// </summary>
        /// <param name="notification">Notificação a gravar.</param>
        /// <returns>True se a gravação afetou alguma linha.</returns>
        Task<bool> CreateAsync(Notification notification);

        /// <summary>
        /// Persiste várias notificações em uma única gravação.
        /// </summary>
        /// <param name="notifications">Notificações a gravar.</param>
        /// <returns>Quantidade de notificações gravadas.</returns>
        Task<int> CreateRangeAsync(IEnumerable<Notification> notifications);

        /// <summary>
        /// Marca uma notificação como lida.
        /// </summary>
        /// <param name="userId">Destinatário.</param>
        /// <param name="notificationId">Identificador da notificação.</param>
        /// <param name="readAt">Momento da leitura, em UTC.</param>
        /// <returns>True se alguma linha foi alterada.</returns>
        Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId, DateTime readAt);

        /// <summary>
        /// Marca todas as notificações não lidas do usuário como lidas.
        /// </summary>
        /// <param name="userId">Destinatário.</param>
        /// <param name="readAt">Momento da leitura, em UTC.</param>
        /// <returns>Quantidade de notificações alteradas.</returns>
        Task<int> MarkAllAsReadAsync(Guid userId, DateTime readAt);

        #endregion

        #region Methods :: GetPendingEmailAsync(), MarkEmailSentAsync()

        /// <summary>
        /// Lista notificações que ainda não foram enviadas por e-mail, já com o destinatário.
        /// </summary>
        /// <param name="maxCount">Quantidade máxima de itens por execução.</param>
        /// <returns>Notificações pendentes de envio, das mais antigas para as mais novas.</returns>
        Task<IReadOnlyList<PendingEmailNotification>> GetPendingEmailAsync(int maxCount);

        /// <summary>
        /// Registra que o e-mail de uma notificação foi enviado.
        /// </summary>
        /// <param name="notificationId">Identificador da notificação.</param>
        /// <param name="sentAt">Momento do envio, em UTC.</param>
        /// <returns>True se alguma linha foi alterada.</returns>
        Task<bool> MarkEmailSentAsync(Guid notificationId, DateTime sentAt);

        #endregion
    }
}
