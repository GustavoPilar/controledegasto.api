using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;
using ControleDeGasto.API.Domain.Interfaces;
using ControleDeGasto.API.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControleDeGasto.API.Infra.Repositories
{
    public class CategoryRepository(
        AppDbContext context) : ICategoryRepository
    {
        #region Fields

        private readonly AppDbContext context = context;

        #endregion

        #region Methods :: GetAllAsync(), GetByIdAsync(), ExistsByNameAsync(), HasTransactionsAsync()

        /// <inheritdoc />
        public async Task<IReadOnlyList<Category>> GetAllAsync(Guid userId, bool onlyActive)
        {
            IQueryable<Category> query = this.context.Categories
                .AsNoTracking()
                .Where(x => x.UserId == userId);

            if (onlyActive)
                query = query.Where(x => x.Active);

            return await query
                .OrderBy(x => x.Type)
                .ThenBy(x => x.Name)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<Category?> GetByIdAsync(Guid userId, Guid categoryId)
        {
            // O filtro por UserId acompanha o Id de propósito: um identificador de outro usuário
            // resulta em "não encontrado", sem revelar que o registro existe.
            return await this.context.Categories
                .FirstOrDefaultAsync(x => x.Id == categoryId && x.UserId == userId);
        }

        /// <inheritdoc />
        public async Task<bool> ExistsByNameAsync(Guid userId, string name, TransactionType type, Guid? excludeCategoryId)
        {
            IQueryable<Category> query = this.context.Categories
                .AsNoTracking()
                .Where(x => x.UserId == userId
                    && x.Active
                    && x.Type == type
                    && x.Name.ToLower() == name.ToLower());

            if (excludeCategoryId.HasValue)
                query = query.Where(x => x.Id != excludeCategoryId.Value);

            return await query.AnyAsync();
        }

        /// <inheritdoc />
        public async Task<bool> HasTransactionsAsync(Guid userId, Guid categoryId)
        {
            return await this.context.Transactions
                .AsNoTracking()
                .AnyAsync(x => x.UserId == userId && x.CategoryId == categoryId);
        }

        #endregion

        #region Methods :: CreateAsync(), CreateRangeAsync(), UpdateAsync()

        /// <inheritdoc />
        public async Task<bool> CreateAsync(Category category)
        {
            ArgumentNullException.ThrowIfNull(category);

            await this.context.Categories.AddAsync(category);

            return await this.context.SaveChangesAsync() > 0;
        }

        /// <inheritdoc />
        public async Task<bool> CreateRangeAsync(IEnumerable<Category> categories)
        {
            ArgumentNullException.ThrowIfNull(categories);

            await this.context.Categories.AddRangeAsync(categories);

            return await this.context.SaveChangesAsync() > 0;
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(Category category)
        {
            ArgumentNullException.ThrowIfNull(category);

            this.context.Categories.Update(category);

            return await this.context.SaveChangesAsync() > 0;
        }

        #endregion
    }
}
