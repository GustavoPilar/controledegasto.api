using ControleDeGasto.API.Application.DTOs;
using ControleDeGasto.API.Application.Interfaces;
using ControleDeGasto.API.Application.Utilities;
using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Exceptions;
using ControleDeGasto.API.Domain.Interfaces;
using ControleDeGasto.API.Domain.Queries;
using ControleDeGasto.API.Domain.ReadModels;

namespace ControleDeGasto.API.Application.Services
{
    public class TransactionService(
        ITransactionRepository repository,
        ICategoryRepository categoryRepository,
        ILogger<TransactionService> logger) : ITransactionService
    {
        #region Fields

        private readonly ITransactionRepository repository = repository;
        private readonly ICategoryRepository categoryRepository = categoryRepository;
        private readonly ILogger<TransactionService> logger = logger;

        #endregion

        #region Helpers :: EnsureCategoryAsync()

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

            TransactionQuery query = new TransactionQuery(
                userId,
                filter.From.HasValue ? DateTimeHelper.ToUtcDate(filter.From.Value) : null,
                filter.To.HasValue ? DateTimeHelper.ToUtcEndOfDay(filter.To.Value) : null,
                filter.CategoryId,
                filter.Type,
                filter.Search,
                page,
                pageSize);

            PagedResult<Transaction> result = await this.repository.GetPagedAsync(query);

            List<TransactionResponse> items = result.Items
                .Select(transaction => new TransactionResponse(transaction))
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

            Transaction transaction = new Transaction()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CategoryId = category.Id,
                Amount = request.Amount,
                Description = request.Description.Trim(),
                OccurredOn = DateTimeHelper.ToUtcDate(request.OccurredOn),
                PaymentMethod = request.PaymentMethod,
                CreatedAt = DateTime.UtcNow,
                Category = category
            };

            bool created = await this.repository.CreateAsync(transaction);

            if (!created)
                throw new BusinessRuleViolationException("Não foi possível registrar o lançamento.");

            this.logger.LogInformation(
                "Lançamento {TransactionId} de {Amount} criado para o usuário {UserId}.",
                transaction.Id,
                transaction.Amount,
                userId);

            return new TransactionResponse(transaction);
        }

        /// <inheritdoc />
        public async Task<TransactionResponse?> UpdateAsync(Guid userId, Guid transactionId, TransactionRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            Transaction? transaction = await this.repository.GetByIdAsync(userId, transactionId);

            if (transaction is null)
                return null;

            Category category = await this.EnsureCategoryAsync(userId, request.CategoryId);

            transaction.CategoryId = category.Id;
            transaction.Amount = request.Amount;
            transaction.Description = request.Description.Trim();
            transaction.OccurredOn = DateTimeHelper.ToUtcDate(request.OccurredOn);
            transaction.PaymentMethod = request.PaymentMethod;
            transaction.UpdatedAt = DateTime.UtcNow;
            transaction.Category = category;

            bool updated = await this.repository.UpdateAsync(transaction);

            if (!updated)
                throw new BusinessRuleViolationException("Não foi possível atualizar o lançamento.");

            this.logger.LogInformation("Lançamento {TransactionId} atualizado pelo usuário {UserId}.", transactionId, userId);

            return new TransactionResponse(transaction);
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
    }
}
