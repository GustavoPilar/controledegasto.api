using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Amigo ou convite devolvido ao cliente, com o saldo de divisões em aberto.
    /// </summary>
    public class FriendResponse
    {
        #region Constructors

        /// <summary>
        /// Cria a resposta resolvendo qual dos dois lados da relação é o amigo.
        /// </summary>
        /// <param name="friendship">Relação de origem, com os dois usuários carregados.</param>
        /// <param name="currentUserId">Usuário que está consultando.</param>
        /// <param name="receivable">Quanto o amigo deve ao usuário.</param>
        /// <param name="payable">Quanto o usuário deve ao amigo.</param>
        public FriendResponse(Friendship friendship, Guid currentUserId, decimal receivable, decimal payable)
        {
            ArgumentNullException.ThrowIfNull(friendship);

            bool currentIsRequester = friendship.RequesterId == currentUserId;

            User? friend = currentIsRequester ? friendship.Addressee : friendship.Requester;

            this.FriendshipId = friendship.Id;
            this.UserId = currentIsRequester ? friendship.AddresseeId : friendship.RequesterId;
            this.FullName = friend?.FullName ?? string.Empty;
            this.UserName = friend?.UserName ?? string.Empty;
            this.Status = friendship.Status;

            // Um convite é "recebido" quando o outro lado é quem enviou. A tela usa isso para
            // decidir entre "aguardando resposta" e os botões de aceitar e recusar.
            this.IsIncoming = !currentIsRequester;

            this.RequestedAt = friendship.RequestedAt;
            this.RespondedAt = friendship.RespondedAt;
            this.Receivable = receivable;
            this.Payable = payable;
            this.NetBalance = receivable - payable;
        }

        #endregion

        #region Properties :: FriendshipId, UserId, FullName, UserName, Status, IsIncoming, RequestedAt, RespondedAt

        public Guid FriendshipId { get; set; }

        /// <summary>Identificador do amigo, usado para dividir compras e convidar a cofrinhos.</summary>
        public Guid UserId { get; set; }

        public string FullName { get; set; }

        public string UserName { get; set; }

        public FriendshipStatus Status { get; set; }

        /// <summary>Verdadeiro quando foi o amigo que enviou o convite.</summary>
        public bool IsIncoming { get; set; }

        public DateTime RequestedAt { get; set; }

        public DateTime? RespondedAt { get; set; }

        #endregion

        #region Properties :: Receivable, Payable, NetBalance

        /// <summary>Quanto o amigo deve ao usuário em divisões ainda em aberto.</summary>
        public decimal Receivable { get; set; }

        /// <summary>Quanto o usuário deve ao amigo em divisões ainda em aberto.</summary>
        public decimal Payable { get; set; }

        /// <summary>Positivo quando o amigo deve; negativo quando o usuário deve.</summary>
        public decimal NetBalance { get; set; }

        #endregion
    }
}
