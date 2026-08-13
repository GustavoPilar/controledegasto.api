using System.ComponentModel.DataAnnotations;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Dados de criação ou edição de cofrinho.
    /// </summary>
    public class SavingsGoalRequest
    {
        #region Constants :: AMOUNT_MINIMUM, AMOUNT_MAXIMUM

        private const double AMOUNT_MINIMUM = 0.01;
        private const double AMOUNT_MAXIMUM = 999_999_999.99;

        #endregion

        #region Properties :: Name, TargetAmount, Deadline, Color, Icon, IsEmergencyReserve

        [Required(ErrorMessage = "Informe o nome do cofrinho.")]
        [StringLength(60, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 60 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [Range(AMOUNT_MINIMUM, AMOUNT_MAXIMUM, ErrorMessage = "A meta deve ser maior que zero.")]
        public decimal TargetAmount { get; set; }

        /// <summary>Prazo desejado. Nulo quando o objetivo não tem data.</summary>
        public DateTime? Deadline { get; set; }

        /// <summary>Cor em hexadecimal (#RRGGBB).</summary>
        [Required(ErrorMessage = "Informe a cor do cofrinho.")]
        [RegularExpression("^#([0-9a-fA-F]{6})$", ErrorMessage = "A cor deve estar no formato #RRGGBB.")]
        public string Color { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o ícone do cofrinho.")]
        [StringLength(40, ErrorMessage = "O ícone deve ter no máximo 40 caracteres.")]
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// Marca o cofrinho como reserva de emergência. O serviço rejeita a segunda reserva
        /// do mesmo usuário.
        /// </summary>
        public bool IsEmergencyReserve { get; set; }

        #endregion
    }
}
