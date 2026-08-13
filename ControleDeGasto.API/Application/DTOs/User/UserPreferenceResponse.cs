using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Preferências de interface devolvidas ao cliente.
    /// </summary>
    public class UserPreferenceResponse(UserPreference userPreference)
    {
        #region Properties :: Appearance, UpdatedAt

        /// <summary>Tema de interface em vigor para a conta.</summary>
        public AppearanceType Appearance { get; set; } = userPreference.Appearance;

        /// <summary>Momento da última alteração, em UTC. Nulo enquanto nunca foi alterada.</summary>
        public DateTime? UpdatedAt { get; set; } = userPreference.UpdatedAt;

        #endregion
    }
}
