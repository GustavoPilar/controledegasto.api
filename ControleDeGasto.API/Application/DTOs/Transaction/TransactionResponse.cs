using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Lançamento devolvido ao cliente, já com os dados da categoria para a listagem não
    /// precisar de uma segunda chamada.
    /// </summary>
    public class TransactionResponse(Transaction transaction)
    {
        #region Properties :: Id, CategoryId, CategoryName, CategoryColor, CategoryIcon, Type, Amount, Description, OccurredOn, PaymentMethod, CreatedAt, UpdatedAt

        public Guid Id { get; set; } = transaction.Id;

        public Guid CategoryId { get; set; } = transaction.CategoryId;

        public string CategoryName { get; set; } = transaction.Category?.Name ?? string.Empty;

        public string CategoryColor { get; set; } = transaction.Category?.Color ?? string.Empty;

        public string CategoryIcon { get; set; } = transaction.Category?.Icon ?? string.Empty;

        /// <summary>Natureza do lançamento, derivada da categoria.</summary>
        public TransactionType Type { get; set; } = transaction.Category?.Type ?? TransactionType.Expense;

        public decimal Amount { get; set; } = transaction.Amount;

        public string Description { get; set; } = transaction.Description;

        public DateTime OccurredOn { get; set; } = transaction.OccurredOn;

        public PaymentMethod PaymentMethod { get; set; } = transaction.PaymentMethod;

        public DateTime CreatedAt { get; set; } = transaction.CreatedAt;

        public DateTime? UpdatedAt { get; set; } = transaction.UpdatedAt;

        #endregion
    }
}
