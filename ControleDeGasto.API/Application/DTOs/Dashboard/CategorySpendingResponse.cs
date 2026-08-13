using ControleDeGasto.API.Domain.Enums;
using ControleDeGasto.API.Domain.ReadModels;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Quanto uma categoria movimentou no período e o peso dela no total.
    /// </summary>
    /// <remarks>
    /// É a resposta da pergunta "onde estou gastando mais": o cliente recebe já ordenado e
    /// com o percentual pronto para o gráfico.
    /// </remarks>
    public class CategorySpendingResponse
    {
        #region Constructors

        /// <summary>
        /// Cria a resposta a partir do total apurado e do total geral do período.
        /// </summary>
        /// <param name="categoryTotal">Total da categoria vindo do banco.</param>
        /// <param name="periodTotal">Total do período, usado para calcular o percentual.</param>
        public CategorySpendingResponse(CategoryTotal categoryTotal, decimal periodTotal)
        {
            ArgumentNullException.ThrowIfNull(categoryTotal);

            this.CategoryId = categoryTotal.CategoryId;
            this.CategoryName = categoryTotal.CategoryName;
            this.Color = categoryTotal.Color;
            this.Icon = categoryTotal.Icon;
            this.Type = categoryTotal.Type;
            this.Total = categoryTotal.Total;
            this.TransactionCount = categoryTotal.TransactionCount;

            this.Percentage = periodTotal <= 0
                ? 0
                : Math.Round(categoryTotal.Total / periodTotal * 100, 2);
        }

        #endregion

        #region Properties :: CategoryId, CategoryName, Color, Icon, Type, Total, TransactionCount, Percentage

        public Guid CategoryId { get; set; }

        public string CategoryName { get; set; }

        public string Color { get; set; }

        public string Icon { get; set; }

        public TransactionType Type { get; set; }

        public decimal Total { get; set; }

        public int TransactionCount { get; set; }

        /// <summary>Participação da categoria no total do período, em percentual.</summary>
        public decimal Percentage { get; set; }

        #endregion
    }
}
