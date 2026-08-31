using ControleDeGasto.API.Domain.Enums;

namespace ControleDeGasto.API.Domain.Entities
{
    /// <summary>
    /// Lançamento financeiro: uma entrada ou saída de dinheiro em uma data.
    /// </summary>
    /// <remarks>
    /// A natureza (entrada/saída) não é gravada aqui: ela vem de <see cref="Category"/>.
    /// Duplicar o dado abriria espaço para lançamento marcado como entrada apontando para
    /// categoria de saída, e nenhum relatório conseguiria decidir qual dos dois está certo.
    /// </remarks>
    public class Transaction
    {
        #region Properties :: Id, UserId, CategoryId, WalletId, Amount, Description, OccurredOn, PaymentMethod, CreatedAt, UpdatedAt

        public Guid Id { get; set; }

        /// <summary>Dono do lançamento.</summary>
        public Guid UserId { get; set; }

        public Guid CategoryId { get; set; }

        /// <summary>
        /// Carteira que pagou ou recebeu. Nula em lançamentos anteriores ao cadastro de
        /// carteiras, que continuam válidos e apenas ficam fora do saldo por carteira.
        /// </summary>
        public Guid? WalletId { get; set; }

        /// <summary>Valor sempre positivo. O sentido do dinheiro vem do tipo da categoria.</summary>
        public decimal Amount { get; set; }

        public string Description { get; set; } = string.Empty;

        /// <summary>Data de competência do lançamento, em UTC.</summary>
        public DateTime OccurredOn { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        #endregion

        #region Properties :: Status, DueDate, SettledAt

        /// <summary>
        /// Situação de liquidação. Nasce liquidada: o caso comum é registrar o que já
        /// aconteceu, e obrigar a confirmar cada lançamento passado seria trabalho sem ganho.
        /// </summary>
        public TransactionStatus Status { get; set; } = TransactionStatus.Settled;

        /// <summary>
        /// Vencimento, em UTC. Nulo quando o lançamento não é uma conta a pagar ou receber.
        /// </summary>
        /// <remarks>
        /// Separado de <see cref="OccurredOn"/> porque uma conta de luz é de competência de
        /// março e vence em abril: unir os dois campos escolheria entre errar o mês do
        /// relatório ou errar o alerta de vencimento.
        /// </remarks>
        public DateTime? DueDate { get; set; }

        /// <summary>Momento da liquidação, em UTC. Nulo enquanto previsto.</summary>
        public DateTime? SettledAt { get; set; }

        #endregion

        #region Properties :: InstallmentPlanId, InstallmentNumber

        /// <summary>Compra parcelada de origem. Nulo em lançamento avulso.</summary>
        public Guid? InstallmentPlanId { get; set; }

        /// <summary>Número da parcela dentro do plano, iniciando em 1. Nulo em lançamento avulso.</summary>
        public int? InstallmentNumber { get; set; }

        #endregion

        #region Properties :: User, Category, Wallet, InstallmentPlan, Tags, Shares

        public User? User { get; set; }

        public Category? Category { get; set; }

        public Wallet? Wallet { get; set; }

        public InstallmentPlan? InstallmentPlan { get; set; }

        public ICollection<TransactionTag>? Tags { get; set; }

        public ICollection<TransactionShare>? Shares { get; set; }

        #endregion
    }
}
