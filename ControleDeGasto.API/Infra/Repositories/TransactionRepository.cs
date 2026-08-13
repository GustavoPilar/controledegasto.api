using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;
using ControleDeGasto.API.Domain.Interfaces;
using ControleDeGasto.API.Domain.Queries;
using ControleDeGasto.API.Domain.ReadModels;
using ControleDeGasto.API.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControleDeGasto.API.Infra.Repositories
{
    public class TransactionRepository(
        AppDbContext context) : ITransactionRepository
    {
        #region Fields

        private readonly AppDbContext context = context;

        #endregion

        #region Helpers :: BuildFilteredQuery()

        /// <summary>
        /// Monta a consulta base já restrita ao dono e aos filtros informados.
        /// </summary>
        /// <param name="query">Filtros da consulta.</param>
        /// <returns>Consulta sem rastreamento, pronta para paginar ou contar.</returns>
        private IQueryable<Transaction> BuildFilteredQuery(TransactionQuery query)
        {
            IQueryable<Transaction> source = this.context.Transactions
                .AsNoTracking()
                .Where(x => x.UserId == query.UserId);

            if (query.From.HasValue)
                source = source.Where(x => x.OccurredOn >= query.From.Value);

            if (query.To.HasValue)
                source = source.Where(x => x.OccurredOn <= query.To.Value);

            if (query.CategoryId.HasValue)
                source = source.Where(x => x.CategoryId == query.CategoryId.Value);

            if (query.Type.HasValue)
                source = source.Where(x => x.Category!.Type == query.Type.Value);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                string search = query.Search.Trim();

                // ILIKE do Postgres: comparação sem diferenciar maiúsculas sem desabilitar índice
                // por função aplicada à coluna.
                source = source.Where(x => EF.Functions.ILike(x.Description, $"%{search}%"));
            }

            return source;
        }

        #endregion

        #region Methods :: GetPagedAsync(), GetByIdAsync(), GetRecentAsync()

        /// <inheritdoc />
        public async Task<PagedResult<Transaction>> GetPagedAsync(TransactionQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            IQueryable<Transaction> source = this.BuildFilteredQuery(query);

            int totalCount = await source.CountAsync();

            List<Transaction> items = await source
                .Include(x => x.Category)
                .OrderByDescending(x => x.OccurredOn)
                .ThenByDescending(x => x.CreatedAt)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<Transaction>(items, totalCount);
        }

        /// <inheritdoc />
        public async Task<Transaction?> GetByIdAsync(Guid userId, Guid transactionId)
        {
            return await this.context.Transactions
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == transactionId && x.UserId == userId);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Transaction>> GetRecentAsync(Guid userId, int count)
        {
            return await this.context.Transactions
                .AsNoTracking()
                .Include(x => x.Category)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.OccurredOn)
                .ThenByDescending(x => x.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        #endregion

        #region Methods :: CreateAsync(), UpdateAsync(), DeleteAsync()

        /// <inheritdoc />
        public async Task<bool> CreateAsync(Transaction transaction)
        {
            ArgumentNullException.ThrowIfNull(transaction);

            await this.context.Transactions.AddAsync(transaction);

            return await this.context.SaveChangesAsync() > 0;
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(Transaction transaction)
        {
            ArgumentNullException.ThrowIfNull(transaction);

            this.context.Transactions.Update(transaction);

            return await this.context.SaveChangesAsync() > 0;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(Transaction transaction)
        {
            ArgumentNullException.ThrowIfNull(transaction);

            this.context.Transactions.Remove(transaction);

            return await this.context.SaveChangesAsync() > 0;
        }

        #endregion

        #region Methods :: GetTotalsByTypeAsync(), GetTotalsByCategoryAsync(), GetMonthlyTotalsAsync()

        /// <inheritdoc />
        public async Task<IReadOnlyList<TypeTotal>> GetTotalsByTypeAsync(Guid userId, DateTime from, DateTime to)
        {
            return await this.context.Transactions
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.OccurredOn >= from && x.OccurredOn <= to)
                .GroupBy(x => x.Category!.Type)
                .Select(group => new TypeTotal
                {
                    Type = group.Key,
                    Total = group.Sum(x => x.Amount)
                })
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<CategoryTotal>> GetTotalsByCategoryAsync(Guid userId, TransactionType type, DateTime from, DateTime to, int? limit)
        {
            IQueryable<CategoryTotal> query = this.context.Transactions
                .AsNoTracking()
                .Where(x => x.UserId == userId
                    && x.OccurredOn >= from
                    && x.OccurredOn <= to
                    && x.Category!.Type == type)
                .GroupBy(x => new
                {
                    x.CategoryId,
                    x.Category!.Name,
                    x.Category.Color,
                    x.Category.Icon,
                    x.Category.Type
                })
                .Select(group => new CategoryTotal
                {
                    CategoryId = group.Key.CategoryId,
                    CategoryName = group.Key.Name,
                    Color = group.Key.Color,
                    Icon = group.Key.Icon,
                    Type = group.Key.Type,
                    Total = group.Sum(x => x.Amount),
                    TransactionCount = group.Count()
                })
                .OrderByDescending(x => x.Total);

            if (limit.HasValue)
                query = query.Take(limit.Value);

            return await query.ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<MonthlyTotal>> GetMonthlyTotalsAsync(Guid userId, DateTime from, DateTime to)
        {
            return await this.context.Transactions
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.OccurredOn >= from && x.OccurredOn <= to)
                .GroupBy(x => new
                {
                    x.OccurredOn.Year,
                    x.OccurredOn.Month,
                    x.Category!.Type
                })
                .Select(group => new MonthlyTotal
                {
                    Year = group.Key.Year,
                    Month = group.Key.Month,
                    Type = group.Key.Type,
                    Total = group.Sum(x => x.Amount)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();
        }

        #endregion
    }
}
