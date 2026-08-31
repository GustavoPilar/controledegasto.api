using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;
using ControleDeGasto.API.Domain.Interfaces;
using ControleDeGasto.API.Domain.ReadModels;
using ControleDeGasto.API.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControleDeGasto.API.Infra.Repositories
{
    public class FriendshipRepository(
        AppDbContext context) : IFriendshipRepository
    {
        #region Fields

        private readonly AppDbContext context = context;

        #endregion

        #region Helpers :: AcceptedBetween()

        /// <summary>
        /// Consulta base das amizades aceitas em que o usuário participa.
        /// </summary>
        /// <param name="userId">Usuário de referência.</param>
        /// <returns>Consulta sem rastreamento das amizades aceitas.</returns>
        private IQueryable<Friendship> AcceptedOf(Guid userId)
        {
            return this.context.Friendships
                .AsNoTracking()
                .Where(x => x.Status == FriendshipStatus.Accepted
                    && (x.RequesterId == userId || x.AddresseeId == userId));
        }

        #endregion

        #region Methods :: GetByUsersAsync(), GetByIdAsync(), GetByStatusAsync(), AreFriendsAsync(), FilterFriendsAsync()

        /// <inheritdoc />
        public async Task<Friendship?> GetByUsersAsync(Guid userId, Guid otherUserId)
        {
            return await this.context.Friendships
                .FirstOrDefaultAsync(x =>
                    (x.RequesterId == userId && x.AddresseeId == otherUserId)
                    || (x.RequesterId == otherUserId && x.AddresseeId == userId));
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Friendship>> GetByUsersManyAsync(Guid userId, IReadOnlyList<Guid> otherUserIds)
        {
            ArgumentNullException.ThrowIfNull(otherUserIds);

            if (otherUserIds.Count == 0)
                return [];

            return await this.context.Friendships
                .AsNoTracking()
                .Where(x =>
                    (x.RequesterId == userId && otherUserIds.Contains(x.AddresseeId))
                    || (x.AddresseeId == userId && otherUserIds.Contains(x.RequesterId)))
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<Friendship?> GetByIdAsync(Guid userId, Guid friendshipId)
        {
            return await this.context.Friendships
                .FirstOrDefaultAsync(x => x.Id == friendshipId
                    && (x.RequesterId == userId || x.AddresseeId == userId));
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Friendship>> GetByStatusAsync(Guid userId, FriendshipStatus status)
        {
            return await this.context.Friendships
                .AsNoTracking()
                .Include(x => x.Requester)
                .Include(x => x.Addressee)
                .Where(x => x.Status == status && (x.RequesterId == userId || x.AddresseeId == userId))
                .OrderByDescending(x => x.RespondedAt ?? x.RequestedAt)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<bool> AreFriendsAsync(Guid userId, Guid otherUserId)
        {
            if (userId == otherUserId)
                return false;

            return await this.AcceptedOf(userId)
                .AnyAsync(x => x.RequesterId == otherUserId || x.AddresseeId == otherUserId);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Guid>> FilterFriendsAsync(Guid userId, IReadOnlyList<Guid> candidateUserIds)
        {
            ArgumentNullException.ThrowIfNull(candidateUserIds);

            if (candidateUserIds.Count == 0)
                return [];

            // Projeta o outro lado da relação e cruza com os candidatos: uma ida ao banco,
            // independente da quantidade de participantes informada.
            return await this.AcceptedOf(userId)
                .Select(x => x.RequesterId == userId ? x.AddresseeId : x.RequesterId)
                .Where(friendId => candidateUserIds.Contains(friendId))
                .ToListAsync();
        }

        #endregion

        #region Methods :: GetFriendIdsAsync(), CreateAsync(), UpdateAsync(), DeleteAsync()

        /// <inheritdoc />
        public async Task<IReadOnlyList<Guid>> GetFriendIdsAsync(Guid userId)
        {
            return await this.AcceptedOf(userId)
                .Select(x => x.RequesterId == userId ? x.AddresseeId : x.RequesterId)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<bool> CreateAsync(Friendship friendship)
        {
            ArgumentNullException.ThrowIfNull(friendship);

            await this.context.Friendships.AddAsync(friendship);

            return await this.context.SaveChangesAsync() > 0;
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(Friendship friendship)
        {
            ArgumentNullException.ThrowIfNull(friendship);

            this.context.Friendships.Update(friendship);

            return await this.context.SaveChangesAsync() > 0;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(Friendship friendship)
        {
            ArgumentNullException.ThrowIfNull(friendship);

            this.context.Friendships.Remove(friendship);

            return await this.context.SaveChangesAsync() > 0;
        }

        #endregion

        #region Methods :: SearchUsersAsync(), GetUserSummariesAsync()

        /// <inheritdoc />
        public async Task<IReadOnlyList<UserSummary>> SearchUsersAsync(Guid userId, string term, int limit)
        {
            if (string.IsNullOrWhiteSpace(term))
                return [];

            string search = term.Trim();

            // O e-mail casa apenas por igualdade, nunca por trecho: busca parcial em e-mail
            // permitiria varrer a base de endereços testando prefixos.
            string normalizedEmail = search.ToUpperInvariant();

            return await this.context.Users
                .AsNoTracking()
                .Where(x => x.Id != userId
                    && x.Active
                    && x.EmailConfirmed
                    && (EF.Functions.ILike(x.UserName!, $"%{search}%") || x.NormalizedEmail == normalizedEmail))
                .OrderBy(x => x.UserName)
                .Take(limit)
                .Select(x => new UserSummary
                {
                    UserId = x.Id,
                    FullName = x.FullName,
                    UserName = x.UserName!
                })
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<UserSummary>> GetUserSummariesAsync(IReadOnlyList<Guid> userIds)
        {
            ArgumentNullException.ThrowIfNull(userIds);

            if (userIds.Count == 0)
                return [];

            return await this.context.Users
                .AsNoTracking()
                .Where(x => userIds.Contains(x.Id))
                .Select(x => new UserSummary
                {
                    UserId = x.Id,
                    FullName = x.FullName,
                    UserName = x.UserName!
                })
                .ToListAsync();
        }

        #endregion
    }
}
