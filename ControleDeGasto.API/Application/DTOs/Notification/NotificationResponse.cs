using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Notificação devolvida ao cliente.
    /// </summary>
    public class NotificationResponse(Notification notification)
    {
        #region Properties :: Id, Type, Title, Message, ReferenceId, IsRead, ReadAt, CreatedAt

        public Guid Id { get; set; } = notification.Id;

        public NotificationType Type { get; set; } = notification.Type;

        public string Title { get; set; } = notification.Title;

        public string Message { get; set; } = notification.Message;

        public Guid? ReferenceId { get; set; } = notification.ReferenceId;

        public bool IsRead { get; set; } = notification.ReadAt.HasValue;

        public DateTime? ReadAt { get; set; } = notification.ReadAt;

        public DateTime CreatedAt { get; set; } = notification.CreatedAt;

        #endregion
    }
}
