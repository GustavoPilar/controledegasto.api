using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;
using ControleDeGasto.API.Domain.Interfaces;
using ControleDeGasto.API.Domain.ReadModels;
using ControleDeGasto.API.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControleDeGasto.API.Infra.Repositories
{
    public class WalletRepository(
        AppDbContext context) : IWalletRepository
    {
        #region Fields

        private readonly AppDbContext context = context;

        #endregion

        #region Methods :: GetAllAsync(), GetByIdAsync(), GetDefaultAsync(), ExistsByNameAsync()

        /// <inheritdoc />
        public async Task<IReadOnlyList<Wallet>> GetAllAsync(Guid userId, bool includeInactive)
        {
            IQueryable<Wallet> query = this.context.Wallets
                .AsNoTracking()
                .Where(x => x.UserId == userId);

            if (!includeInactive)
                query = query.Where(x => x.Active);

            // A padrão vem primeiro: é a carteira que a interface pré-seleciona nos formulários.
            return await query
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.Kind)
                .ThenBy(x => x.Name)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<Wallet?> GetByIdAsync(Guid userId, Guid walletId)
        {
            return await this.context.Wallets
                .FirstOrDefaultAsync(x => x.Id == walletId && x.UserId == userId);
        }

        /// <inheritdoc />
        public async Task<Wallet?> GetDefaultAsync(Guid userId)
        {
            return await this.context.Wallets
                .FirstOrDefaultAsync(x => x.UserId == userId && x.IsDefault && x.Active);
        }

        /// <inheritdoc />
        public async Task<bool> ExistsByNameAsync(Guid userId, string name, Guid? excludeWalletId)
        {
            IQueryable<Wallet> query = this.context.Wallets
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.Active && x.Name.ToLower() == name.ToLower());

            if (excludeWalletId.HasValue)
                query = query.Where(x => x.Id != excludeWalletId.Value);

            return await query.AnyAsync();
        }

        #endregion

        #region Methods :: CreateAsync(), UpdateAsync(), ClearDefaultAsync(), HasMovementAsync()

        /// <inheritdoc />
        public async Task<bool> CreateAsync(Wallet wallet)
        {
            ArgumentNullException.ThrowIfNull(wallet);

            await this.context.Wallets.AddAsync(wallet);

            return await this.context.SaveChangesAsync() > 0;
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(Wallet wallet)
        {
            ArgumentNullException.ThrowIfNull(wallet);

            this.context.Wallets.Update(wallet);

            return await this.context.SaveChangesAsync() > 0;
        }

        /// <inheritdoc />
        public async Task<int> ClearDefaultAsync(Guid userId, Guid? exceptWalletId)
        {
            // ExecuteUpdate em vez de carregar e alterar: a operação é uma atualização em massa
            // condicional, e materializar as carteiras só para virar um booleano é desperdício.
            IQueryable<Wallet> query = this.context.Wallets
                .Where(x => x.UserId == userId && x.IsDefault);

            if (exceptWalletId.HasValue)
                query = query.Where(x => x.Id != exceptWalletId.Value);

            return await query.ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.IsDefault, false)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow));
        }

        /// <inheritdoc />
        public async Task<bool> HasMovementAsync(Guid userId, Guid walletId)
        {
            bool hasTransaction = await this.context.Transactions
                .AsNoTracking()
                .AnyAsync(x => x.UserId == userId && x.WalletId == walletId);

            if (hasTransaction)
                return true;

            return await this.context.WalletTransfers
                .AsNoTracking()
                .AnyAsync(x => x.UserId == userId && (x.FromWalletId == walletId || x.ToWalletId == walletId));
        }

        #endregion

        #region Methods :: GetBalancesAsync(), GetTransferTotalsAsync()

        /// <inheritdoc />
        public async Task<IReadOnlyList<WalletBalance>> GetBalancesAsync(Guid userId)
        {
            // Uma consulta agrupada para todas as carteiras. O sinal vem do tipo da categoria,
            // por isso o CASE no lugar de várias consultas.
            return await this.context.Transactions
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.WalletId != null)
                .GroupBy(x => x.WalletId!.Value)
                .Select(group => new WalletBalance
                {
                    WalletId = group.Key,
                    MovementBalance = group.Sum(x => x.Status == TransactionStatus.Settled
                        ? (x.Category!.Type == TransactionType.Income ? x.Amount : -x.Amount)
                        : 0),
                    PendingIncome = group.Sum(x => x.Status == TransactionStatus.Pending && x.Category!.Type == TransactionType.Income
                        ? x.Amount
                        : 0),
                    PendingExpense = group.Sum(x => x.Status == TransactionStatus.Pending && x.Category!.Type == TransactionType.Expense
                        ? x.Amount
                        : 0)
                })
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<WalletTransferTotal>> GetTransferTotalsAsync(Guid userId)
        {
            // As duas pontas de cada transferência viram linhas com sinal e são agrupadas em
            // seguida: assim uma carteira que só recebeu e outra que só enviou aparecem na
            // mesma consulta, sem UNION escrito à mão.
            var outgoing = this.context.WalletTransfers
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => new { WalletId = x.FromWalletId, In = 0m, Out = x.Amount });

            var incoming = this.context.WalletTransfers
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => new { WalletId = x.ToWalletId, In = x.Amount, Out = 0m });

            return await outgoing
                .Concat(incoming)
                .GroupBy(x => x.WalletId)
                .Select(group => new WalletTransferTotal
                {
                    WalletId = group.Key,
                    TransferredIn = group.Sum(x => x.In),
                    TransferredOut = group.Sum(x => x.Out)
                })
                .ToListAsync();
        }

        #endregion

        #region Methods :: GetTransfersAsync(), GetTransferByIdAsync(), CreateTransferAsync(), DeleteTransferAsync()

        /// <inheritdoc />
        public async Task<IReadOnlyList<WalletTransfer>> GetTransfersAsync(Guid userId, Guid? walletId, int limit)
        {
            IQueryable<WalletTransfer> query = this.context.WalletTransfers
                .AsNoTracking()
                .Include(x => x.FromWallet)
                .Include(x => x.ToWallet)
                .Where(x => x.UserId == userId);

            if (walletId.HasValue)
                query = query.Where(x => x.FromWalletId == walletId.Value || x.ToWalletId == walletId.Value);

            return await query
                .OrderByDescending(x => x.OccurredOn)
                .ThenByDescending(x => x.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<WalletTransfer?> GetTransferByIdAsync(Guid userId, Guid transferId)
        {
            return await this.context.WalletTransfers
                .Include(x => x.FromWallet)
                .Include(x => x.ToWallet)
                .FirstOrDefaultAsync(x => x.Id == transferId && x.UserId == userId);
        }

        /// <inheritdoc />
        public async Task<bool> CreateTransferAsync(WalletTransfer transfer)
        {
            ArgumentNullException.ThrowIfNull(transfer);

            await this.context.WalletTransfers.AddAsync(transfer);

            return await this.context.SaveChangesAsync() > 0;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteTransferAsync(WalletTransfer transfer)
        {
            ArgumentNullException.ThrowIfNull(transfer);

            this.context.WalletTransfers.Remove(transfer);

            return await this.context.SaveChangesAsync() > 0;
        }

        #endregion
    }
}
