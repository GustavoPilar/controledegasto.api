using ControleDeGasto.API.Application.DTOs;
using ControleDeGasto.API.Domain.Entities;

namespace ControleDeGasto.API.Application.Interfaces
{
    public interface IUserService
    {
        #region Methods :: CreateUserPreferenceAsync(), GetUserPreferenceAsync(), UpdateUserPreferenceAsync()

        /// <summary>
        /// Cria a preferência inicial de um usuário recém-cadastrado.
        /// </summary>
        /// <param name="user">Usuário dono da preferência.</param>
        /// <param name="request">Preferências enviadas pelo cliente. Nulo assume o tema claro.</param>
        /// <returns>A preferência criada, ou nulo se a gravação falhar.</returns>
        Task<UserPreference?> CreateUserPreferenceAsync(User user, UserPreferenceRequest? request);

        /// <summary>
        /// Obtém as preferências de um usuário.
        /// </summary>
        /// <param name="userId">Identificador do usuário.</param>
        /// <returns>As preferências, ou nulo se o usuário não tiver.</returns>
        Task<UserPreference?> GetUserPreferenceAsync(Guid userId);

        /// <summary>
        /// Atualiza as preferências de um usuário.
        /// </summary>
        /// <param name="userId">Identificador do usuário.</param>
        /// <param name="request">Novas preferências.</param>
        /// <returns>As preferências atualizadas, ou nulo se o usuário não tiver preferência gravada.</returns>
        Task<UserPreferenceResponse?> UpdateUserPreferenceAsync(Guid userId, UserPreferenceRequest request);

        #endregion
    }
}
