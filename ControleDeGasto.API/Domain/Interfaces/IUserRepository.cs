using ControleDeGasto.API.Domain.Entities;

namespace ControleDeGasto.API.Domain.Interfaces
{
    public interface IUserRepository
    {
        #region Methods :: CreateUserPreferenceAsync(), GetUserPreferenceAsync(), UpdateUserPreferenceAsync()

        /// <summary>
        /// Persiste a preferência informada.
        /// </summary>
        /// <param name="userPreference">Preferência a gravar.</param>
        /// <returns>True se a gravação afetou alguma linha.</returns>
        Task<bool> CreateUserPreferenceAsync(UserPreference userPreference);

        /// <summary>
        /// Obtém a preferência de um usuário.
        /// </summary>
        /// <param name="userId">Identificador do usuário.</param>
        /// <returns>A preferência, ou nulo se o usuário não tiver uma.</returns>
        Task<UserPreference?> GetUserPreferenceAsync(Guid userId);

        /// <summary>
        /// Persiste alterações feitas em uma preferência já rastreada.
        /// </summary>
        /// <param name="userPreference">Preferência alterada.</param>
        /// <returns>True se a gravação afetou alguma linha.</returns>
        Task<bool> UpdateUserPreferenceAsync(UserPreference userPreference);

        #endregion

        #region Methods :: GetActiveUserIdsAsync()

        /// <summary>
        /// Lista os identificadores dos usuários ativos com e-mail confirmado.
        /// </summary>
        /// <remarks>
        /// Usada pela rotina de notificações. Devolve só os identificadores para não trazer
        /// a base de usuários inteira para a memória.
        /// </remarks>
        /// <returns>Identificadores dos usuários elegíveis a receber avisos.</returns>
        Task<IReadOnlyList<Guid>> GetActiveUserIdsAsync();

        #endregion
    }
}
