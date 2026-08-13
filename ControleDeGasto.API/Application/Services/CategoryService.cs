using ControleDeGasto.API.Application.DTOs;
using ControleDeGasto.API.Application.Interfaces;
using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;
using ControleDeGasto.API.Domain.Exceptions;
using ControleDeGasto.API.Domain.Interfaces;

namespace ControleDeGasto.API.Application.Services
{
    public class CategoryService(
        ICategoryRepository repository,
        ILogger<CategoryService> logger) : ICategoryService
    {
        #region Constants :: DEFAULT_CATEGORIES

        /// <summary>
        /// Categorias criadas junto com a conta, para o usuário conseguir lançar algo no
        /// primeiro acesso sem precisar cadastrar nada antes.
        /// </summary>
        private static readonly (string Name, TransactionType Type, string Color, string Icon)[] DEFAULT_CATEGORIES =
        [
            ("Alimentação", TransactionType.Expense, "#E76F51", "shopping-cart"),
            ("Moradia", TransactionType.Expense, "#264653", "home"),
            ("Transporte", TransactionType.Expense, "#2A9D8F", "car"),
            ("Saúde", TransactionType.Expense, "#E63946", "heart"),
            ("Educação", TransactionType.Expense, "#457B9D", "book"),
            ("Lazer", TransactionType.Expense, "#F4A261", "star"),
            ("Contas e Assinaturas", TransactionType.Expense, "#6D6875", "receipt"),
            ("Outras saídas", TransactionType.Expense, "#8D99AE", "ellipsis-h"),
            ("Salário", TransactionType.Income, "#2A9D8F", "wallet"),
            ("Freelance", TransactionType.Income, "#457B9D", "briefcase"),
            ("Investimentos", TransactionType.Income, "#C5A44E", "chart-line"),
            ("Outras entradas", TransactionType.Income, "#8D99AE", "plus")
        ];

        #endregion

        #region Fields

        private readonly ICategoryRepository repository = repository;
        private readonly ILogger<CategoryService> logger = logger;

        #endregion

        #region Methods :: GetAllAsync(), GetByIdAsync()

        /// <inheritdoc />
        public async Task<IReadOnlyList<CategoryResponse>> GetAllAsync(Guid userId, bool onlyActive)
        {
            IReadOnlyList<Category> categories = await this.repository.GetAllAsync(userId, onlyActive);

            return categories.Select(category => new CategoryResponse(category)).ToList();
        }

        /// <inheritdoc />
        public async Task<CategoryResponse?> GetByIdAsync(Guid userId, Guid categoryId)
        {
            Category? category = await this.repository.GetByIdAsync(userId, categoryId);

            return category is null ? null : new CategoryResponse(category);
        }

        #endregion

        #region Methods :: CreateAsync(), UpdateAsync(), DeactivateAsync()

        /// <inheritdoc />
        public async Task<CategoryResponse> CreateAsync(Guid userId, CategoryRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            string name = request.Name.Trim();

            bool exists = await this.repository.ExistsByNameAsync(userId, name, request.Type, null);

            if (exists)
                throw new BusinessRuleViolationException("Já existe uma categoria com esse nome para o mesmo tipo.");

            Category category = new Category()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = name,
                Type = request.Type,
                Color = request.Color.ToUpperInvariant(),
                Icon = request.Icon.Trim(),
                IsDefault = false,
                Active = true,
                CreatedAt = DateTime.UtcNow
            };

            bool created = await this.repository.CreateAsync(category);

            if (!created)
                throw new BusinessRuleViolationException("Não foi possível criar a categoria.");

            this.logger.LogInformation("Categoria {CategoryId} criada para o usuário {UserId}.", category.Id, userId);

            return new CategoryResponse(category);
        }

        /// <inheritdoc />
        public async Task<CategoryResponse?> UpdateAsync(Guid userId, Guid categoryId, CategoryRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            Category? category = await this.repository.GetByIdAsync(userId, categoryId);

            if (category is null)
                return null;

            string name = request.Name.Trim();

            bool exists = await this.repository.ExistsByNameAsync(userId, name, request.Type, categoryId);

            if (exists)
                throw new BusinessRuleViolationException("Já existe uma categoria com esse nome para o mesmo tipo.");

            // Trocar o tipo de uma categoria com histórico inverteria o sinal de lançamentos já
            // fechados: uma despesa passada viraria receita em todos os relatórios.
            if (category.Type != request.Type)
            {
                bool hasTransactions = await this.repository.HasTransactionsAsync(userId, categoryId);

                if (hasTransactions)
                    throw new BusinessRuleViolationException("Não é possível trocar o tipo de uma categoria que já possui lançamentos.");
            }

            category.Name = name;
            category.Type = request.Type;
            category.Color = request.Color.ToUpperInvariant();
            category.Icon = request.Icon.Trim();
            category.UpdatedAt = DateTime.UtcNow;

            bool updated = await this.repository.UpdateAsync(category);

            if (!updated)
                throw new BusinessRuleViolationException("Não foi possível atualizar a categoria.");

            this.logger.LogInformation("Categoria {CategoryId} atualizada pelo usuário {UserId}.", categoryId, userId);

            return new CategoryResponse(category);
        }

        /// <inheritdoc />
        public async Task<bool> DeactivateAsync(Guid userId, Guid categoryId)
        {
            Category? category = await this.repository.GetByIdAsync(userId, categoryId);

            if (category is null)
                return false;

            if (!category.Active)
                return true;

            // Exclusão lógica sempre, mesmo sem lançamentos: mantém um caminho único e evita
            // que a chave estrangeira dos lançamentos aponte para um registro removido.
            category.Active = false;
            category.UpdatedAt = DateTime.UtcNow;

            bool updated = await this.repository.UpdateAsync(category);

            if (updated)
                this.logger.LogInformation("Categoria {CategoryId} desativada pelo usuário {UserId}.", categoryId, userId);

            return updated;
        }

        #endregion

        #region Methods :: CreateDefaultsAsync()

        /// <inheritdoc />
        public async Task<int> CreateDefaultsAsync(Guid userId)
        {
            DateTime createdAt = DateTime.UtcNow;

            List<Category> categories = DEFAULT_CATEGORIES
                .Select(item => new Category()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Name = item.Name,
                    Type = item.Type,
                    Color = item.Color,
                    Icon = item.Icon,
                    IsDefault = true,
                    Active = true,
                    CreatedAt = createdAt
                })
                .ToList();

            bool created = await this.repository.CreateRangeAsync(categories);

            if (!created)
            {
                this.logger.LogError("Falha ao criar as categorias padrão do usuário {UserId}.", userId);
                return 0;
            }

            this.logger.LogInformation("{Count} categorias padrão criadas para o usuário {UserId}.", categories.Count, userId);

            return categories.Count;
        }

        #endregion
    }
}
