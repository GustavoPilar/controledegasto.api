using ControleDeGasto.API.Application.DTOs;
using ControleDeGasto.API.Application.Interfaces;
using ControleDeGasto.API.Application.Utilities;
using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;
using ControleDeGasto.API.Domain.Exceptions;
using ControleDeGasto.API.Domain.Interfaces;

namespace ControleDeGasto.API.Application.Services
{
    public class FixedEntryService(
        IFixedEntryRepository repository,
        ICategoryRepository categoryRepository,
        IWalletRepository walletRepository,
        ILogger<FixedEntryService> logger) : IFixedEntryService
    {
        #region Fields

        private readonly IFixedEntryRepository repository = repository;
        private readonly ICategoryRepository categoryRepository = categoryRepository;
        private readonly IWalletRepository walletRepository = walletRepository;
        private readonly ILogger<FixedEntryService> logger = logger;

        #endregion

        #region Helpers :: ValidateAsync()

        /// <summary>
        /// Valida a definição e resolve a categoria e a carteira conforme a natureza.
        /// </summary>
        /// <remarks>
        /// A categoria é obrigatória em entrada e saída porque a previsão por categoria depende
        /// dela; no crédito de benefício ela é dispensada, pois o crédito não classifica gasto —
        /// só abastece a carteira, que aí passa a ser obrigatória.
        /// </remarks>
        /// <param name="userId">Dono da definição.</param>
        /// <param name="request">Dados informados.</param>
        /// <param name="excludeFixedEntryId">Definição a ignorar na checagem de duplicidade.</param>
        /// <returns>Categoria e carteira resolvidas.</returns>
        /// <exception cref="BusinessRuleViolationException">Dados incompatíveis com a natureza.</exception>
        private async Task<(Category? Category, Wallet? Wallet)> ValidateAsync(Guid userId, FixedEntryRequest request, Guid? excludeFixedEntryId)
        {
            string description = request.Description.Trim();

            bool duplicated = await this.repository.ExistsByDescriptionAsync(userId, request.Kind, description, excludeFixedEntryId);

            if (duplicated)
                throw new BusinessRuleViolationException("Já existe um valor fixo com essa descrição.");

            if (request.EndsOn.HasValue && request.EndsOn.Value < request.StartsOn)
                throw new BusinessRuleViolationException("O fim da vigência não pode ser antes do início.");

            Category? category = null;
            Wallet? wallet = null;

            if (request.WalletId.HasValue)
            {
                wallet = await this.walletRepository.GetByIdAsync(userId, request.WalletId.Value);

                if (wallet is null)
                    throw new BusinessRuleViolationException("Carteira não encontrada.");

                if (!wallet.Active)
                    throw new BusinessRuleViolationException("A carteira selecionada está inativa.");
            }

            if (request.Kind == FixedEntryKind.BenefitCredit)
            {
                if (wallet is null)
                    throw new BusinessRuleViolationException("Informe a carteira de benefício que recebe o crédito.");

                if (!WalletKindHelper.IsBenefit(wallet.Kind))
                    throw new BusinessRuleViolationException("O crédito de benefício exige uma carteira de vale (VR, VA, VT, VC ou vale-cultura).");

                return (null, wallet);
            }

            if (!request.CategoryId.HasValue)
                throw new BusinessRuleViolationException("Informe a categoria.");

            category = await this.categoryRepository.GetByIdAsync(userId, request.CategoryId.Value);

            if (category is null)
                throw new BusinessRuleViolationException("Categoria não encontrada.");

            if (!category.Active)
                throw new BusinessRuleViolationException("A categoria selecionada está inativa.");

            // A natureza da categoria precisa combinar com a da definição: salário classificado
            // como saída fixa produziria uma previsão com o sinal invertido.
            TransactionType expected = request.Kind == FixedEntryKind.Income
                ? TransactionType.Income
                : TransactionType.Expense;

            if (category.Type != expected)
            {
                throw new BusinessRuleViolationException(request.Kind == FixedEntryKind.Income
                    ? "Escolha uma categoria de entrada para uma renda fixa."
                    : "Escolha uma categoria de saída para uma despesa fixa.");
            }

            return (category, wallet);
        }

        #endregion

        #region Methods :: GetAllAsync(), GetByIdAsync()

        /// <inheritdoc />
        public async Task<IReadOnlyList<FixedEntryResponse>> GetAllAsync(Guid userId, bool includeInactive)
        {
            IReadOnlyList<FixedEntry> entries = await this.repository.GetAllAsync(userId, includeInactive);

            return entries
                .Select(entry => new FixedEntryResponse(entry))
                .ToList();
        }

        /// <inheritdoc />
        public async Task<FixedEntryResponse?> GetByIdAsync(Guid userId, Guid fixedEntryId)
        {
            FixedEntry? entry = await this.repository.GetByIdAsync(userId, fixedEntryId);

            return entry is null ? null : new FixedEntryResponse(entry);
        }

        #endregion

        #region Methods :: CreateAsync(), UpdateAsync(), SetActiveAsync(), DeleteAsync()

        /// <inheritdoc />
        public async Task<FixedEntryResponse> CreateAsync(Guid userId, FixedEntryRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            (Category? category, Wallet? wallet) = await this.ValidateAsync(userId, request, null);

            FixedEntry entry = new FixedEntry()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Kind = request.Kind,
                CategoryId = category?.Id,
                WalletId = wallet?.Id,
                Description = request.Description.Trim(),
                Amount = request.Amount,
                DayOfMonth = request.DayOfMonth,

                // A vigência é guardada no primeiro dia do mês: o dia exato de cada ocorrência
                // vem de DayOfMonth, e misturar os dois faria uma definição do dia 5 começando
                // no dia 20 perder o primeiro mês sem motivo.
                StartsOn = DateTimeHelper.StartOfMonth(request.StartsOn),
                EndsOn = request.EndsOn.HasValue ? DateTimeHelper.EndOfMonth(request.EndsOn.Value) : null,
                Active = request.Active,
                CreatedAt = DateTime.UtcNow,
                Category = category,
                Wallet = wallet
            };

            bool created = await this.repository.CreateAsync(entry);

            if (!created)
                throw new BusinessRuleViolationException("Não foi possível criar o valor fixo.");

            this.logger.LogInformation(
                "Valor fixo {FixedEntryId} ({Kind}) de {Amount} criado para o usuário {UserId}.",
                entry.Id,
                entry.Kind,
                entry.Amount,
                userId);

            return new FixedEntryResponse(entry);
        }

