using ControleDeGasto.API.Application.DTOs;
using ControleDeGasto.API.Application.Interfaces;
using ControleDeGasto.API.Application.Utilities;
using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;
using ControleDeGasto.API.Domain.Exceptions;
using ControleDeGasto.API.Domain.Interfaces;
using ControleDeGasto.API.Domain.Queries;
using ControleDeGasto.API.Domain.ReadModels;

namespace ControleDeGasto.API.Application.Services
{
    public class TransactionService(
        ITransactionRepository repository,
        ICategoryRepository categoryRepository,
        IWalletService walletService,
        ITagRepository tagRepository,
        IFriendshipRepository friendshipRepository,
        INotificationService notificationService,
        ILogger<TransactionService> logger) : ITransactionService
    {
        #region Fields

        private readonly ITransactionRepository repository = repository;
        private readonly ICategoryRepository categoryRepository = categoryRepository;
        private readonly IWalletService walletService = walletService;
        private readonly ITagRepository tagRepository = tagRepository;
        private readonly IFriendshipRepository friendshipRepository = friendshipRepository;
        private readonly INotificationService notificationService = notificationService;
        private readonly ILogger<TransactionService> logger = logger;

        #endregion

        #region Helpers :: EnsureCategoryAsync(), EnsureTagsAsync(), BuildSharesAsync()

        /// <summary>
        /// Garante que a categoria informada existe, pertence ao usuário e está ativa.
        /// </summary>
        /// <remarks>
        /// É a checagem de posse do lançamento: sem ela, um cliente poderia classificar seus
        /// lançamentos com a categoria de outra conta e vazar o nome dela na listagem.
        /// </remarks>
        /// <param name="userId">Dono do lançamento.</param>
        /// <param name="categoryId">Categoria escolhida.</param>
        /// <returns>A categoria validada.</returns>
        /// <exception cref="BusinessRuleViolationException">Categoria inválida para o usuário.</exception>
        private async Task<Category> EnsureCategoryAsync(Guid userId, Guid categoryId)
        {
            Category? category = await this.categoryRepository.GetByIdAsync(userId, categoryId);

            if (category is null)
                throw new BusinessRuleViolationException("Categoria não encontrada.");

            if (!category.Active)
                throw new BusinessRuleViolationException("A categoria selecionada está inativa.");

            return category;
        }

        /// <summary>
        /// Garante que todas as etiquetas informadas pertencem ao usuário.
        /// </summary>
        /// <param name="userId">Dono das etiquetas.</param>
        /// <param name="tagIds">Etiquetas escolhidas.</param>
        /// <returns>Etiquetas validadas, sem repetição.</returns>
        /// <exception cref="BusinessRuleViolationException">Alguma etiqueta não pertence ao usuário.</exception>
        private async Task<IReadOnlyList<Guid>> EnsureTagsAsync(Guid userId, IReadOnlyList<Guid> tagIds)
        {
            if (tagIds.Count == 0)
                return [];

            List<Guid> distinct = tagIds.Distinct().ToList();

            IReadOnlyList<Guid> owned = await this.tagRepository.FilterOwnedAsync(userId, distinct);

            if (owned.Count != distinct.Count)
                throw new BusinessRuleViolationException("Uma das etiquetas informadas não foi encontrada.");

            return distinct;
        }

        /// <summary>
        /// Valida a divisão informada e monta as partes a gravar.
        /// </summary>
        /// <remarks>
        /// Duas regras sustentam o resto: só amigos entram na divisão, e a soma das partes não
        /// pode passar do valor da compra. Sem a primeira, seria possível criar dívidas para
        /// desconhecidos; sem a segunda, o dono do lançamento apareceria com gasto negativo.
        /// </remarks>
        /// <param name="userId">Dono do lançamento.</param>
        /// <param name="transactionId">Lançamento que está sendo dividido.</param>
        /// <param name="amount">Valor total do lançamento.</param>
        /// <param name="shares">Partes informadas.</param>
        /// <returns>Partes prontas para gravação.</returns>
        /// <exception cref="BusinessRuleViolationException">Participante não é amigo ou soma inválida.</exception>
        private async Task<IReadOnlyList<TransactionShare>> BuildSharesAsync(
            Guid userId,
            Guid transactionId,
            decimal amount,
            IReadOnlyList<TransactionShareRequest> shares)
        {
            if (shares.Count == 0)
                return [];

            if (shares.Any(item => item.FriendUserId == userId))
                throw new BusinessRuleViolationException("Você não entra na divisão: sua parte é o que sobra do valor total.");

            List<Guid> friendIds = shares.Select(item => item.FriendUserId).Distinct().ToList();

            if (friendIds.Count != shares.Count)
                throw new BusinessRuleViolationException("Cada amigo pode aparecer uma única vez na divisão.");

            IReadOnlyList<Guid> confirmed = await this.friendshipRepository.FilterFriendsAsync(userId, friendIds);

            if (confirmed.Count != friendIds.Count)
                throw new BusinessRuleViolationException("Só é possível dividir uma compra com amigos.");

            decimal total = shares.Sum(item => item.Amount);

            if (total > amount)
                throw new BusinessRuleViolationException("A soma das partes não pode ser maior que o valor do lançamento.");

            DateTime now = DateTime.UtcNow;

            return shares
                .Select(item => new TransactionShare()
                {
                    Id = Guid.NewGuid(),
                    TransactionId = transactionId,
                    FriendUserId = item.FriendUserId,
                    Amount = item.Amount,
                    CreatedAt = now
                })
                .ToList();
        }

        #endregion

        #region Helpers :: ResolveStatus(), NotifySharesAsync()

        /// <summary>
        /// Resolve a situação e as datas de liquidação a partir do que o cliente pediu.
        /// </summary>
        /// <remarks>
        /// Um lançamento previsto sem vencimento assume a própria data de competência: é o que
        /// permite o aviso de vencimento e a ordenação por "o que vence primeiro" funcionarem
        /// sem obrigar o usuário a preencher dois campos com o mesmo dia.
        /// </remarks>
        /// <param name="transaction">Lançamento a ajustar.</param>
        /// <param name="request">Dados informados.</param>
        private static void ResolveStatus(Transaction transaction, TransactionRequest request)
        {
            transaction.Status = request.Status;

            transaction.DueDate = request.DueDate.HasValue
                ? DateTimeHelper.ToUtcDate(request.DueDate.Value)
                : null;

            if (request.Status == TransactionStatus.Pending)
            {
                transaction.SettledAt = null;
                transaction.DueDate ??= transaction.OccurredOn;

                return;
            }

            transaction.SettledAt ??= DateTime.UtcNow;
        }

        /// <summary>
        /// Avisa os amigos que receberam uma parte na divisão.
        /// </summary>
        /// <param name="userId">Quem dividiu.</param>
        /// <param name="description">Descrição do lançamento.</param>
        /// <param name="shares">Partes gravadas.</param>
        private async Task NotifySharesAsync(Guid userId, string description, IReadOnlyList<TransactionShare> shares)
        {
            if (shares.Count == 0)
                return;

            IReadOnlyList<UserSummary> summaries = await this.friendshipRepository.GetUserSummariesAsync([userId]);

            string payerName = summaries.FirstOrDefault()?.FullName ?? "Um amigo";

            foreach (TransactionShare share in shares)
            {
                await this.notificationService.CreateAsync(
                    share.FriendUserId,
                    NotificationType.ExpenseShared,
                    "Compra dividida com você",
                    $"{payerName} dividiu \"{description}\" e sua parte é de {share.Amount:C2}.",
                    share.Id,
                    dedupeWindow: null);
            }
        }

        #endregion

        #region Methods :: GetPagedAsync(), GetByIdAsync()

        /// <inheritdoc />
        public async Task<PagedResponse<TransactionResponse>> GetPagedAsync(Guid userId, TransactionFilterRequest filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            int page = filter.Page < 1 ? TransactionFilterRequest.DEFAULT_PAGE : filter.Page;

            int pageSize = filter.PageSize is < 1 or > TransactionFilterRequest.MAX_PAGE_SIZE
                ? TransactionFilterRequest.DEFAULT_PAGE_SIZE
                : filter.PageSize;

            if (filter.MinAmount.HasValue && filter.MaxAmount.HasValue && filter.MinAmount > filter.MaxAmount)
                throw new BusinessRuleViolationException("O valor mínimo não pode ser maior que o valor máximo.");

            DateTime reference = DateTime.UtcNow;

            TransactionQuery query = new TransactionQuery(
                userId,
                filter.From.HasValue ? DateTimeHelper.ToUtcDate(filter.From.Value) : null,
                filter.To.HasValue ? DateTimeHelper.ToUtcEndOfDay(filter.To.Value) : null,
                filter.CategoryId,
                filter.Type,
                filter.Search,
                filter.WalletId,
                filter.TagIds,
                filter.Status,
                filter.PaymentMethod,
                filter.MinAmount,
                filter.MaxAmount,
                filter.DueFrom.HasValue ? DateTimeHelper.ToUtcDate(filter.DueFrom.Value) : null,
                filter.DueTo.HasValue ? DateTimeHelper.ToUtcEndOfDay(filter.DueTo.Value) : null,
                filter.OnlyOverdue,
                filter.OnlyShared,
                filter.OnlyInstallments,
                filter.InstallmentPlanId,
                DateTimeHelper.ToUtcDate(reference),
                filter.SortBy,
                filter.SortDescending,
                page,
                pageSize);

            PagedResult<Transaction> result = await this.repository.GetPagedAsync(query);

            List<TransactionResponse> items = result.Items
                .Select(transaction => new TransactionResponse(transaction, reference))
                .ToList();

            return new PagedResponse<TransactionResponse>(items, result.TotalCount, page, pageSize);
        }

        /// <inheritdoc />
        public async Task<TransactionResponse?> GetByIdAsync(Guid userId, Guid transactionId)
        {
            Transaction? transaction = await this.repository.GetByIdAsync(userId, transactionId);

            return transaction is null ? null : new TransactionResponse(transaction);
        }

        #endregion

        #region Methods :: CreateAsync(), UpdateAsync(), DeleteAsync()

        /// <inheritdoc />
        public async Task<TransactionResponse> CreateAsync(Guid userId, TransactionRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            Category category = await this.EnsureCategoryAsync(userId, request.CategoryId);
            Wallet? wallet = await this.walletService.ResolveWalletAsync(userId, request.WalletId);

            IReadOnlyList<Guid> tagIds = await this.EnsureTagsAsync(userId, request.TagIds);

            Guid transactionId = Guid.NewGuid();

            IReadOnlyList<TransactionShare> shares = await this.BuildSharesAsync(
                userId,
                transactionId,
                request.Amount,
                request.Shares);

            Transaction transaction = new Transaction()
            {
                Id = transactionId,
                UserId = userId,
                CategoryId = category.Id,
                WalletId = wallet?.Id,
                Amount = request.Amount,
                Description = request.Description.Trim(),
                OccurredOn = DateTimeHelper.ToUtcDate(request.OccurredOn),
                PaymentMethod = request.PaymentMethod,
                CreatedAt = DateTime.UtcNow,
                Category = category,
                Wallet = wallet
            };

            ResolveStatus(transaction, request);

            bool created = await this.repository.CreateAsync(transaction);

            if (!created)
                throw new BusinessRuleViolationException("Não foi possível registrar o lançamento.");

            if (tagIds.Count > 0)
                await this.repository.ReplaceTagsAsync(transactionId, tagIds);

            if (shares.Count > 0)
            {
                await this.repository.ReplaceSharesAsync(transactionId, shares);
                await this.NotifySharesAsync(userId, transaction.Description, shares);
            }

            this.logger.LogInformation(
                "Lançamento {TransactionId} de {Amount} criado para o usuário {UserId} com situação {Status}.",
                transaction.Id,
                transaction.Amount,
                userId,
                transaction.Status);

            // Recarrega para a resposta trazer etiquetas e divisões já com os nomes: montá-las
            // à mão duplicaria a lógica de projeção que o repositório já faz.
            Transaction? reloaded = await this.repository.GetByIdAsync(userId, transactionId);

            return new TransactionResponse(reloaded ?? transaction);
        }

        /// <inheritdoc />
        public async Task<TransactionResponse?> UpdateAsync(Guid userId, Guid transactionId, TransactionRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            Transaction? transaction = await this.repository.GetByIdAsync(userId, transactionId);

            if (transaction is null)
                return null;

            Category category = await this.EnsureCategoryAsync(userId, request.CategoryId);
            Wallet? wallet = await this.walletService.ResolveWalletAsync(userId, request.WalletId);

            IReadOnlyList<Guid> tagIds = await this.EnsureTagsAsync(userId, request.TagIds);

            IReadOnlyList<TransactionShare> shares = await this.BuildSharesAsync(
                userId,
                transactionId,
                request.Amount,
                request.Shares);

            transaction.CategoryId = category.Id;
            transaction.WalletId = wallet?.Id;
            transaction.Amount = request.Amount;
            transaction.Description = request.Description.Trim();
            transaction.OccurredOn = DateTimeHelper.ToUtcDate(request.OccurredOn);
            transaction.PaymentMethod = request.PaymentMethod;
            transaction.UpdatedAt = DateTime.UtcNow;
            transaction.Category = category;
            transaction.Wallet = wallet;

            ResolveStatus(transaction, request);

            bool updated = await this.repository.UpdateAsync(transaction);

            if (!updated)
                throw new BusinessRuleViolationException("Não foi possível atualizar o lançamento.");

            await this.repository.ReplaceTagsAsync(transactionId, tagIds);

            IReadOnlyList<TransactionShare> previous = transaction.Shares?.ToList() ?? [];

            await this.repository.ReplaceSharesAsync(transactionId, shares);

            // Avisa apenas quem entrou agora: quem já estava na divisão foi avisado antes, e
            // repetir o aviso a cada edição de descrição viraria ruído.
            IReadOnlyList<TransactionShare> added = shares
                .Where(item => previous.All(existing => existing.FriendUserId != item.FriendUserId))
                .ToList();

            await this.NotifySharesAsync(userId, transaction.Description, added);

            this.logger.LogInformation("Lançamento {TransactionId} atualizado pelo usuário {UserId}.", transactionId, userId);

            Transaction? reloaded = await this.repository.GetByIdAsync(userId, transactionId);

            return new TransactionResponse(reloaded ?? transaction);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(Guid userId, Guid transactionId)
        {
            Transaction? transaction = await this.repository.GetByIdAsync(userId, transactionId);

            if (transaction is null)
                return false;

            bool deleted = await this.repository.DeleteAsync(transaction);

            if (deleted)
                this.logger.LogInformation("Lançamento {TransactionId} removido pelo usuário {UserId}.", transactionId, userId);

            return deleted;
        }

        #endregion

        #region Methods :: SettleAsync()

        /// <inheritdoc />
        public async Task<TransactionResponse?> SettleAsync(Guid userId, Guid transactionId, TransactionSettleRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            Transaction? transaction = await this.repository.GetByIdAsync(userId, transactionId);

            if (transaction is null)
                return null;

            if (request.Settled)
            {
                transaction.Status = TransactionStatus.Settled;

                transaction.SettledAt = request.SettledOn.HasValue
                    ? DateTimeHelper.ToUtcDate(request.SettledOn.Value)
                    : DateTime.UtcNow;
            }
            else
            {
                transaction.Status = TransactionStatus.Pending;
                transaction.SettledAt = null;

                // Reabrir sem vencimento deixaria a conta fora do bloco "a vencer" e fora do
                // "vencidas": a competência serve de vencimento até o usuário informar um.
                transaction.DueDate ??= transaction.OccurredOn;
            }

            transaction.UpdatedAt = DateTime.UtcNow;

            bool updated = await this.repository.UpdateAsync(transaction);

            if (!updated)
                throw new BusinessRuleViolationException("Não foi possível alterar a situação do lançamento.");

            this.logger.LogInformation(
                "Lançamento {TransactionId} passou para {Status} pelo usuário {UserId}.",
                transactionId,
                transaction.Status,
                userId);

            return new TransactionResponse(transaction);
        }

        #endregion

        #region Methods :: GetSharedWithMeAsync(), SettleShareAsync()

        /// <inheritdoc />
        public async Task<PagedResponse<SharedWithMeResponse>> GetSharedWithMeAsync(Guid userId, bool onlyOpen, int page, int pageSize)
        {
            int safePage = page < 1 ? TransactionFilterRequest.DEFAULT_PAGE : page;

            int safePageSize = pageSize is < 1 or > TransactionFilterRequest.MAX_PAGE_SIZE
                ? TransactionFilterRequest.DEFAULT_PAGE_SIZE
                : pageSize;

            PagedResult<TransactionShare> result = await this.repository
                .GetSharedWithUserAsync(userId, onlyOpen, safePage, safePageSize);

            List<SharedWithMeResponse> items = result.Items
                .Select(share => new SharedWithMeResponse(share))
                .ToList();

            return new PagedResponse<SharedWithMeResponse>(items, result.TotalCount, safePage, safePageSize);
        }

        /// <inheritdoc />
        public async Task<TransactionShareResponse?> SettleShareAsync(Guid userId, Guid shareId, bool settled)
        {
            TransactionShare? share = await this.repository.GetShareByIdAsync(userId, shareId);

            if (share is null)
                return null;

            if (share.SettledAt.HasValue == settled)
                return new TransactionShareResponse(share);

            share.SettledAt = settled ? DateTime.UtcNow : null;

            bool updated = await this.repository.UpdateShareAsync(share);

            if (!updated)
                throw new BusinessRuleViolationException("Não foi possível alterar a situação da divisão.");

            if (settled)
            {
                // Avisa o outro lado da relação, seja ele o pagador ou o devedor.
                Guid counterpartId = share.FriendUserId == userId
                    ? share.Transaction?.UserId ?? Guid.Empty
                    : share.FriendUserId;

                if (counterpartId != Guid.Empty)
                {
                    IReadOnlyList<UserSummary> summaries = await this.friendshipRepository.GetUserSummariesAsync([userId]);

                    string actorName = summaries.FirstOrDefault()?.FullName ?? "Um amigo";

                    await this.notificationService.CreateAsync(
                        counterpartId,
                        NotificationType.ExpenseShareSettled,
                        "Divisão acertada",
                        $"{actorName} marcou como acertada a parte de {share.Amount:C2} em \"{share.Transaction?.Description}\".",
                        share.Id,
                        dedupeWindow: null);
                }
            }

            this.logger.LogInformation(
                "Divisão {ShareId} marcada como {Status} pelo usuário {UserId}.",
                shareId,
                settled ? "acertada" : "em aberto",
                userId);

            return new TransactionShareResponse(share);
        }

        #endregion

        #region Methods :: GetInstallmentPlansAsync(), GetInstallmentPlanByIdAsync()

        /// <inheritdoc />
        public async Task<IReadOnlyList<InstallmentPlanResponse>> GetInstallmentPlansAsync(Guid userId, bool onlyOpen)
        {
            IReadOnlyList<InstallmentPlan> plans = await this.repository.GetInstallmentPlansAsync(userId, onlyOpen);

            return plans
                .Select(plan => new InstallmentPlanResponse(plan))
                .ToList();
        }

        /// <inheritdoc />
        public async Task<InstallmentPlanResponse?> GetInstallmentPlanByIdAsync(Guid userId, Guid installmentPlanId)
        {
            InstallmentPlan? plan = await this.repository.GetInstallmentPlanByIdAsync(userId, installmentPlanId);

            return plan is null ? null : new InstallmentPlanResponse(plan);
        }

        #endregion

        #region Methods :: CreateInstallmentPlanAsync(), DeleteInstallmentPlanAsync()

        /// <inheritdoc />
        public async Task<InstallmentPlanResponse> CreateInstallmentPlanAsync(Guid userId, InstallmentPlanRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            Category category = await this.EnsureCategoryAsync(userId, request.CategoryId);

            // Parcelar entrada não descreve nada que exista no aplicativo: uma venda a receber
            // em parcelas é modelada como contas a receber, cada uma com seu vencimento.
            if (category.Type != TransactionType.Expense)
                throw new BusinessRuleViolationException("Compras parceladas exigem uma categoria de saída.");

            Wallet? wallet = await this.walletService.ResolveWalletAsync(userId, request.WalletId);

            IReadOnlyList<Guid> tagIds = await this.EnsureTagsAsync(userId, request.TagIds);

            DateTime firstDueDate = DateTimeHelper.ToUtcDate(request.FirstDueDate);

            InstallmentPlan plan = new InstallmentPlan()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CategoryId = category.Id,
                WalletId = wallet?.Id,
                Description = request.Description.Trim(),
                TotalAmount = request.TotalAmount,
                InstallmentCount = request.InstallmentCount,
                FirstDueDate = firstDueDate,
                PaymentMethod = request.PaymentMethod,
                CreatedAt = DateTime.UtcNow,
                Category = category,
                Wallet = wallet
            };

            IReadOnlyList<decimal> amounts = MoneyHelper.SplitEvenly(request.TotalAmount, request.InstallmentCount);

            DateTime now = DateTime.UtcNow;

            List<Transaction> installments = new List<Transaction>(request.InstallmentCount);

            for (int index = 0; index < request.InstallmentCount; index++)
            {
                DateTime dueDate = FixedEntryHelper.ResolveDateInMonth(firstDueDate.Day, firstDueDate.AddMonths(index));

                installments.Add(new Transaction()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CategoryId = category.Id,
                    WalletId = wallet?.Id,
                    Amount = amounts[index],

                    // A descrição já carrega "3/12": é o que faz a parcela se explicar sozinha no
                    // extrato, sem depender de a tela ter carregado o plano.
                    Description = $"{plan.Description} ({index + 1}/{request.InstallmentCount})",

                    // A competência é o próprio vencimento: a parcela pertence ao mês em que vence,
                    // e é assim que ela aparece na previsão daquele mês.
                    OccurredOn = dueDate,
                    DueDate = dueDate,
                    PaymentMethod = request.PaymentMethod,
                    Status = TransactionStatus.Pending,
                    InstallmentPlanId = plan.Id,
                    InstallmentNumber = index + 1,
                    CreatedAt = now
                });
            }

            bool created = await this.repository.CreateInstallmentPlanAsync(plan, installments);

            if (!created)
                throw new BusinessRuleViolationException("Não foi possível registrar a compra parcelada.");

            if (tagIds.Count > 0)
            {
                foreach (Transaction installment in installments)
                    await this.repository.ReplaceTagsAsync(installment.Id, tagIds);
            }

            this.logger.LogInformation(
                "Compra parcelada {InstallmentPlanId} de {TotalAmount} em {InstallmentCount}x criada para o usuário {UserId}.",
                plan.Id,
                plan.TotalAmount,
                plan.InstallmentCount,
                userId);

            InstallmentPlan? reloaded = await this.repository.GetInstallmentPlanByIdAsync(userId, plan.Id);

            return new InstallmentPlanResponse(reloaded ?? plan);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteInstallmentPlanAsync(Guid userId, Guid installmentPlanId)
        {
            InstallmentPlan? plan = await this.repository.GetInstallmentPlanByIdAsync(userId, installmentPlanId);

            if (plan is null)
                return false;

            bool hasSettled = plan.Installments?.Any(item => item.Status == TransactionStatus.Settled) ?? false;

            if (hasSettled)
                throw new BusinessRuleViolationException("Esta compra já tem parcelas pagas. Exclua as parcelas em aberto individualmente.");

            bool deleted = await this.repository.DeleteInstallmentPlanAsync(plan);

            if (deleted)
                this.logger.LogInformation("Compra parcelada {InstallmentPlanId} cancelada pelo usuário {UserId}.", installmentPlanId, userId);

            return deleted;
        }

        #endregion
    }
}
