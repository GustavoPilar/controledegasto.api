using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;
using ControleDeGasto.API.Domain.Interfaces;
using ControleDeGasto.API.Domain.ReadModels;
using ControleDeGasto.API.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControleDeGasto.API.Infra.Repositories
{
    public class TagRepository(
        AppDbContext context) : ITagRepository
    {
        #region Fields

        private readonly AppDbContext context = context;

        #endregion

        #region Methods :: GetAllAsync(), GetByIdAsync(), FilterOwnedAsync(), ExistsByNameAsync()

        /// <inheritdoc />
        public async Task<IReadOnlyList<Tag>> GetAllAsync(Guid userId)
        {
            return await this.context.Tags
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<Tag?> GetByIdAsync(Guid userId, Guid tagId)
        {
            return await this.context.Tags
                .FirstOrDefaultAsync(x => x.Id == tagId && x.UserId == userId);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Guid>> FilterOwnedAsync(Guid userId, IReadOnlyList<Guid> tagIds)
        {
            ArgumentNullException.ThrowIfNull(tagIds);

            if (tagIds.Count == 0)
                return [];

            return await this.context.Tags
                .AsNoTracking()
                .Where(x => x.UserId == userId && tagIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<bool> ExistsByNameAsync(Guid userId, string name, Guid? excludeTagId)
        {
            IQueryable<Tag> query = this.context.Tags
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.Name.ToLower() == name.ToLower());

            if (excludeTagId.HasValue)
                query = query.Where(x => x.Id != excludeTagId.Value);

            return await query.AnyAsync();
        }

        #endregion

        #region Methods :: CreateAsync(), UpdateAsync(), DeleteAsync(), GetUsageCountAsync()

        /// <inheritdoc />
        public async Task<bool> CreateAsync(Tag tag)
        {
            ArgumentNullException.ThrowIfNull(tag);

            await this.context.Tags.AddAsync(tag);

            return await this.context.SaveChangesAsync() > 0;
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(Tag tag)
        {
            ArgumentNullException.ThrowIfNull(tag);

            this.context.Tags.Update(tag);

            return await this.context.SaveChangesAsync() > 0;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(Tag tag)
        {
            ArgumentNullException.ThrowIfNull(tag);

            this.context.Tags.Remove(tag);

            return await this.context.SaveChangesAsync() > 0;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyDictionary<Guid, int>> GetUsageCountAsync(Guid userId)
        {
            // Uma consulta agrupada para todas as etiquetas do usuário, em vez de uma contagem
            // por etiqueta na montagem da lista.
            List<KeyValuePair<Guid, int>> counts = await this.context.TransactionTags
                .AsNoTracking()
                .Where(x => x.Tag!.UserId == userId)
                .GroupBy(x => x.TagId)
                .Select(group => new KeyValuePair<Guid, int>(group.Key, group.Count()))
                .ToListAsync();

            return counts.ToDictionary(item => item.Key, item => item.Value);
        }

        #endregion

        #region Methods :: GetTotalsAsync()

        /// <inheritdoc />
        public async Task<IReadOnlyList<TagTotal>> GetTotalsAsync(Guid userId, DateTime from, DateTime to)
        {
            return await this.context.TransactionTags
                .AsNoTracking()
                .Where(x => x.Tag!.UserId == userId
                    && x.Transaction!.OccurredOn >= from
                    && x.Transaction.OccurredOn <= to)
                .GroupBy(x => new { x.TagId, x.Tag!.Name, x.Tag.Color })
                .Select(group => new TagTotal
                {
                    TagId = group.Key.TagId,
                    TagName = group.Key.Name,
                    Color = group.Key.Color,
                    IncomeTotal = group.Sum(x => x.Transaction!.Category!.Type == TransactionType.Income
                        ? x.Transaction.Amount
                        : 0),
                    ExpenseTotal = group.Sum(x => x.Transaction!.Category!.Type == TransactionType.Expense
                        ? x.Transaction.Amount
                        : 0),
                    TransactionCount = group.Count()
                })
                .OrderByDescending(x => x.ExpenseTotal + x.IncomeTotal)
                .ToListAsync();
        }

        #endregion
    }
}
