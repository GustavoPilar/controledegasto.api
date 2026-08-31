using ControleDeGasto.API.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Dados de criação ou edição de carteira.
    /// </summary>
    public class WalletRequest
    {
        #region Constants :: BALANCE_MINIMUM, BALANCE_MAXIMUM, LIMIT_MINIMUM, LIMIT_MAXIMUM

        private const double BALANCE_MINIMUM = -999_999_999.99;
        private const double BALANCE_MAXIMUM = 999_999_999.99;
        private const double LIMIT_MINIMUM = 0.01;
        private const double LIMIT_MAXIMUM = 999_999_999.99;

        #endregion

        #region Properties :: Name, Kind, Color, Icon, InitialBalance

        [Required(ErrorMessage = "Informe o nome da carteira.")]
        [StringLength(60, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 60 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [EnumDataType(typeof(WalletKind), ErrorMessage = "Tipo de carteira inválido.")]
        public WalletKind Kind { get; set; } = WalletKind.Checking;

        /// <summary>Cor em hexadecimal (#RRGGBB).</summary>
        [Required(ErrorMessage = "Informe a cor da carteira.")]
        [RegularExpression("^#([0-9a-fA-F]{6})$", ErrorMessage = "A cor deve estar no formato #RRGGBB.")]
        public string Color { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o ícone da carteira.")]
        [StringLength(40, ErrorMessage = "O ícone deve ter no máximo 40 caracteres.")]
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// Saldo já existente na carteira. Aceita negativo: uma conta pode entrar no sistema
        /// no vermelho, e recusar isso obrigaria o usuário a inventar um lançamento.
        /// </summary>
        [Range(BALANCE_MINIMUM, BALANCE_MAXIMUM, ErrorMessage = "O saldo inicial informado é inválido.")]
        public decimal InitialBalance { get; set; }

        #endregion

        #region Properties :: CreditLimit, StatementClosingDay, PaymentDueDay, IsDefault

        /// <summary>Limite do cartão. Ignorado nas demais naturezas.</summary>
        [Range(LIMIT_MINIMUM, LIMIT_MAXIMUM, ErrorMessage = "O limite deve ser maior que zero.")]
        public decimal? CreditLimit { get; set; }

        [Range(1, 31, ErrorMessage = "O dia de fechamento deve estar entre 1 e 31.")]
        public int? StatementClosingDay { get; set; }

        [Range(1, 31, ErrorMessage = "O dia de vencimento deve estar entre 1 e 31.")]
        public int? PaymentDueDay { get; set; }

        /// <summary>Marca a carteira como padrão dos novos lançamentos.</summary>
        public bool IsDefault { get; set; }

        #endregion
    }
}
