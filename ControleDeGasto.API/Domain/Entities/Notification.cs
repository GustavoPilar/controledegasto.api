using ControleDeGasto.API.Domain.Enums;

namespace ControleDeGasto.API.Domain.Entities
{
    /// <summary>
    /// Aviso gerado pelo sistema para o usuário (meta atingida, reserva baixa, gasto acima
    /// do mês anterior...).
    /// </summary>
    public class Notification
    {
        #region Properties :: Id, UserId, Type, Title, Message, ReferenceId, ReadAt, EmailSentAt, CreatedAt, User

        public Guid Id { get; set; }

        /// <summary>Destinatário.</summary>
        public Guid UserId { get; set; }

        public NotificationType Type { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Entidade que originou o aviso (cofrinho, categoria). Usada para não repetir a
        /// mesma notificação e para navegação na interface.
        /// </summary>
        public Guid? ReferenceId { get; set; }

        /// <summary>Momento da leitura, em UTC. Nulo enquanto não lida.</summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>Momento do envio por e-mail, em UTC. Nulo quando não houve envio.</summary>
        public DateTime? EmailSentAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public User? User { get; set; }

        #endregion
    }
}
