using System.ComponentModel.DataAnnotations;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Dados de criação ou edição de etiqueta.
    /// </summary>
    public class TagRequest
    {
        #region Properties :: Name, Color

        [Required(ErrorMessage = "Informe o nome da etiqueta.")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 30 caracteres.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Cor em hexadecimal (#RRGGBB).</summary>
        [Required(ErrorMessage = "Informe a cor da etiqueta.")]
        [RegularExpression("^#([0-9a-fA-F]{6})$", ErrorMessage = "A cor deve estar no formato #RRGGBB.")]
        public string Color { get; set; } = string.Empty;

        #endregion
    }
}
