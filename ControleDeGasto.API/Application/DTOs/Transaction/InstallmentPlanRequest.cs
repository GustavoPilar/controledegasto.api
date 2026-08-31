using ControleDeGasto.API.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Compra parcelada a registrar. As parcelas são geradas pelo servidor.
    /// </summary>
    /// <remarks>
    /// O cliente envia o total e a quantidade de parcelas, não as parcelas prontas: o rateio
    /// dos centavos precisa fechar exatamente com o total, e deixar isso no cliente permitiria
    /// um plano de doze parcelas que soma um centavo a menos que a compra.
    /// </remarks>
    public class InstallmentPlanRequest
    {
        #region Constants :: AMOUNT_MINIMUM, AMOUNT_MAXIMUM, MIN_INSTALLMENTS, MAX_INSTALLMENTS, MAX_TAGS

        private const double AMOUNT_MINIMUM = 0.02;
        private const double AMOUNT_MAXIMUM = 999_999_999.99;

        public const int MIN_INSTALLMENTS = 2;
        public const int MAX_INSTALLMENTS = 360;
        public const int MAX_TAGS = 10;

        #endregion

        #region Properties :: CategoryId, WalletId, Description, TotalAmount

        [Required(ErrorMessage = "Informe a categoria.")]
        public Guid CategoryId { get; set; }

        /// <summary>Carteira que paga as parcelas. Nula usa a carteira padrão, se houver.</summary>
        public Guid? WalletId { get; set; }

        [Required(ErrorMessage = "Informe a descrição da compra.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "A descrição deve ter entre 2 e 100 caracteres.")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Valor total da compra. O mínimo é dois centavos porque o total precisa render pelo
        /// menos um centavo por parcela.
        /// </summary>
        [Range(AMOUNT_MINIMUM, AMOUNT_MAXIMUM, ErrorMessage = "O valor total deve ser maior que zero.")]
        public decimal TotalAmount { get; set; }

        #endregion

        #region Properties :: InstallmentCount, FirstDueDate, PaymentMethod, TagIds

        [Range(MIN_INSTALLMENTS, MAX_INSTALLMENTS, ErrorMessage = "A quantidade de parcelas deve estar entre 2 e 360.")]
        public int InstallmentCount { get; set; } = MIN_INSTALLMENTS;

        [Required(ErrorMessage = "Informe o vencimento da primeira parcela.")]
        public DateTime FirstDueDate { get; set; }

        [EnumDataType(typeof(PaymentMethod), ErrorMessage = "Forma de pagamento inválida.")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CreditCard;

        /// <summary>Etiquetas aplicadas a todas as parcelas geradas.</summary>
        [MaxLength(MAX_TAGS, ErrorMessage = "São permitidas no máximo 10 etiquetas.")]
        public IReadOnlyList<Guid> TagIds { get; set; } = [];

        #endregion
    }
}
