using ControleDeGasto.API.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Preferências enviadas pelo cliente no cadastro ou na atualização.
    /// </summary>
    /// <remarks>
    /// No cadastro, carrega o tema que o visitante escolheu na tela de login antes de existir
    /// conta. Quando o objeto não vem na requisição, o servidor assume <see cref="AppearanceType.Light"/>.
    /// </remarks>
    public class UserPreferenceRequest
    {
        #region Properties :: Appearance

        /// <summary>Tema de interface escolhido.</summary>
        [EnumDataType(typeof(AppearanceType), ErrorMessage = "Aparência inválida.")]
        public AppearanceType Appearance { get; set; } = AppearanceType.Light;

        #endregion
    }
}
