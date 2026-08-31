using ControleDeGasto.API.Domain.ReadModels;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Quanto uma etiqueta movimentou em um período.
    /// </summary>
    public class TagTotalResponse(TagTotal total)
    {
        #region Properties :: TagId, TagName, Color, IncomeTotal, ExpenseTotal, Balance, TransactionCount

        public Guid TagId { get; set; } = total.TagId;

        public string TagName { get; set; } = total.TagName;

        public string Color { get; set; } = total.Color;

        public decimal IncomeTotal { get; set; } = total.IncomeTotal;

        public decimal ExpenseTotal { get; set; } = total.ExpenseTotal;

        /// <summary>Entradas menos saídas marcadas com a etiqueta.</summary>
        public decimal Balance { get; set; } = total.IncomeTotal - total.ExpenseTotal;

        public int TransactionCount { get; set; } = total.TransactionCount;

        #endregion
    }
}
