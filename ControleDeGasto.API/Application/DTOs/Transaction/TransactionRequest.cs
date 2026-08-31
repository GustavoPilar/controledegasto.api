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
        #region Constants :: AMOUNT_MINIMUM, AMOUNT_MAXIMUM, MAX_TAGS, MAX_SHARES

        private const double AMOUNT_MINIMUM = 0.01;
        private const double AMOUNT_MAXIMUM = 999_999_999.99;

        /// <summary>Teto de etiquetas por lançamento. Existe para limitar o custo da gravação.</summary>
        public const int MAX_TAGS = 10;

        /// <summary>Teto de participantes em uma divisão.</summary>
        public const int MAX_SHARES = 20;

        #endregion

        #region Properties :: CategoryId, WalletId, Amount, Description, OccurredOn, PaymentMethod

        [Required(ErrorMessage = "Informe a categoria.")]
        public Guid CategoryId { get; set; }

        /// <summary>Carteira que paga ou recebe. Nula usa a carteira padrão, se houver.</summary>
        public Guid? WalletId { get; set; }

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

        #region Properties :: Status, DueDate

        [EnumDataType(typeof(TransactionStatus), ErrorMessage = "Situação inválida.")]
        public TransactionStatus Status { get; set; } = TransactionStatus.Settled;

        /// <summary>
        /// Vencimento da conta. Nulo em lançamento que não é conta a pagar ou receber.
        /// </summary>
        public DateTime? DueDate { get; set; }

        #endregion

        #region Properties :: TagIds, Shares

        /// <summary>Etiquetas a aplicar. Lista vazia remove todas as existentes.</summary>
        [MaxLength(MAX_TAGS, ErrorMessage = "São permitidas no máximo 10 etiquetas por lançamento.")]
        public IReadOnlyList<Guid> TagIds { get; set; } = [];

        /// <summary>
        /// Divisão da compra entre amigos. Lista vazia remove a divisão existente.
        /// </summary>
        [MaxLength(MAX_SHARES, ErrorMessage = "São permitidos no máximo 20 participantes na divisão.")]
        public IReadOnlyList<TransactionShareRequest> Shares { get; set; } = [];

        #endregion
    }
}
