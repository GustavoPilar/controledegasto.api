using ControleDeGasto.API.Domain.Enums;

namespace ControleDeGasto.API.Domain.Entities
{
    /// <summary>
    /// Relação de amizade entre dois usuários, usada para dividir compras e compartilhar
    /// cofrinhos.
    /// </summary>
    /// <remarks>
    /// Uma única linha representa a relação nos dois sentidos. Gravar duas linhas espelhadas
    /// facilitaria a leitura, mas exigiria manter as duas em sincronia a cada resposta — e uma
    /// falha no meio deixaria a amizade aceita para um lado e pendente para o outro.
    /// </remarks>
    public class Friendship
    {
        #region Properties :: Id, RequesterId, AddresseeId, Status, RequestedAt, RespondedAt, BlockedByUserId, Requester, Addressee

        public Guid Id { get; set; }

        /// <summary>Quem enviou o convite.</summary>
        public Guid RequesterId { get; set; }

        /// <summary>Quem recebeu o convite.</summary>
        public Guid AddresseeId { get; set; }

        public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;

        public DateTime RequestedAt { get; set; }

        /// <summary>Momento da resposta ao convite, em UTC. Nulo enquanto pendente.</summary>
        public DateTime? RespondedAt { get; set; }

        /// <summary>
        /// Quem aplicou o bloqueio. Guardado porque só esse lado pode desfazê-lo: sem o
        /// registro, o usuário bloqueado conseguiria se desbloquear.
        /// </summary>
        public Guid? BlockedByUserId { get; set; }

        public User? Requester { get; set; }

        public User? Addressee { get; set; }

        #endregion
    }
}
