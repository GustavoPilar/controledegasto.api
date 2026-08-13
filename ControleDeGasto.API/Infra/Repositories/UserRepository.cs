using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Interfaces;
using ControleDeGasto.API.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControleDeGasto.API.Infra.Repositories
{
    public class UserRepository(
        AppDbContext context) : IUserRepository
    {
        #region Fields

        private readonly AppDbContext context = context;

        #endregion

        #region Methods :: CreateUserPreferenceAsync(), GetUserPreferenceAsync(), UpdateUserPreferenceAsync()

        /// <inheritdoc />
        public async Task<bool> CreateUserPreferenceAsync(UserPreference userPreference)
        {
            ArgumentNullException.ThrowIfNull(userPreference);

            await this.context.UserPreferences.AddAsync(userPreference);

            // O SaveChanges vive aqui porque o chamador não tem outro ponto de commit: sem ele
            // o Add ficava pendente e a preferência nunca chegava ao banco.
            int affected = await this.context.SaveChangesAsync();

            return affected > 0;
        }

        /// <inheritdoc />
        public async Task<UserPreference?> GetUserPreferenceAsync(Guid userId)
        {
            return await this.context.UserPreferences
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }

        /// <inheritdoc />
        public async Task<bool> UpdateUserPreferenceAsync(UserPreference userPreference)
        {
            ArgumentNullException.ThrowIfNull(userPreference);

            this.context.UserPreferences.Update(userPreference);

            int affected = await this.context.SaveChangesAsync();

            return affected > 0;
        }

        #endregion

        #region Methods :: GetActiveUserIdsAsync()

        /// <inheritdoc />
        public async Task<IReadOnlyList<Guid>> GetActiveUserIdsAsync()
        {
            return await this.context.Users
                .AsNoTracking()
                .Where(x => x.Active && x.EmailConfirmed)
                .Select(x => x.Id)
                .ToListAsync();
        }

        #endregion
    }
}