        /// <inheritdoc />
        public async Task<FixedEntryResponse?> UpdateAsync(Guid userId, Guid fixedEntryId, FixedEntryRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            FixedEntry? entry = await this.repository.GetByIdAsync(userId, fixedEntryId);

            if (entry is null)
                return null;

            (Category? category, Wallet? wallet) = await this.ValidateAsync(userId, request, fixedEntryId);

            entry.Kind = request.Kind;
            entry.CategoryId = category?.Id;
            entry.WalletId = wallet?.Id;
            entry.Description = request.Description.Trim();
            entry.Amount = request.Amount;
            entry.DayOfMonth = request.DayOfMonth;
            entry.StartsOn = DateTimeHelper.StartOfMonth(request.StartsOn);
            entry.EndsOn = request.EndsOn.HasValue ? DateTimeHelper.EndOfMonth(request.EndsOn.Value) : null;
            entry.Active = request.Active;
            entry.UpdatedAt = DateTime.UtcNow;
            entry.Category = category;
            entry.Wallet = wallet;

            bool updated = await this.repository.UpdateAsync(entry);

            if (!updated)
                throw new BusinessRuleViolationException("Não foi possível atualizar o valor fixo.");

            this.logger.LogInformation("Valor fixo {FixedEntryId} atualizado pelo usuário {UserId}.", fixedEntryId, userId);

            return new FixedEntryResponse(entry);
        }

        /// <inheritdoc />
        public async Task<FixedEntryResponse?> SetActiveAsync(Guid userId, Guid fixedEntryId, bool active)
        {
            FixedEntry? entry = await this.repository.GetByIdAsync(userId, fixedEntryId);

            if (entry is null)
                return null;

            if (entry.Active == active)
                return new FixedEntryResponse(entry);

            entry.Active = active;
            entry.UpdatedAt = DateTime.UtcNow;

            bool updated = await this.repository.UpdateAsync(entry);

            if (!updated)
                throw new BusinessRuleViolationException("Não foi possível alterar a situação do valor fixo.");

            return new FixedEntryResponse(entry);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(Guid userId, Guid fixedEntryId)
        {
            FixedEntry? entry = await this.repository.GetByIdAsync(userId, fixedEntryId);

            if (entry is null)
                return false;

            bool deleted = await this.repository.DeleteAsync(entry);

            if (deleted)
                this.logger.LogInformation("Valor fixo {FixedEntryId} removido pelo usuário {UserId}.", fixedEntryId, userId);

            return deleted;
        }

        #endregion
    }
}
