namespace ControleDeGasto.API.Domain.Entities
{
    /// <summary>
    /// Parte de um lançamento atribuída a um amigo em uma compra dividida.
    /// </summary>
    /// <remarks>
    /// A divisão não cria lançamento na conta do amigo: o dinheiro saiu de quem pagou, e
    /// duplicar a despesa nas duas contas dobraria o gasto do casal em qualquer relatório
    /// somado. O que existe é uma dívida, acompanhada por <see cref="SettledAt"/>.
    /// </remarks>
    public class TransactionShare
    {
        #region Properties :: Id, TransactionId, FriendUserId, Amount, SettledAt, CreatedAt, Transaction, FriendUser

        public Guid Id { get; set; }

        public Guid TransactionId { get; set; }

        /// <summary>Amigo responsável por esta parte.</summary>
        public Guid FriendUserId { get; set; }

        /// <summary>Valor que cabe ao amigo. Sempre positivo.</summary>
        public decimal Amount { get; set; }

        /// <summary>Momento em que a parte foi acertada, em UTC. Nulo enquanto em aberto.</summary>
        public DateTime? SettledAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public Transaction? Transaction { get; set; }

        public User? FriendUser { get; set; }

        #endregion
    }
}
