using ControleDeGasto.API.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Dados de criação ou edição de categoria.
    /// </summary>
    /// <remarks>
    /// O dono não vem no corpo: é resolvido pela identidade autenticada. Aceitar UserId do
    /// cliente permitiria criar categoria na conta de outra pessoa.
    /// </remarks>
    public class CategoryRequest
    {
        #region Properties :: Name, Type, Color, Icon

        [Required(ErrorMessage = "Informe o nome da categoria.")]
        [StringLength(60, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 60 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [EnumDataType(typeof(TransactionType), ErrorMessage = "Tipo de categoria inválido.")]
        public TransactionType Type { get; set; }

        /// <summary>Cor em hexadecimal (#RRGGBB).</summary>
        [Required(ErrorMessage = "Informe a cor da categoria.")]
        [RegularExpression("^#([0-9a-fA-F]{6})$", ErrorMessage = "A cor deve estar no formato #RRGGBB.")]
        public string Color { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o ícone da categoria.")]
        [StringLength(40, ErrorMessage = "O ícone deve ter no máximo 40 caracteres.")]
        public string Icon { get; set; } = string.Empty;

        #endregion
    }
}
