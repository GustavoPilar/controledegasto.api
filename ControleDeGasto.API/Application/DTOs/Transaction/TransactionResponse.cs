using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Lançamento devolvido ao cliente, já com categoria, carteira, etiquetas e divisão para a
    /// listagem não precisar de chamadas adicionais.
    /// </summary>
    public class TransactionResponse
    {
        #region Constructors

        /// <summary>
        /// Cria a resposta a partir do lançamento.
        /// </summary>
        /// <param name="transaction">Lançamento de origem.</param>
        /// <param name="reference">
        /// Momento usado como "hoje" ao decidir se a conta está vencida, em UTC. Nulo assume o
        /// instante atual.
        /// </param>
        public TransactionResponse(Transaction transaction, DateTime? reference = null)
        {
            ArgumentNullException.ThrowIfNull(transaction);

            this.Id = transaction.Id;
            this.CategoryId = transaction.CategoryId;
            this.CategoryName = transaction.Category?.Name ?? string.Empty;
            this.CategoryColor = transaction.Category?.Color ?? string.Empty;
            this.CategoryIcon = transaction.Category?.Icon ?? string.Empty;
            this.Type = transaction.Category?.Type ?? TransactionType.Expense;

            this.WalletId = transaction.WalletId;
            this.WalletName = transaction.Wallet?.Name;
            this.WalletColor = transaction.Wallet?.Color;
            this.WalletIcon = transaction.Wallet?.Icon;
            this.WalletKind = transaction.Wallet?.Kind;

            this.Amount = transaction.Amount;
            this.Description = transaction.Description;
            this.OccurredOn = transaction.OccurredOn;
            this.PaymentMethod = transaction.PaymentMethod;
            this.CreatedAt = transaction.CreatedAt;
            this.UpdatedAt = transaction.UpdatedAt;

            this.Status = transaction.Status;
            this.DueDate = transaction.DueDate;
            this.SettledAt = transaction.SettledAt;

            // O atraso é derivado na resposta em vez de gravado: a data de referência muda a
            // cada dia, e uma coluna gravada exigiria uma rotina diária para continuar correta.
            DateTime now = reference ?? DateTime.UtcNow;

            this.IsOverdue = transaction.Status == TransactionStatus.Pending
                && transaction.DueDate.HasValue
                && transaction.DueDate.Value.Date < now.Date;

            this.InstallmentPlanId = transaction.InstallmentPlanId;
            this.InstallmentNumber = transaction.InstallmentNumber;
            this.InstallmentCount = transaction.InstallmentPlan?.InstallmentCount;

            this.Tags = transaction.Tags?
                .Where(item => item.Tag is not null)
                .Select(item => new TagResponse(item.Tag!, 0))
                .OrderBy(item => item.Name)
                .ToList() ?? [];

            this.Shares = transaction.Shares?
                .Select(item => new TransactionShareResponse(item))
                .OrderBy(item => item.FriendFullName)
                .ToList() ?? [];

            this.SharedAmount = this.Shares.Sum(item => item.Amount);

            // A parte do dono é o que sobra depois da divisão: é esse valor, e não o total, que
            // representa o gasto dele. O total continua disponível em Amount.
            this.OwnAmount = Math.Max(0, transaction.Amount - this.SharedAmount);

            this.PendingSharedAmount = this.Shares
                .Where(item => !item.IsSettled)
                .Sum(item => item.Amount);

            this.IsShared = this.Shares.Count > 0;
        }

        #endregion

        #region Properties :: Id, CategoryId, CategoryName, CategoryColor, CategoryIcon, Type, Amount, Description, OccurredOn, PaymentMethod, CreatedAt, UpdatedAt

        public Guid Id { get; set; }

        public Guid CategoryId { get; set; }

        public string CategoryName { get; set; }

        public string CategoryColor { get; set; }

        public string CategoryIcon { get; set; }

        /// <summary>Natureza do lançamento, derivada da categoria.</summary>
        public TransactionType Type { get; set; }

        public decimal Amount { get; set; }

        public string Description { get; set; }

        public DateTime OccurredOn { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        #endregion

        #region Properties :: WalletId, WalletName, WalletColor, WalletIcon, WalletKind

        public Guid? WalletId { get; set; }

        public string? WalletName { get; set; }

        public string? WalletColor { get; set; }

        public string? WalletIcon { get; set; }

        public WalletKind? WalletKind { get; set; }

        #endregion

        #region Properties :: Status, DueDate, SettledAt, IsOverdue

        public TransactionStatus Status { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? SettledAt { get; set; }

        /// <summary>Previsto com vencimento já passado.</summary>
        public bool IsOverdue { get; set; }

        #endregion

        #region Properties :: InstallmentPlanId, InstallmentNumber, InstallmentCount

        public Guid? InstallmentPlanId { get; set; }

        /// <summary>Número da parcela. Nulo em lançamento avulso.</summary>
        public int? InstallmentNumber { get; set; }

        /// <summary>Total de parcelas do plano. Nulo quando o plano não foi carregado.</summary>
        public int? InstallmentCount { get; set; }

        #endregion

        #region Properties :: Tags, Shares, IsShared, SharedAmount, OwnAmount, PendingSharedAmount

        public IReadOnlyList<TagResponse> Tags { get; set; }

        public IReadOnlyList<TransactionShareResponse> Shares { get; set; }

        public bool IsShared { get; set; }

        /// <summary>Soma das partes atribuídas a amigos.</summary>
        public decimal SharedAmount { get; set; }

        /// <summary>Parte que cabe a quem lançou, depois da divisão.</summary>
        public decimal OwnAmount { get; set; }

        /// <summary>Soma das partes ainda não acertadas.</summary>
        public decimal PendingSharedAmount { get; set; }

        #endregion
    }
}
