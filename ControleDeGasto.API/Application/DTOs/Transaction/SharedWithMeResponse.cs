using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Divisão que um amigo atribuiu ao usuário.
    /// </summary>
    /// <remarks>
    /// Formato próprio, e não <see cref="TransactionResponse"/>: aqui o usuário não é o dono do
    /// lançamento, e devolver o lançamento inteiro exporia a carteira, as etiquetas e as outras
    /// partes da divisão de outra pessoa.
    /// </remarks>
    public class SharedWithMeResponse
    {
        #region Constructors

        /// <summary>
        /// Cria a resposta a partir da divisão, com o lançamento e o pagador carregados.
        /// </summary>
        /// <param name="share">Divisão de origem.</param>
        public SharedWithMeResponse(TransactionShare share)
        {
            ArgumentNullException.ThrowIfNull(share);

            this.ShareId = share.Id;
            this.Amount = share.Amount;
            this.SettledAt = share.SettledAt;
            this.IsSettled = share.SettledAt.HasValue;
            this.CreatedAt = share.CreatedAt;

            Transaction? transaction = share.Transaction;

            this.Description = transaction?.Description ?? string.Empty;
            this.TotalAmount = transaction?.Amount ?? 0;
            this.OccurredOn = transaction?.OccurredOn ?? share.CreatedAt;
            this.CategoryName = transaction?.Category?.Name ?? string.Empty;
            this.CategoryColor = transaction?.Category?.Color ?? string.Empty;
            this.CategoryIcon = transaction?.Category?.Icon ?? string.Empty;
            this.Type = transaction?.Category?.Type ?? TransactionType.Expense;

            this.PaidByUserId = transaction?.UserId ?? Guid.Empty;
            this.PaidByFullName = transaction?.User?.FullName ?? string.Empty;
            this.PaidByUserName = transaction?.User?.UserName ?? string.Empty;
        }

        #endregion

        #region Properties :: ShareId, Amount, SettledAt, IsSettled, CreatedAt

        public Guid ShareId { get; set; }

        /// <summary>Quanto cabe ao usuário nesta compra.</summary>
        public decimal Amount { get; set; }

        public DateTime? SettledAt { get; set; }

        public bool IsSettled { get; set; }

        public DateTime CreatedAt { get; set; }

        #endregion

        #region Properties :: Description, TotalAmount, OccurredOn, CategoryName, CategoryColor, CategoryIcon, Type

        public string Description { get; set; }

        /// <summary>Valor total da compra, para o usuário conferir a proporção da sua parte.</summary>
        public decimal TotalAmount { get; set; }

        public DateTime OccurredOn { get; set; }

        public string CategoryName { get; set; }

        public string CategoryColor { get; set; }

        public string CategoryIcon { get; set; }

        public TransactionType Type { get; set; }

        #endregion

        #region Properties :: PaidByUserId, PaidByFullName, PaidByUserName

        public Guid PaidByUserId { get; set; }

        public string PaidByFullName { get; set; }

        public string PaidByUserName { get; set; }

        #endregion
    }
}
