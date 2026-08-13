using ControleDeGasto.API.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Movimento a registrar em um cofrinho.
    /// </summary>
    public class ContributionRequest
    {
        #region Constants :: AMOUNT_MINIMUM, AMOUNT_MAXIMUM

        private const double AMOUNT_MINIMUM = 0.01;
        private const double AMOUNT_MAXIMUM = 999_999_999.99;

        #endregion

        #region Properties :: Amount, Kind, OccurredOn, Note

        /// <summary>Valor sempre positivo. O sentido vem de <see cref="Kind"/>.</summary>
        [Range(AMOUNT_MINIMUM, AMOUNT_MAXIMUM, ErrorMessage = "O valor deve ser maior que zero.")]
        public decimal Amount { get; set; }

        [EnumDataType(typeof(ContributionKind), ErrorMessage = "Tipo de movimento inválido.")]
        public ContributionKind Kind { get; set; } = ContributionKind.Deposit;

        [Required(ErrorMessage = "Informe a data do movimento.")]
        public DateTime OccurredOn { get; set; }

        [StringLength(120, ErrorMessage = "A observação deve ter no máximo 120 caracteres.")]
        public string? Note { get; set; }

        #endregion
    }
}
