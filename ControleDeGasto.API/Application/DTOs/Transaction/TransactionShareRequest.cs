using System.ComponentModel.DataAnnotations;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Parte de uma compra atribuída a um amigo.
    /// </summary>
    /// <remarks>
    /// O valor é absoluto, e não percentual: o cliente calcula a divisão como quiser (meio a
    /// meio, por item, proporcional) e envia o resultado. Aceitar percentual no servidor
    /// obrigaria a decidir quem fica com o centavo do arredondamento sem saber o critério.
    /// </remarks>
    public class TransactionShareRequest
    {
        #region Constants :: AMOUNT_MINIMUM, AMOUNT_MAXIMUM

        private const double AMOUNT_MINIMUM = 0.01;
        private const double AMOUNT_MAXIMUM = 999_999_999.99;

        #endregion

        #region Properties :: FriendUserId, Amount

        [Required(ErrorMessage = "Informe o amigo da divisão.")]
        public Guid FriendUserId { get; set; }

        [Range(AMOUNT_MINIMUM, AMOUNT_MAXIMUM, ErrorMessage = "O valor da parte deve ser maior que zero.")]
        public decimal Amount { get; set; }

        #endregion
    }
}
