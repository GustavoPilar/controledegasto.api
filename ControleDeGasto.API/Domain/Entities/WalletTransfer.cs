namespace ControleDeGasto.API.Domain.Entities
{
    /// <summary>
    /// Movimento de dinheiro entre duas carteiras do mesmo usuário.
    /// </summary>
    /// <remarks>
    /// Não é um par de lançamentos porque transferência não é receita nem despesa: lançada como
    /// saída em uma carteira e entrada em outra, ela inflaria os totais do mês em duas vezes o
    /// valor transferido e exigiria uma categoria fantasma para se esconder dos relatórios.
    /// </remarks>
    public class WalletTransfer
    {
        #region Properties :: Id, UserId, FromWalletId, ToWalletId, Amount, OccurredOn, Note, CreatedAt, User, FromWallet, ToWallet

        public Guid Id { get; set; }

        /// <summary>Dono das duas carteiras.</summary>
        public Guid UserId { get; set; }

        public Guid FromWalletId { get; set; }

        public Guid ToWalletId { get; set; }

        /// <summary>Valor transferido. Sempre positivo.</summary>
        public decimal Amount { get; set; }

        /// <summary>Data da transferência, em UTC.</summary>
        public DateTime OccurredOn { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }

        public User? User { get; set; }

        public Wallet? FromWallet { get; set; }

        public Wallet? ToWallet { get; set; }

        #endregion
    }
}
