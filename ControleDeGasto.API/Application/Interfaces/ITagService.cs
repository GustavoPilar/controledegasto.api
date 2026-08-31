using ControleDeGasto.API.Application.DTOs;

namespace ControleDeGasto.API.Application.Interfaces
{
    /// <summary>
    /// Regras das etiquetas de lançamento.
    /// </summary>
    public interface ITagService
    {
        #region Methods :: GetAllAsync(), CreateAsync(), UpdateAsync(), DeleteAsync(), GetTotalsAsync()

        /// <summary>
        /// Lista as etiquetas do usuário com a contagem de uso.
        /// </summary>
        /// <param name="userId">Dono das etiquetas.</param>
        /// <returns>Etiquetas em ordem alfabética.</returns>
        Task<IReadOnlyList<TagResponse>> GetAllAsync(Guid userId);

        /// <summary>
        /// Cria uma etiqueta.
        /// </summary>
        /// <param name="userId">Dono da etiqueta.</param>
        /// <param name="request">Dados da etiqueta.</param>
        /// <returns>A etiqueta criada.</returns>
        /// <exception cref="Domain.Exceptions.BusinessRuleViolationException">Nome já usado.</exception>
        Task<TagResponse> CreateAsync(Guid userId, TagRequest request);

        /// <summary>
        /// Atualiza uma etiqueta.
        /// </summary>
        /// <param name="userId">Dono da etiqueta.</param>
        /// <param name="tagId">Identificador da etiqueta.</param>
        /// <param name="request">Novos dados.</param>
        /// <returns>A etiqueta atualizada, ou nulo se não existir para esse usuário.</returns>
        /// <exception cref="Domain.Exceptions.BusinessRuleViolationException">Nome já usado.</exception>
        Task<TagResponse?> UpdateAsync(Guid userId, Guid tagId, TagRequest request);

        /// <summary>
        /// Remove uma etiqueta e a desmarca dos lançamentos.
        /// </summary>
        /// <param name="userId">Dono da etiqueta.</param>
        /// <param name="tagId">Identificador da etiqueta.</param>
        /// <returns>True se removeu; false se não existir para esse usuário.</returns>
        Task<bool> DeleteAsync(Guid userId, Guid tagId);

        /// <summary>
        /// Soma, por etiqueta, o que foi movimentado em um período.
        /// </summary>
        /// <param name="userId">Dono dos lançamentos.</param>
        /// <param name="from">Início do período. Nulo assume o mês atual.</param>
        /// <param name="to">Fim do período. Nulo assume o mês atual.</param>
        /// <returns>Totais por etiqueta, da que mais movimentou para a que menos movimentou.</returns>
        Task<IReadOnlyList<TagTotalResponse>> GetTotalsAsync(Guid userId, DateTime? from, DateTime? to);

        #endregion
    }
}
