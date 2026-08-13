using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;

namespace ControleDeGasto.API.Domain.Interfaces
{
    /// <summary>
    /// Acesso a categorias. Todo método recebe o dono e filtra por ele.
    /// </summary>
    public interface ICategoryRepository
    {
        #region Methods :: GetAllAsync(), GetByIdAsync(), ExistsByNameAsync(), HasTransactionsAsync(), CreateAsync(), CreateRangeAsync(), UpdateAsync()

        /// <summary>
        /// Lista as categorias do usuário.
        /// </summary>
        /// <param name="userId">Dono das categorias.</param>
        /// <param name="onlyActive">Quando verdadeiro, omite as excluídas logicamente.</param>
        /// <returns>Categorias ordenadas por tipo e nome.</returns>
        Task<IReadOnlyList<Category>> GetAllAsync(Guid userId, bool onlyActive);

        /// <summary>
        /// Obtém uma categoria do usuário.
        /// </summary>
        /// <param name="userId">Dono da categoria.</param>
        /// <param name="categoryId">Identificador da categoria.</param>
        /// <returns>A categoria, ou nulo se não existir ou não pertencer ao usuário.</returns>
        Task<Category?> GetByIdAsync(Guid userId, Guid categoryId);

        /// <summary>
        /// Verifica se já existe categoria com o mesmo nome e tipo.
        /// </summary>
        /// <param name="userId">Dono da categoria.</param>
        /// <param name="name">Nome a verificar.</param>
        /// <param name="type">Natureza da categoria.</param>
        /// <param name="excludeCategoryId">Categoria a ignorar na verificação (usado na edição).</param>
        /// <returns>True se já existir.</returns>
        Task<bool> ExistsByNameAsync(Guid userId, string name, TransactionType type, Guid? excludeCategoryId);

        /// <summary>
        /// Indica se a categoria possui lançamentos.
        /// </summary>
        /// <param name="userId">Dono da categoria.</param>
        /// <param name="categoryId">Identificador da categoria.</param>
        /// <returns>True se houver ao menos um lançamento.</returns>
        Task<bool> HasTransactionsAsync(Guid userId, Guid categoryId);

        /// <summary>
        /// Persiste uma categoria nova.
        /// </summary>
        /// <param name="category">Categoria a gravar.</param>
        /// <returns>True se a gravação afetou alguma linha.</returns>
        Task<bool> CreateAsync(Category category);

        /// <summary>
        /// Persiste várias categorias em uma única gravação.
        /// </summary>
        /// <param name="categories">Categorias a gravar.</param>
        /// <returns>True se a gravação afetou alguma linha.</returns>
        Task<bool> CreateRangeAsync(IEnumerable<Category> categories);

        /// <summary>
        /// Persiste alterações de uma categoria.
        /// </summary>
        /// <param name="category">Categoria alterada.</param>
        /// <returns>True se a gravação afetou alguma linha.</returns>
        Task<bool> UpdateAsync(Category category);

        #endregion
    }
}
