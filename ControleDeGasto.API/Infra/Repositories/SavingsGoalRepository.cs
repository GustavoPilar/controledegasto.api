using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;
using ControleDeGasto.API.Domain.Interfaces;
using ControleDeGasto.API.Domain.ReadModels;
using ControleDeGasto.API.Infra.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ControleDeGasto.API.Infra.Repositories
{
    public class SavingsGoalRepository(
        AppDbContext context) : ISavingsGoalRepository
    {
        #region Fields

        private readonly AppDbContext context = context;

        #endregion

        #region Methods :: GetAllAsync(), GetByIdAsync(), GetEmergencyReserveAsync(), ExistsByNameAsync()

        /// <inheritdoc />
        public async Task<IReadOnlyList<SavingsGoal>> GetAllAsync(Guid userId, bool includeArchived)
        {
            // O recorte é a participação: um cofrinho compartilhado aparece para quem foi
            // convidado, mesmo não sendo o criador.
            IQueryable<SavingsGoal> query = this.context.SavingsGoals
                .AsNoTracking()
                .Where(x => x.Members!.Any(member => member.UserId == userId));

            if (!includeArchived)
                query = query.Where(x => x.Status != SavingsGoalStatus.Archived);

            // A reserva de emergência vem primeiro: é o cofrinho que a interface destaca.
            return await query
                .Include(x => x.Members!)
                    .ThenInclude(x => x.User)
                .OrderByDescending(x => x.IsEmergencyReserve)
                .ThenBy(x => x.Name)
                .AsSplitQuery()
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<SavingsGoal?> GetByIdAsync(Guid userId, Guid savingsGoalId)
        {
            return await this.context.SavingsGoals
                .Include(x => x.Members!)
                    .ThenInclude(x => x.User)
                .AsSplitQuery()
                .FirstOrDefaultAsync(x => x.Id == savingsGoalId
                    && x.Members!.Any(member => member.UserId == userId));
        }

        /// <inheritdoc />
        public async Task<SavingsGoal?> GetEmergencyReserveAsync(Guid userId)
        {
            return await this.context.SavingsGoals
                .FirstOrDefaultAsync(x => x.UserId == userId && x.IsEmergencyReserve);
        }

        /// <inheritdoc />
        public async Task<bool> ExistsByNameAsync(Guid userId, string name, Guid? excludeSavingsGoalId)
        {
            IQueryable<SavingsGoal> query = this.context.SavingsGoals
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.Name.ToLower() == name.ToLower());

            if (excludeSavingsGoalId.HasValue)
                query = query.Where(x => x.Id != excludeSavingsGoalId.Value);

            return await query.AnyAsync();
        }

        #endregion

        #region Methods :: CreateAsync(), UpdateAsync(), DeleteAsync()

        /// <inheritdoc />
        public async Task<bool> CreateAsync(SavingsGoal savingsGoal)
        {
            ArgumentNullException.ThrowIfNull(savingsGoal);

            // Transação explícita: sem a linha de participação do criador, o cofrinho ficaria
            // invisível até para ele, já que o acesso é decidido pela participação.
            await using IDbContextTransaction dbTransaction = await this.context.Database.BeginTransactionAsync();

            try
            {
                await this.context.SavingsGoals.AddAsync(savingsGoal);

                await this.context.SavingsGoalMembers.AddAsync(new SavingsGoalMember()
                {
                    Id = Guid.NewGuid(),
                    SavingsGoalId = savingsGoal.Id,
                    UserId = savingsGoal.UserId,
                    Role = SavingsGoalMemberRole.Owner,
                    JoinedAt = DateTime.UtcNow
                });

                int affected = await this.context.SaveChangesAsync();

                await dbTransaction.CommitAsync();

                return affected > 0;
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(SavingsGoal savingsGoal)
        {
            ArgumentNullException.ThrowIfNull(savingsGoal);

            this.context.SavingsGoals.Update(savingsGoal);

            return await this.context.SaveChangesAsync() > 0;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(SavingsGoal savingsGoal)
        {
            ArgumentNullException.ThrowIfNull(savingsGoal);

            this.context.SavingsGoals.Remove(savingsGoal);

            return await this.context.SaveChangesAsync() > 0;
        }

        #endregion

        #region Methods :: GetMembersAsync(), GetMemberAsync(), AddMemberAsync(), RemoveMemberAsync(), IsMemberAsync()

        /// <inheritdoc />
        public async Task<IReadOnlyList<SavingsGoalMember>> GetMembersAsync(Guid savingsGoalId)
        {
            return await this.context.SavingsGoalMembers
                .AsNoTracking()
                .Include(x => x.User)
                .Where(x => x.SavingsGoalId == savingsGoalId)
                .OrderBy(x => x.Role)
                .ThenBy(x => x.JoinedAt)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<SavingsGoalMember?> GetMemberAsync(Guid savingsGoalId, Guid userId)
        {
            return await this.context.SavingsGoalMembers
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.SavingsGoalId == savingsGoalId && x.UserId == userId);
        }

        /// <inheritdoc />
        public async Task<bool> AddMemberAsync(SavingsGoalMember member)
        {
            ArgumentNullException.ThrowIfNull(member);

            await this.context.SavingsGoalMembers.AddAsync(member);

            return await this.context.SaveChangesAsync() > 0;
        }

        /// <inheritdoc />
        public async Task<bool> RemoveMemberAsync(SavingsGoalMember member)
        {
            ArgumentNullException.ThrowIfNull(member);

            this.context.SavingsGoalMembers.Remove(member);

            return await this.context.SaveChangesAsync() > 0;
        }

        /// <inheritdoc />
        public async Task<bool> IsMemberAsync(Guid savingsGoalId, Guid userId)
        {
            return await this.context.SavingsGoalMembers
                .AsNoTracking()
                .AnyAsync(x => x.SavingsGoalId == savingsGoalId && x.UserId == userId);
        }

        #endregion

        #region Methods :: GetBalanceAsync(), GetBalancesAsync(), GetMemberBalancesAsync()

        /// <inheritdoc />
        public async Task<decimal> GetBalanceAsync(Guid savingsGoalId)
        {
            // A soma sai do banco em uma linha, considerando os aportes de todos os
            // participantes. O sinal vem do tipo do movimento, por isso o CASE.
            return await this.context.SavingsGoalContributions
                .AsNoTracking()
                .Where(x => x.SavingsGoalId == savingsGoalId)
                .SumAsync(x => x.Kind == ContributionKind.Deposit ? x.Amount : -x.Amount);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<GoalBalance>> GetBalancesAsync(Guid userId)
        {
            // Uma consulta agrupada para todos os cofrinhos em que o usuário participa, em vez
            // de uma por cofrinho: evita o N+1 na listagem e no painel.
            return await this.context.SavingsGoalContributions
                .AsNoTracking()
                .Where(x => x.SavingsGoal!.Members!.Any(member => member.UserId == userId))
                .GroupBy(x => x.SavingsGoalId)
                .Select(group => new GoalBalance
                {
                    SavingsGoalId = group.Key,
                    Balance = group.Sum(x => x.Kind == ContributionKind.Deposit ? x.Amount : -x.Amount)
                })
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyDictionary<Guid, decimal>> GetMemberBalancesAsync(Guid savingsGoalId)
        {
            List<KeyValuePair<Guid, decimal>> balances = await this.context.SavingsGoalContributions
                .AsNoTracking()
                .Where(x => x.SavingsGoalId == savingsGoalId)
                .GroupBy(x => x.UserId)
                .Select(group => new KeyValuePair<Guid, decimal>(
                    group.Key,
                    group.Sum(x => x.Kind == ContributionKind.Deposit ? x.Amount : -x.Amount)))
                .ToListAsync();

            return balances.ToDictionary(item => item.Key, item => item.Value);
        }

        #endregion

        #region Methods :: GetContributionsAsync(), GetContributionByIdAsync(), CreateContributionAsync(), DeleteContributionAsync()

        /// <inheritdoc />
        public async Task<IReadOnlyList<SavingsGoalContribution>> GetContributionsAsync(Guid savingsGoalId, int limit)
        {
            return await this.context.SavingsGoalContributions
                .AsNoTracking()
                .Include(x => x.User)
                .Where(x => x.SavingsGoalId == savingsGoalId)
                .OrderByDescending(x => x.OccurredOn)
                .ThenByDescending(x => x.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<SavingsGoalContribution?> GetContributionByIdAsync(Guid userId, Guid contributionId)
        {
            return await this.context.SavingsGoalContributions
                .FirstOrDefaultAsync(x => x.Id == contributionId && x.UserId == userId);
        }

        /// <inheritdoc />
        public async Task<bool> CreateContributionAsync(SavingsGoalContribution contribution)
        {
            ArgumentNullException.ThrowIfNull(contribution);

            await this.context.SavingsGoalContributions.AddAsync(contribution);

            return await this.context.SaveChangesAsync() > 0;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteContributionAsync(SavingsGoalContribution contribution)
        {
            ArgumentNullException.ThrowIfNull(contribution);

            this.context.SavingsGoalContributions.Remove(contribution);

            return await this.context.SaveChangesAsync() > 0;
        }

        #endregion
    }
}
