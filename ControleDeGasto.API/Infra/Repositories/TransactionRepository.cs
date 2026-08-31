using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;
using ControleDeGasto.API.Domain.Interfaces;
using ControleDeGasto.API.Domain.Queries;
using ControleDeGasto.API.Domain.ReadModels;
using ControleDeGasto.API.Infra.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ControleDeGasto.API.Infra.Repositories
{
    public class TransactionRepository(
        AppDbContext context) : ITransactionRepository
    {
        #region Fields

        private readonly AppDbContext context = context;

        #endregion

        #region Helpers :: BuildFilteredQuery(), ApplySort()

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

            if (query.WalletId.HasValue)
                source = source.Where(x => x.WalletId == query.WalletId.Value);

            if (query.Status.HasValue)
                source = source.Where(x => x.Status == query.Status.Value);

            if (query.PaymentMethod.HasValue)
                source = source.Where(x => x.PaymentMethod == query.PaymentMethod.Value);

            if (query.MinAmount.HasValue)
                source = source.Where(x => x.Amount >= query.MinAmount.Value);

            if (query.MaxAmount.HasValue)
                source = source.Where(x => x.Amount <= query.MaxAmount.Value);

            if (query.DueFrom.HasValue)
                source = source.Where(x => x.DueDate != null && x.DueDate >= query.DueFrom.Value);

            if (query.DueTo.HasValue)
                source = source.Where(x => x.DueDate != null && x.DueDate <= query.DueTo.Value);

            // Atraso é derivado, não gravado: previsto com vencimento anterior à referência.
            if (query.OnlyOverdue)
            {
                source = source.Where(x => x.Status == TransactionStatus.Pending
                    && x.DueDate != null
                    && x.DueDate < query.Reference);
            }

            if (query.OnlyShared)
                source = source.Where(x => x.Shares!.Any());

            if (query.OnlyInstallments)
                source = source.Where(x => x.InstallmentPlanId != null);

            if (query.InstallmentPlanId.HasValue)
                source = source.Where(x => x.InstallmentPlanId == query.InstallmentPlanId.Value);

            // "Ao menos uma das etiquetas" em vez de "todas": é o comportamento que o usuário
            // espera de um filtro de marcadores, e um E entre etiquetas quase sempre devolveria
            // lista vazia.
            if (query.TagIds is { Count: > 0 })
            {
                IReadOnlyList<Guid> tagIds = query.TagIds;

                source = source.Where(x => x.Tags!.Any(tag => tagIds.Contains(tag.TagId)));
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                string search = query.Search.Trim();

                // ILIKE do Postgres: comparação sem diferenciar maiúsculas sem desabilitar índice
                // por função aplicada à coluna.
                source = source.Where(x => EF.Functions.ILike(x.Description, $"%{search}%"));
            }

            return source;
        }

        /// <summary>
        /// Aplica a ordenação pedida, sempre com um desempate estável.
        /// </summary>
        /// <remarks>
        /// O desempate por <c>CreatedAt</c> existe porque a paginação no servidor precisa de
        /// ordem total: com empate, a mesma linha pode aparecer em duas páginas e outra em
        /// nenhuma.
        /// </remarks>
        /// <param name="source">Consulta filtrada.</param>
        /// <param name="query">Filtros, de onde saem o campo e o sentido.</param>
        /// <returns>Consulta ordenada.</returns>
        private static IQueryable<Transaction> ApplySort(IQueryable<Transaction> source, TransactionQuery query)
        {
            IOrderedQueryable<Transaction> ordered = (query.SortBy, query.SortDescending) switch
            {
                (TransactionSortField.DueDate, true) => source.OrderByDescending(x => x.DueDate ?? x.OccurredOn),
                (TransactionSortField.DueDate, false) => source.OrderBy(x => x.DueDate ?? x.OccurredOn),
                (TransactionSortField.Amount, true) => source.OrderByDescending(x => x.Amount),
                (TransactionSortField.Amount, false) => source.OrderBy(x => x.Amount),
                (TransactionSortField.Description, true) => source.OrderByDescending(x => x.Description),
                (TransactionSortField.Description, false) => source.OrderBy(x => x.Description),
                (TransactionSortField.CreatedAt, true) => source.OrderByDescending(x => x.CreatedAt),
                (TransactionSortField.CreatedAt, false) => source.OrderBy(x => x.CreatedAt),
                (_, false) => source.OrderBy(x => x.OccurredOn),
                _ => source.OrderByDescending(x => x.OccurredOn)
            };

            return ordered.ThenByDescending(x => x.CreatedAt);
        }

        #endregion

        #region Methods :: GetPagedAsync(), GetByIdAsync(), GetRecentAsync()

        /// <inheritdoc />
        public async Task<PagedResult<Transaction>> GetPagedAsync(TransactionQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            IQueryable<Transaction> source = this.BuildFilteredQuery(query);

            int totalCount = await source.CountAsync();

            List<Transaction> items = await ApplySort(source, query)
                .Include(x => x.Category)
                .Include(x => x.Wallet)
                .Include(x => x.Tags!)
                    .ThenInclude(x => x.Tag)
                .Include(x => x.Shares!)
                    .ThenInclude(x => x.FriendUser)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .AsSplitQuery()
                .ToListAsync();

            return new PagedResult<Transaction>(items, totalCount);
        }

        /// <inheritdoc />
        public async Task<Transaction?> GetByIdAsync(Guid userId, Guid transactionId)
        {
            return await this.context.Transactions
                .Include(x => x.Category)
                .Include(x => x.Wallet)
                .Include(x => x.Tags!)
                    .ThenInclude(x => x.Tag)
                .Include(x => x.Shares!)
                    .ThenInclude(x => x.FriendUser)
                .AsSplitQuery()
                .FirstOrDefaultAsync(x => x.Id == transactionId && x.UserId == userId);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Transaction>> GetRecentAsync(Guid userId, int count)
        {
            return await this.context.Transactions
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Wallet)
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

        #region Methods :: GetTotalsByTypeAsync(), GetTotalsByCategoryAsync(), GetMonthlyTotalsAsync(), GetPendingTotalsAsync()

        /// <inheritdoc />
        public async Task<IReadOnlyList<TypeTotal>> GetTotalsByTypeAsync(Guid userId, DateTime from, DateTime to)
        {
            return await this.context.Transactions
                .AsNoTracking()
                .Where(x => x.UserId == userId
                    && x.Status == TransactionStatus.Settled
                    && x.OccurredOn >= from
                    && x.OccurredOn <= to)
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
                    && x.Status == TransactionStatus.Settled
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
                .Where(x => x.UserId == userId
                    && x.Status == TransactionStatus.Settled
                    && x.OccurredOn >= from
                    && x.OccurredOn <= to)
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

        /// <inheritdoc />
        public async Task<IReadOnlyList<PendingTypeTotal>> GetPendingTotalsAsync(Guid userId, DateTime from, DateTime to, DateTime reference)
        {
            // O período é avaliado pelo vencimento quando existe, e pela competência quando não:
            // uma conta a pagar sem vencimento informado ainda precisa aparecer no mês em que
            // foi lançada.
            return await this.context.Transactions
                .AsNoTracking()
                .Where(x => x.UserId == userId
                    && x.Status == TransactionStatus.Pending
                    && (x.DueDate ?? x.OccurredOn) >= from
                    && (x.DueDate ?? x.OccurredOn) <= to)
                .GroupBy(x => x.Category!.Type)
                .Select(group => new PendingTypeTotal
                {
                    Type = group.Key,
                    Total = group.Sum(x => x.Amount),
                    OverdueTotal = group.Sum(x => x.DueDate != null && x.DueDate < reference ? x.Amount : 0),
                    TransactionCount = group.Count()
                })
                .ToListAsync();
        }

        #endregion

        #region Methods :: GetUpcomingAsync(), GetOverdueAsync()

        /// <inheritdoc />
        public async Task<IReadOnlyList<Transaction>> GetUpcomingAsync(Guid userId, DateTime from, DateTime to, int limit)
        {
            return await this.context.Transactions
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Wallet)
                .Where(x => x.UserId == userId
                    && x.Status == TransactionStatus.Pending
                    && x.DueDate != null
                    && x.DueDate >= from
                    && x.DueDate <= to)
                .OrderBy(x => x.DueDate)
                .ThenBy(x => x.Amount)
                .Take(limit)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Transaction>> GetOverdueAsync(Guid userId, DateTime reference, int limit)
        {
            return await this.context.Transactions
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Wallet)
                .Where(x => x.UserId == userId
                    && x.Status == TransactionStatus.Pending
                    && x.DueDate != null
                    && x.DueDate < reference)
                .OrderBy(x => x.DueDate)
                .Take(limit)
                .ToListAsync();
        }

        #endregion

        #region Methods :: ReplaceTagsAsync(), ReplaceSharesAsync(), GetShareByIdAsync(), UpdateShareAsync()

        /// <inheritdoc />
        public async Task<int> ReplaceTagsAsync(Guid transactionId, IReadOnlyList<Guid> tagIds)
        {
            ArgumentNullException.ThrowIfNull(tagIds);

            List<TransactionTag> current = await this.context.TransactionTags
                .Where(x => x.TransactionId == transactionId)
                .ToListAsync();

            // Só o que mudou é gravado: reescrever os vínculos idênticos geraria DELETE e INSERT
            // desnecessários a cada edição de descrição.
            List<TransactionTag> toRemove = current
                .Where(item => !tagIds.Contains(item.TagId))
                .ToList();

            List<Guid> toAdd = tagIds
                .Distinct()
                .Where(tagId => current.TrueForAll(item => item.TagId != tagId))
                .ToList();

            if (toRemove.Count > 0)
                this.context.TransactionTags.RemoveRange(toRemove);

            if (toAdd.Count > 0)
            {
                await this.context.TransactionTags.AddRangeAsync(
                    toAdd.Select(tagId => new TransactionTag
                    {
                        TransactionId = transactionId,
                        TagId = tagId
                    }));
            }

            if (toRemove.Count == 0 && toAdd.Count == 0)
                return 0;

            return await this.context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<int> ReplaceSharesAsync(Guid transactionId, IReadOnlyList<TransactionShare> shares)
        {
            ArgumentNullException.ThrowIfNull(shares);

            List<TransactionShare> current = await this.context.TransactionShares
                .Where(x => x.TransactionId == transactionId)
                .ToListAsync();

            foreach (TransactionShare existing in current)
            {
                TransactionShare? incoming = shares.FirstOrDefault(x => x.FriendUserId == existing.FriendUserId);

                if (incoming is null)
                {
                    this.context.TransactionShares.Remove(existing);
                    continue;
                }

                // O acerto já registrado é preservado: mudar o valor da divisão não pode fazer
                // um amigo que já pagou voltar a constar como devedor.
                existing.Amount = incoming.Amount;
            }

            List<TransactionShare> toAdd = shares
                .Where(incoming => current.TrueForAll(existing => existing.FriendUserId != incoming.FriendUserId))
                .ToList();

            if (toAdd.Count > 0)
                await this.context.TransactionShares.AddRangeAsync(toAdd);

            return await this.context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<TransactionShare?> GetShareByIdAsync(Guid userId, Guid shareId)
        {
            return await this.context.TransactionShares
                .Include(x => x.Transaction)
                    .ThenInclude(x => x!.Category)
                .Include(x => x.FriendUser)
                .FirstOrDefaultAsync(x => x.Id == shareId
                    && (x.FriendUserId == userId || x.Transaction!.UserId == userId));
        }

        /// <inheritdoc />
        public async Task<bool> UpdateShareAsync(TransactionShare share)
        {
            ArgumentNullException.ThrowIfNull(share);

            this.context.TransactionShares.Update(share);

            return await this.context.SaveChangesAsync() > 0;
        }

        #endregion

        #region Methods :: GetSharedWithUserAsync(), GetFriendBalancesAsync()

        /// <inheritdoc />
        public async Task<PagedResult<TransactionShare>> GetSharedWithUserAsync(Guid userId, bool onlyOpen, int page, int pageSize)
        {
            IQueryable<TransactionShare> source = this.context.TransactionShares
                .AsNoTracking()
                .Where(x => x.FriendUserId == userId);

            if (onlyOpen)
                source = source.Where(x => x.SettledAt == null);

            int totalCount = await source.CountAsync();

            List<TransactionShare> items = await source
                .Include(x => x.Transaction)
                    .ThenInclude(x => x!.Category)
                .Include(x => x.Transaction)
                    .ThenInclude(x => x!.User)
                .OrderByDescending(x => x.Transaction!.OccurredOn)
                .ThenByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsSplitQuery()
                .ToListAsync();

            return new PagedResult<TransactionShare>(items, totalCount);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<FriendBalance>> GetFriendBalancesAsync(Guid userId)
        {
            // Os dois sentidos entram na mesma projeção e são agrupados pelo amigo: assim um
            // amigo que deve e a quem também se deve aparece uma única vez, com o líquido.
            var receivable = this.context.TransactionShares
                .AsNoTracking()
                .Where(x => x.Transaction!.UserId == userId && x.SettledAt == null)
                .Select(x => new { FriendUserId = x.FriendUserId, Receivable = x.Amount, Payable = 0m });

            var payable = this.context.TransactionShares
                .AsNoTracking()
                .Where(x => x.FriendUserId == userId && x.SettledAt == null)
                .Select(x => new { FriendUserId = x.Transaction!.UserId, Receivable = 0m, Payable = x.Amount });

            return await receivable
                .Concat(payable)
                .GroupBy(x => x.FriendUserId)
                .Select(group => new FriendBalance
                {
                    FriendUserId = group.Key,
                    Receivable = group.Sum(x => x.Receivable),
                    Payable = group.Sum(x => x.Payable)
                })
                .ToListAsync();
        }

        #endregion

        #region Methods :: GetInstallmentPlansAsync(), GetInstallmentPlanByIdAsync(), CreateInstallmentPlanAsync(), DeleteInstallmentPlanAsync()

        /// <inheritdoc />
        public async Task<IReadOnlyList<InstallmentPlan>> GetInstallmentPlansAsync(Guid userId, bool onlyOpen)
        {
            IQueryable<InstallmentPlan> query = this.context.InstallmentPlans
                .AsNoTracking()
                .Where(x => x.UserId == userId);

            if (onlyOpen)
                query = query.Where(x => x.Installments!.Any(item => item.Status == TransactionStatus.Pending));

            return await query
                .Include(x => x.Category)
                .Include(x => x.Wallet)
                .Include(x => x.Installments)
                .OrderByDescending(x => x.CreatedAt)
                .AsSplitQuery()
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<InstallmentPlan?> GetInstallmentPlanByIdAsync(Guid userId, Guid installmentPlanId)
        {
            return await this.context.InstallmentPlans
                .Include(x => x.Category)
                .Include(x => x.Wallet)
                .Include(x => x.Installments)
                .AsSplitQuery()
                .FirstOrDefaultAsync(x => x.Id == installmentPlanId && x.UserId == userId);
        }

        /// <inheritdoc />
        public async Task<bool> CreateInstallmentPlanAsync(InstallmentPlan plan, IReadOnlyList<Transaction> installments)
        {
            ArgumentNullException.ThrowIfNull(plan);
            ArgumentNullException.ThrowIfNull(installments);

            // Transação explícita: o plano e as parcelas formam uma unidade, e um plano gravado
            // sem as parcelas seria uma dívida que não fecha com o total da compra.
            await using IDbContextTransaction dbTransaction = await this.context.Database.BeginTransactionAsync();

            try
            {
                await this.context.InstallmentPlans.AddAsync(plan);
                await this.context.Transactions.AddRangeAsync(installments);

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
        public async Task<bool> DeleteInstallmentPlanAsync(InstallmentPlan plan)
        {
            ArgumentNullException.ThrowIfNull(plan);

            this.context.InstallmentPlans.Remove(plan);

            return await this.context.SaveChangesAsync() > 0;
        }

        #endregion
    }
}
