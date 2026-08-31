using ControleDeGasto.API.Application.DTOs;
using ControleDeGasto.API.Application.Interfaces;
using ControleDeGasto.API.Application.Utilities;
using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;
using ControleDeGasto.API.Domain.Exceptions;
using ControleDeGasto.API.Domain.Interfaces;
using ControleDeGasto.API.Domain.ReadModels;

namespace ControleDeGasto.API.Application.Services
{
    public class WalletService(
        IWalletRepository repository,
        IFixedEntryRepository fixedEntryRepository,
        ILogger<WalletService> logger) : IWalletService
    {
        #region Fields

        private readonly IWalletRepository repository = repository;
        private readonly IFixedEntryRepository fixedEntryRepository = fixedEntryRepository;
        private readonly ILogger<WalletService> logger = logger;

        #endregion

        #region Helpers :: BuildResponsesAsync(), NormalizeForKind()

        /// <summary>
        /// Monta as respostas de um conjunto de carteiras com saldo e crédito apurados.
        /// </summary>
        /// <remarks>
        /// Três consultas agrupadas — movimento, transferências e créditos fixos — em vez de uma
        /// por carteira: a listagem e o painel abrem com todas as carteiras de uma vez.
        /// </remarks>
        /// <param name="userId">Dono das carteiras.</param>
        /// <param name="wallets">Carteiras a converter.</param>
        /// <returns>Respostas na ordem recebida.</returns>
        private async Task<IReadOnlyList<WalletResponse>> BuildResponsesAsync(Guid userId, IReadOnlyList<Wallet> wallets)
        {
            if (wallets.Count == 0)
                return [];

            IReadOnlyList<WalletBalance> balances = await this.repository.GetBalancesAsync(userId);
            IReadOnlyList<WalletTransferTotal> transfers = await this.repository.GetTransferTotalsAsync(userId);
            IReadOnlyList<FixedEntry> fixedEntries = await this.fixedEntryRepository.GetAllAsync(userId, includeInactive: false);

            Dictionary<Guid, WalletBalance> balanceByWallet = balances.ToDictionary(item => item.WalletId);
            Dictionary<Guid, WalletTransferTotal> transferByWallet = transfers.ToDictionary(item => item.WalletId);

            Dictionary<Guid, FixedEntry> creditByWallet = fixedEntries
                .Where(item => item.Kind == FixedEntryKind.BenefitCredit && item.WalletId.HasValue)
                .GroupBy(item => item.WalletId!.Value)
                .ToDictionary(group => group.Key, group => group.First());

            DateTime now = DateTime.UtcNow;

            return wallets
                .Select(wallet => new WalletResponse(
                    wallet,
                    balanceByWallet.GetValueOrDefault(wallet.Id),
                    transferByWallet.GetValueOrDefault(wallet.Id),
                    creditByWallet.GetValueOrDefault(wallet.Id),
                    now))
                .ToList();
        }

        /// <summary>
        /// Ajusta os campos que só existem em determinadas naturezas.
        /// </summary>
        /// <remarks>
        /// Limpar em vez de rejeitar: o cliente pode ter preenchido limite e fechamento antes de
        /// trocar a natureza para dinheiro, e recusar a gravação por isso só produziria um erro
        /// que o usuário não entenderia.
        /// </remarks>
        /// <param name="wallet">Carteira a ajustar.</param>
        private static void NormalizeForKind(Wallet wallet)
        {
            if (wallet.Kind == WalletKind.CreditCard)
                return;

            wallet.CreditLimit = null;
            wallet.StatementClosingDay = null;
            wallet.PaymentDueDay = null;
        }

        #endregion

        #region Methods :: GetAllAsync(), GetByIdAsync()

        /// <inheritdoc />
        public async Task<IReadOnlyList<WalletResponse>> GetAllAsync(Guid userId, bool includeInactive)
        {
            IReadOnlyList<Wallet> wallets = await this.repository.GetAllAsync(userId, includeInactive);

            return await this.BuildResponsesAsync(userId, wallets);
        }

        /// <inheritdoc />
        public async Task<WalletResponse?> GetByIdAsync(Guid userId, Guid walletId)
        {
            Wallet? wallet = await this.repository.GetByIdAsync(userId, walletId);

            if (wallet is null)
                return null;

            IReadOnlyList<WalletResponse> responses = await this.BuildResponsesAsync(userId, [wallet]);

            return responses[0];
        }

        #endregion

        #region Methods :: CreateAsync(), UpdateAsync(), DeleteAsync()

        /// <inheritdoc />
        public async Task<WalletResponse> CreateAsync(Guid userId, WalletRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            string name = request.Name.Trim();

            bool exists = await this.repository.ExistsByNameAsync(userId, name, null);

            if (exists)
                throw new BusinessRuleViolationException("Já existe uma carteira com esse nome.");

            IReadOnlyList<Wallet> current = await this.repository.GetAllAsync(userId, includeInactive: false);

            Wallet wallet = new Wallet()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = name,
                Kind = request.Kind,
                Color = request.Color.ToUpperInvariant(),
                Icon = request.Icon.Trim(),
                InitialBalance = request.InitialBalance,
                CreditLimit = request.CreditLimit,
                StatementClosingDay = request.StatementClosingDay,
                PaymentDueDay = request.PaymentDueDay,

                // A primeira carteira nasce como padrão: sem isso, o usuário cadastraria uma
                // carteira e continuaria lançando sem nenhuma até descobrir a marcação.
                IsDefault = request.IsDefault || current.Count == 0,
                Active = true,
                CreatedAt = DateTime.UtcNow
            };

            NormalizeForKind(wallet);

            if (wallet.IsDefault)
                await this.repository.ClearDefaultAsync(userId, null);

            bool created = await this.repository.CreateAsync(wallet);

            if (!created)
                throw new BusinessRuleViolationException("Não foi possível criar a carteira.");

            this.logger.LogInformation("Carteira {WalletId} ({Kind}) criada para o usuário {UserId}.", wallet.Id, wallet.Kind, userId);

            IReadOnlyList<WalletResponse> responses = await this.BuildResponsesAsync(userId, [wallet]);

            return responses[0];
        }

