using ControleDeGasto.API.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Dados de criação ou edição de um valor fixo mensal.
    /// </summary>
    public class FixedEntryRequest
    {
        #region Constants :: AMOUNT_MINIMUM, AMOUNT_MAXIMUM

        private const double AMOUNT_MINIMUM = 0.01;
        private const double AMOUNT_MAXIMUM = 999_999_999.99;

        #endregion

        #region Properties :: Kind, CategoryId, WalletId, Description, Amount

        [EnumDataType(typeof(FixedEntryKind), ErrorMessage = "Tipo de valor fixo inválido.")]
        public FixedEntryKind Kind { get; set; } = FixedEntryKind.Expense;

        /// <summary>
        /// Categoria da previsão. Obrigatória em entrada e saída; ignorada no crédito de
        /// benefício, que não classifica gasto algum.
        /// </summary>
        public Guid? CategoryId { get; set; }

        /// <summary>Carteira de origem ou destino. Obrigatória no crédito de benefício.</summary>
        public Guid? WalletId { get; set; }

        [Required(ErrorMessage = "Informe a descrição.")]
        [StringLength(120, MinimumLength = 2, ErrorMessage = "A descrição deve ter entre 2 e 120 caracteres.")]
        public string Description { get; set; } = string.Empty;

        [Range(AMOUNT_MINIMUM, AMOUNT_MAXIMUM, ErrorMessage = "O valor deve ser maior que zero.")]
        public decimal Amount { get; set; }

        #endregion

        #region Properties :: DayOfMonth, StartsOn, EndsOn, Active

        [Range(1, 31, ErrorMessage = "O dia deve estar entre 1 e 31.")]
        public int DayOfMonth { get; set; } = 1;

        [Required(ErrorMessage = "Informe o mês em que passa a valer.")]
        public DateTime StartsOn { get; set; }

        /// <summary>Último mês de vigência. Nulo enquanto não tem previsão de fim.</summary>
        public DateTime? EndsOn { get; set; }

        public bool Active { get; set; } = true;

        #endregion
    }
}
