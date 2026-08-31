using ControleDeGasto.API.Domain.Enums;

namespace ControleDeGasto.API.Domain.Entities
{
    /// <summary>
    /// Compra parcelada. Agrupa as parcelas geradas como lançamentos previstos.
    /// </summary>
    /// <remarks>
    /// As parcelas são lançamentos de verdade, criados no momento da compra: é o que permite
    /// filtrar, liquidar e ver cada mês no extrato como qualquer outra conta. O plano existe
    /// para que "3/12 de Geladeira" continue sabendo a que compra pertence, e para que editar
    /// ou cancelar o restante seja uma operação única.
    /// </remarks>
    public class InstallmentPlan
    {
        #region Properties :: Id, UserId, CategoryId, WalletId, Description, TotalAmount, InstallmentCount, FirstDueDate, PaymentMethod, CreatedAt, User, Category, Wallet, Installments

        public Guid Id { get; set; }

        /// <summary>Dono da compra.</summary>
        public Guid UserId { get; set; }

        public Guid CategoryId { get; set; }

        /// <summary>Carteira que paga as parcelas. Nula quando não informada.</summary>
        public Guid? WalletId { get; set; }

        public string Description { get; set; } = string.Empty;

        /// <summary>Valor total da compra. Sempre positivo.</summary>
        public decimal TotalAmount { get; set; }

        /// <summary>Quantidade de parcelas.</summary>
        public int InstallmentCount { get; set; }

        /// <summary>Vencimento da primeira parcela, em UTC.</summary>
        public DateTime FirstDueDate { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public DateTime CreatedAt { get; set; }

        public User? User { get; set; }

        public Category? Category { get; set; }

        public Wallet? Wallet { get; set; }

        public ICollection<Transaction>? Installments { get; set; }

        #endregion
    }
}
