using ControleDeGasto.API.Application.DTOs;

namespace ControleDeGasto.API.Application.Interfaces
{
    /// <summary>
    /// Regras de categoria.
    /// </summary>
    public interface ICategoryService
    {
        #region Methods :: GetAllAsync(), GetByIdAsync(), CreateAsync(), UpdateAsync(), DeactivateAsync(), CreateDefaultsAsync()

        /// <summary>
        /// Lista as categorias do usuário.
        /// </summary>
        /// <param name="userId">Dono das categorias.</param>
        /// <param name="onlyActive">Quando verdadeiro, omite as excluídas.</param>
        /// <returns>Categorias do usuário.</returns>
        Task<IReadOnlyList<CategoryResponse>> GetAllAsync(Guid userId, bool onlyActive);

        /// <summary>
        /// Obtém uma categoria do usuário.
        /// </summary>
        /// <param name="userId">Dono da categoria.</param>
        /// <param name="categoryId">Identificador da categoria.</param>
        /// <returns>A categoria, ou nulo se não existir para esse usuário.</returns>
        Task<CategoryResponse?> GetByIdAsync(Guid userId, Guid categoryId);

        /// <summary>
        /// Cria uma categoria.
        /// </summary>
        /// <param name="userId">Dono da categoria.</param>
        /// <param name="request">Dados da categoria.</param>
        /// <returns>A categoria criada.</returns>
        /// <exception cref="Domain.Exceptions.BusinessRuleViolationException">Nome já usado no mesmo tipo.</exception>
        Task<CategoryResponse> CreateAsync(Guid userId, CategoryRequest request);

        /// <summary>
        /// Atualiza uma categoria.
        /// </summary>
        /// <param name="userId">Dono da categoria.</param>
        /// <param name="categoryId">Identificador da categoria.</param>
        /// <param name="request">Novos dados.</param>
        /// <returns>A categoria atualizada, ou nulo se não existir para esse usuário.</returns>
        /// <exception cref="Domain.Exceptions.BusinessRuleViolationException">Nome já usado ou troca de tipo com lançamentos.</exception>
        Task<CategoryResponse?> UpdateAsync(Guid userId, Guid categoryId, CategoryRequest request);

        /// <summary>
        /// Exclui logicamente uma categoria.
        /// </summary>
        /// <param name="userId">Dono da categoria.</param>
        /// <param name="categoryId">Identificador da categoria.</param>
        /// <returns>True se a categoria foi desativada; false se não existir para esse usuário.</returns>
        Task<bool> DeactivateAsync(Guid userId, Guid categoryId);

        /// <summary>
        /// Cria o conjunto inicial de categorias de uma conta nova.
        /// </summary>
        /// <param name="userId">Dono das categorias.</param>
        /// <returns>Quantidade de categorias criadas.</returns>
        Task<int> CreateDefaultsAsync(Guid userId);

        #endregion
    }
}
