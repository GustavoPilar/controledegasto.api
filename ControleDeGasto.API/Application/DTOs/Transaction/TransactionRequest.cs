using ControleDeGasto.API.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Dados de criação ou edição de lançamento.
    /// </summary>
    /// <remarks>
    /// A natureza (entrada/saída) não é enviada: vem da categoria escolhida, que é validada
    /// como pertencente ao usuário autenticado.
    /// </remarks>
    public class TransactionRequest
    {
        #region Constants :: AMOUNT_MINIMUM, AMOUNT_MAXIMUM

        private const double AMOUNT_MINIMUM = 0.01;
        private const double AMOUNT_MAXIMUM = 999_999_999.99;

        #endregion

        #region Properties :: CategoryId, Amount, Description, OccurredOn, PaymentMethod

        [Required(ErrorMessage = "Informe a categoria.")]
        public Guid CategoryId { get; set; }

        [Range(AMOUNT_MINIMUM, AMOUNT_MAXIMUM, ErrorMessage = "O valor deve ser maior que zero.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Informe a descrição.")]
        [StringLength(120, MinimumLength = 2, ErrorMessage = "A descrição deve ter entre 2 e 120 caracteres.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a data do lançamento.")]
        public DateTime OccurredOn { get; set; }

        [EnumDataType(typeof(PaymentMethod), ErrorMessage = "Forma de pagamento inválida.")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Other;

        #endregion
    }
}
