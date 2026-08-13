using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;
using ControleDeGasto.API.Domain.Interfaces;
using ControleDeGasto.API.Domain.ReadModels;
using ControleDeGasto.API.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

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
            IQueryable<SavingsGoal> query = this.context.SavingsGoals
                .AsNoTracking()
                .Where(x => x.UserId == userId);

            if (!includeArchived)
                query = query.Where(x => x.Status != SavingsGoalStatus.Archived);

            // A reserva de emergência vem primeiro: é o cofrinho que a interface destaca.
            return await query
                .OrderByDescending(x => x.IsEmergencyReserve)
                .ThenBy(x => x.Name)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<SavingsGoal?> GetByIdAsync(Guid userId, Guid savingsGoalId)
        {
            return await this.context.SavingsGoals
                .FirstOrDefaultAsync(x => x.Id == savingsGoalId && x.UserId == userId);
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

            await this.context.SavingsGoals.AddAsync(savingsGoal);

            return await this.context.SaveChangesAsync() > 0;
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

        #region Methods :: GetBalanceAsync(), GetBalancesAsync()

        /// <inheritdoc />
        public async Task<decimal> GetBalanceAsync(Guid userId, Guid savingsGoalId)
        {
            // A soma sai do banco em uma linha. O sinal vem do tipo do movimento, por isso o
            // CASE no lugar de duas consultas.
            return await this.context.SavingsGoalContributions
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.SavingsGoalId == savingsGoalId)
                .SumAsync(x => x.Kind == ContributionKind.Deposit ? x.Amount : -x.Amount);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<GoalBalance>> GetBalancesAsync(Guid userId)
        {
            // Uma consulta agrupada para todos os cofrinhos, em vez de uma por cofrinho:
            // evita o N+1 na listagem e no painel.
            return await this.context.SavingsGoalContributions
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .GroupBy(x => x.SavingsGoalId)
                .Select(group => new GoalBalance
                {
                    SavingsGoalId = group.Key,
                    Balance = group.Sum(x => x.Kind == ContributionKind.Deposit ? x.Amount : -x.Amount)
                })
                .ToListAsync();
        }

        #endregion

        #region Methods :: GetContributionsAsync(), GetContributionByIdAsync(), CreateContributionAsync(), DeleteContributionAsync()

        /// <inheritdoc />
        public async Task<IReadOnlyList<SavingsGoalContribution>> GetContributionsAsync(Guid userId, Guid savingsGoalId, int limit)
        {
            return await this.context.SavingsGoalContributions
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.SavingsGoalId == savingsGoalId)
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
