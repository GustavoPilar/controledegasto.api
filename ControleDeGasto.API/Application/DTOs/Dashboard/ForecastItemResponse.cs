using ControleDeGasto.API.Domain.Enums;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Uma linha da previsão do mês.
    /// </summary>
    public class ForecastItemResponse
    {
        #region Properties :: Source, ReferenceId, Description, Amount, Type, IsBenefitCredit, ExpectedOn, IsOverdue

        public ForecastSource Source { get; set; }

        /// <summary>
        /// Identificador da origem: a definição fixa, o lançamento previsto ou a parcela. Serve
        /// para a tela navegar do item da previsão para o registro que o gerou.
        /// </summary>
        public Guid ReferenceId { get; set; }

        public string Description { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public TransactionType Type { get; set; }

        /// <summary>
        /// Verdadeiro em crédito de benefício. Marcado separadamente porque esse valor não
        /// entra no saldo livre do mês: é dinheiro que só pode ser gasto no que o vale aceita.
        /// </summary>
        public bool IsBenefitCredit { get; set; }

        /// <summary>Data prevista, em UTC.</summary>
        public DateTime ExpectedOn { get; set; }

        /// <summary>Verdadeiro quando a data prevista já passou e o valor não foi liquidado.</summary>
        public bool IsOverdue { get; set; }

        #endregion

        #region Properties :: CategoryId, CategoryName, CategoryColor, CategoryIcon, WalletId, WalletName, WalletColor

        public Guid? CategoryId { get; set; }

        public string? CategoryName { get; set; }

        public string? CategoryColor { get; set; }

        public string? CategoryIcon { get; set; }

        public Guid? WalletId { get; set; }

        public string? WalletName { get; set; }

        public string? WalletColor { get; set; }

        #endregion

        #region Properties :: AlreadyRealizedAmount

        /// <summary>
        /// Parte do valor fixo que já apareceu como lançamento no mês.
        /// </summary>
        /// <remarks>
        /// Uma conta fixa lançada à mão não pode ser contada duas vezes. O que já foi lançado é
        /// abatido da previsão, e este campo mostra o quanto foi abatido para o número na tela
        /// ser explicável.
        /// </remarks>
        public decimal AlreadyRealizedAmount { get; set; }

        #endregion
    }
}
