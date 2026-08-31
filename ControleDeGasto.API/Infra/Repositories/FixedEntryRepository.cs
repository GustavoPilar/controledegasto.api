using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;
using ControleDeGasto.API.Domain.Interfaces;
using ControleDeGasto.API.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControleDeGasto.API.Infra.Repositories
{
    public class FixedEntryRepository(
        AppDbContext context) : IFixedEntryRepository
    {
        #region Fields

        private readonly AppDbContext context = context;

        #endregion

        #region Methods :: GetAllAsync(), GetActiveForMonthAsync(), GetByIdAsync(), ExistsByDescriptionAsync()

        /// <inheritdoc />
        public async Task<IReadOnlyList<FixedEntry>> GetAllAsync(Guid userId, bool includeInactive)
        {
            IQueryable<FixedEntry> query = this.context.FixedEntries
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Wallet)
                .Where(x => x.UserId == userId);

            if (!includeInactive)
                query = query.Where(x => x.Active);

            return await query
                .OrderBy(x => x.Kind)
                .ThenBy(x => x.DayOfMonth)
                .ThenBy(x => x.Description)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<FixedEntry>> GetActiveForMonthAsync(Guid userId, DateTime monthStart, DateTime monthEnd)
        {
            return await this.context.FixedEntries
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Wallet)
                .Where(x => x.UserId == userId
                    && x.Active
                    && x.StartsOn <= monthEnd
                    && (x.EndsOn == null || x.EndsOn >= monthStart))
                .OrderBy(x => x.Kind)
                .ThenBy(x => x.DayOfMonth)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<FixedEntry?> GetByIdAsync(Guid userId, Guid fixedEntryId)
        {
            return await this.context.FixedEntries
                .Include(x => x.Category)
                .Include(x => x.Wallet)
                .FirstOrDefaultAsync(x => x.Id == fixedEntryId && x.UserId == userId);
        }

        /// <inheritdoc />
        public async Task<bool> ExistsByDescriptionAsync(Guid userId, FixedEntryKind kind, string description, Guid? excludeFixedEntryId)
        {
            IQueryable<FixedEntry> query = this.context.FixedEntries
                .AsNoTracking()
                .Where(x => x.UserId == userId
                    && x.Kind == kind
                    && x.Description.ToLower() == description.ToLower());

            if (excludeFixedEntryId.HasValue)
                query = query.Where(x => x.Id != excludeFixedEntryId.Value);

            return await query.AnyAsync();
        }

        #endregion

        #region Methods :: CreateAsync(), UpdateAsync(), DeleteAsync()

        /// <inheritdoc />
        public async Task<bool> CreateAsync(FixedEntry fixedEntry)
        {
            ArgumentNullException.ThrowIfNull(fixedEntry);

            await this.context.FixedEntries.AddAsync(fixedEntry);

            return await this.context.SaveChangesAsync() > 0;
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(FixedEntry fixedEntry)
        {
            ArgumentNullException.ThrowIfNull(fixedEntry);

            this.context.FixedEntries.Update(fixedEntry);

            return await this.context.SaveChangesAsync() > 0;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(FixedEntry fixedEntry)
        {
            ArgumentNullException.ThrowIfNull(fixedEntry);

            this.context.FixedEntries.Remove(fixedEntry);

            return await this.context.SaveChangesAsync() > 0;
        }

        #endregion
    }
}
