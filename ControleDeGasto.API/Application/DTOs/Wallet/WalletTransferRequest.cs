using System.ComponentModel.DataAnnotations;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Transferência a registrar entre duas carteiras.
    /// </summary>
    public class WalletTransferRequest
    {
        #region Constants :: AMOUNT_MINIMUM, AMOUNT_MAXIMUM

        private const double AMOUNT_MINIMUM = 0.01;
        private const double AMOUNT_MAXIMUM = 999_999_999.99;

        #endregion

        #region Properties :: FromWalletId, ToWalletId, Amount, OccurredOn, Note

        [Required(ErrorMessage = "Informe a carteira de origem.")]
        public Guid FromWalletId { get; set; }

        [Required(ErrorMessage = "Informe a carteira de destino.")]
        public Guid ToWalletId { get; set; }

        [Range(AMOUNT_MINIMUM, AMOUNT_MAXIMUM, ErrorMessage = "O valor deve ser maior que zero.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Informe a data da transferência.")]
        public DateTime OccurredOn { get; set; }

        [StringLength(120, ErrorMessage = "A observação deve ter no máximo 120 caracteres.")]
        public string? Note { get; set; }

        #endregion
    }
}