        /// <inheritdoc />
        public async Task<WalletResponse?> UpdateAsync(Guid userId, Guid walletId, WalletRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            Wallet? wallet = await this.repository.GetByIdAsync(userId, walletId);

            if (wallet is null)
                return null;

            string name = request.Name.Trim();

            bool exists = await this.repository.ExistsByNameAsync(userId, name, walletId);

            if (exists)
                throw new BusinessRuleViolationException("Já existe uma carteira com esse nome.");

            bool becomingDefault = request.IsDefault && !wallet.IsDefault;

            wallet.Name = name;
            wallet.Kind = request.Kind;
            wallet.Color = request.Color.ToUpperInvariant();
            wallet.Icon = request.Icon.Trim();
            wallet.InitialBalance = request.InitialBalance;
            wallet.CreditLimit = request.CreditLimit;
            wallet.StatementClosingDay = request.StatementClosingDay;
            wallet.PaymentDueDay = request.PaymentDueDay;
            wallet.UpdatedAt = DateTime.UtcNow;

            NormalizeForKind(wallet);

            // Desmarcar a anterior antes de marcar a nova: o índice único filtrado recusaria as
            // duas ao mesmo tempo.
            if (becomingDefault)
            {
                await this.repository.ClearDefaultAsync(userId, walletId);
                wallet.IsDefault = true;
            }
            else if (!request.IsDefault && wallet.IsDefault)
            {
                wallet.IsDefault = false;
            }

            bool updated = await this.repository.UpdateAsync(wallet);

            if (!updated)
                throw new BusinessRuleViolationException("Não foi possível atualizar a carteira.");

            this.logger.LogInformation("Carteira {WalletId} atualizada pelo usuário {UserId}.", walletId, userId);

            IReadOnlyList<WalletResponse> responses = await this.BuildResponsesAsync(userId, [wallet]);

            return responses[0];
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(Guid userId, Guid walletId)
        {
            Wallet? wallet = await this.repository.GetByIdAsync(userId, walletId);

            if (wallet is null)
                return false;

            // A exclusão é sempre lógica: apagar a carteira levaria embora a origem dos
            // lançamentos históricos, e o saldo de meses fechados deixaria de fechar.
            wallet.Active = false;
            wallet.IsDefault = false;
            wallet.UpdatedAt = DateTime.UtcNow;

            bool updated = await this.repository.UpdateAsync(wallet);

            if (updated)
                this.logger.LogInformation("Carteira {WalletId} excluída pelo usuário {UserId}.", walletId, userId);

            return updated;
        }

        #endregion

        #region Methods :: EnsureWalletAsync(), ResolveWalletAsync()

        /// <inheritdoc />
        public async Task<Wallet> EnsureWalletAsync(Guid userId, Guid walletId)
        {
            Wallet? wallet = await this.repository.GetByIdAsync(userId, walletId);

            if (wallet is null)
                throw new BusinessRuleViolationException("Carteira não encontrada.");

            if (!wallet.Active)
                throw new BusinessRuleViolationException("A carteira selecionada está inativa.");

            return wallet;
        }

        /// <inheritdoc />
        public async Task<Wallet?> ResolveWalletAsync(Guid userId, Guid? walletId)
        {
            if (walletId.HasValue)
                return await this.EnsureWalletAsync(userId, walletId.Value);

            return await this.repository.GetDefaultAsync(userId);
        }

        #endregion

        #region Methods :: GetTransfersAsync(), CreateTransferAsync(), DeleteTransferAsync()

        /// <inheritdoc />
        public async Task<IReadOnlyList<WalletTransferResponse>> GetTransfersAsync(Guid userId, Guid? walletId, int limit)
        {
            IReadOnlyList<WalletTransfer> transfers = await this.repository.GetTransfersAsync(userId, walletId, limit);

            return transfers
                .Select(transfer => new WalletTransferResponse(transfer))
                .ToList();
        }

        /// <inheritdoc />
        public async Task<WalletTransferResponse> CreateTransferAsync(Guid userId, WalletTransferRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.FromWalletId == request.ToWalletId)
                throw new BusinessRuleViolationException("As carteiras de origem e destino devem ser diferentes.");

            Wallet from = await this.EnsureWalletAsync(userId, request.FromWalletId);
            Wallet to = await this.EnsureWalletAsync(userId, request.ToWalletId);

            // Vale não vira dinheiro: transferir saldo de VR para a conta corrente descreveria
            // uma operação que não existe e inflaria o dinheiro livre do usuário.
            if (WalletKindHelper.IsBenefit(from.Kind) && !WalletKindHelper.IsBenefit(to.Kind))
                throw new BusinessRuleViolationException("Não é possível transferir saldo de um benefício para uma carteira de dinheiro.");

            WalletTransfer transfer = new WalletTransfer()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FromWalletId = from.Id,
                ToWalletId = to.Id,
                Amount = request.Amount,
                OccurredOn = DateTimeHelper.ToUtcDate(request.OccurredOn),
                Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
                CreatedAt = DateTime.UtcNow,
                FromWallet = from,
                ToWallet = to
            };

            bool created = await this.repository.CreateTransferAsync(transfer);

            if (!created)
                throw new BusinessRuleViolationException("Não foi possível registrar a transferência.");

            this.logger.LogInformation(
                "Transferência {TransferId} de {Amount} entre carteiras registrada pelo usuário {UserId}.",
                transfer.Id,
                transfer.Amount,
                userId);

            return new WalletTransferResponse(transfer);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteTransferAsync(Guid userId, Guid transferId)
        {
            WalletTransfer? transfer = await this.repository.GetTransferByIdAsync(userId, transferId);

            if (transfer is null)
                return false;

            bool deleted = await this.repository.DeleteTransferAsync(transfer);

            if (deleted)
                this.logger.LogInformation("Transferência {TransferId} removida pelo usuário {UserId}.", transferId, userId);

            return deleted;
        }

        #endregion
    }
}
