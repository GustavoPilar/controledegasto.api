using ControleDeGasto.API.Application.DTOs;
using ControleDeGasto.API.Application.Interfaces;
using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;
using ControleDeGasto.API.Domain.Interfaces;

namespace ControleDeGasto.API.Application.Services
{
    public class UserService(
        IUserRepository repository,
        ILogger<UserService> logger) : IUserService
    {
        #region Fields

        private readonly IUserRepository repository = repository;
        private readonly ILogger<UserService> logger = logger;

        #endregion

        #region Methods :: CreateUserPreferenceAsync(), GetUserPreferenceAsync(), UpdateUserPreferenceAsync()

        /// <inheritdoc />
        public async Task<UserPreference?> CreateUserPreferenceAsync(User user, UserPreferenceRequest? request)
        {
            ArgumentNullException.ThrowIfNull(user);

            // O cliente pode omitir as preferências (ex.: visitante que não tocou no seletor de
            // tema). Nesse caso a conta nasce com o tema claro.
            AppearanceType appearance = request?.Appearance ?? AppearanceType.Light;

            UserPreference userPreference = new UserPreference()
            {
                UserId = user.Id,
                Appearance = appearance,
                CreatedAt = DateTime.UtcNow
            };

            bool created = await this.repository.CreateUserPreferenceAsync(userPreference);

            if (!created)
            {
                this.logger.LogError("Falha ao criar preferência do usuário {UserId}.", user.Id);
                return null;
            }

            this.logger.LogInformation("Preferência criada para o usuário {UserId} com aparência {Appearance}.", user.Id, appearance);
            return userPreference;
        }

        /// <inheritdoc />
        public async Task<UserPreference?> GetUserPreferenceAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                return null;

            return await this.repository.GetUserPreferenceAsync(userId);
        }

        /// <inheritdoc />
        public async Task<UserPreferenceResponse?> UpdateUserPreferenceAsync(Guid userId, UserPreferenceRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (userId == Guid.Empty)
                return null;

            UserPreference? userPreference = await this.repository.GetUserPreferenceAsync(userId);

            if (userPreference is null)
            {
                this.logger.LogWarning("Preferência não encontrada para o usuário {UserId}.", userId);
                return null;
            }

            // Sem alteração real não há motivo para escrever nem para mexer no UpdatedAt.
            if (userPreference.Appearance == request.Appearance)
                return new UserPreferenceResponse(userPreference);

            userPreference.Appearance = request.Appearance;
            userPreference.UpdatedAt = DateTime.UtcNow;

            bool updated = await this.repository.UpdateUserPreferenceAsync(userPreference);

            if (!updated)
            {
                this.logger.LogError("Falha ao atualizar preferência do usuário {UserId}.", userId);
                return null;
            }

            this.logger.LogInformation("Preferência do usuário {UserId} atualizada para {Appearance}.", userId, request.Appearance);
            return new UserPreferenceResponse(userPreference);
        }

        #endregion
    }
}
