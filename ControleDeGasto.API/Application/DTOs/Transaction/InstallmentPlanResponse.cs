using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Compra parcelada devolvida ao cliente, com o andamento das parcelas apurado.
    /// </summary>
    public class InstallmentPlanResponse
    {
        #region Constructors

        /// <summary>
        /// Cria a resposta a partir do plano, com as parcelas carregadas.
        /// </summary>
        /// <param name="plan">Compra de origem.</param>
        public InstallmentPlanResponse(InstallmentPlan plan)
        {
            ArgumentNullException.ThrowIfNull(plan);

            this.Id = plan.Id;
            this.Description = plan.Description;
            this.TotalAmount = plan.TotalAmount;
            this.InstallmentCount = plan.InstallmentCount;
            this.FirstDueDate = plan.FirstDueDate;
            this.PaymentMethod = plan.PaymentMethod;
            this.CreatedAt = plan.CreatedAt;

            this.CategoryId = plan.CategoryId;
            this.CategoryName = plan.Category?.Name ?? string.Empty;
            this.CategoryColor = plan.Category?.Color ?? string.Empty;
            this.CategoryIcon = plan.Category?.Icon ?? string.Empty;

            this.WalletId = plan.WalletId;
            this.WalletName = plan.Wallet?.Name;
            this.WalletColor = plan.Wallet?.Color;

            List<Transaction> installments = plan.Installments?.ToList() ?? [];

            this.PaidCount = installments.Count(item => item.Status == TransactionStatus.Settled);
            this.PaidAmount = installments
                .Where(item => item.Status == TransactionStatus.Settled)
                .Sum(item => item.Amount);

            this.RemainingCount = installments.Count(item => item.Status == TransactionStatus.Pending);
            this.RemainingAmount = installments
                .Where(item => item.Status == TransactionStatus.Pending)
                .Sum(item => item.Amount);

            this.ProgressPercentage = plan.InstallmentCount <= 0
                ? 0
                : Math.Round(this.PaidCount / (decimal)plan.InstallmentCount * 100, 2);

            // O vencimento em aberto mais próximo é o que a tela destaca: "próxima parcela".
            this.NextDueDate = installments
                .Where(item => item.Status == TransactionStatus.Pending && item.DueDate.HasValue)
                .OrderBy(item => item.DueDate)
                .Select(item => item.DueDate)
                .FirstOrDefault();

            this.LastDueDate = installments
                .Where(item => item.DueDate.HasValue)
                .OrderByDescending(item => item.DueDate)
                .Select(item => item.DueDate)
                .FirstOrDefault();

            this.IsCompleted = this.RemainingCount == 0 && installments.Count > 0;
        }

        #endregion

        #region Properties :: Id, Description, TotalAmount, InstallmentCount, FirstDueDate, PaymentMethod, CreatedAt

        public Guid Id { get; set; }

        public string Description { get; set; }

        public decimal TotalAmount { get; set; }

        public int InstallmentCount { get; set; }

        public DateTime FirstDueDate { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public DateTime CreatedAt { get; set; }

        #endregion

        #region Properties :: CategoryId, CategoryName, CategoryColor, CategoryIcon, WalletId, WalletName, WalletColor

        public Guid CategoryId { get; set; }

        public string CategoryName { get; set; }

        public string CategoryColor { get; set; }

        public string CategoryIcon { get; set; }

        public Guid? WalletId { get; set; }

        public string? WalletName { get; set; }

        public string? WalletColor { get; set; }

        #endregion

        #region Properties :: PaidCount, PaidAmount, RemainingCount, RemainingAmount, ProgressPercentage, NextDueDate, LastDueDate, IsCompleted

        public int PaidCount { get; set; }

        public decimal PaidAmount { get; set; }

        public int RemainingCount { get; set; }

        /// <summary>Quanto ainda vai vencer. É o que entra na previsão dos próximos meses.</summary>
        public decimal RemainingAmount { get; set; }

        public decimal ProgressPercentage { get; set; }

        /// <summary>Vencimento da próxima parcela em aberto. Nulo quando tudo foi pago.</summary>
        public DateTime? NextDueDate { get; set; }

        public DateTime? LastDueDate { get; set; }

        public bool IsCompleted { get; set; }

        #endregion
    }
}
