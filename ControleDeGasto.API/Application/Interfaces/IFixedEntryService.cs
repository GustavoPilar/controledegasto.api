using ControleDeGasto.API.Application.DTOs;

namespace ControleDeGasto.API.Application.Interfaces
{
    /// <summary>
    /// Regras dos valores fixos mensais (entradas, saídas e créditos de benefício).
    /// </summary>
    public interface IFixedEntryService
    {
        #region Methods :: GetAllAsync(), GetByIdAsync(), CreateAsync(), UpdateAsync(), SetActiveAsync(), DeleteAsync()

        /// <summary>
        /// Lista os valores fixos do usuário.
        /// </summary>
        /// <param name="userId">Dono das definições.</param>
        /// <param name="includeInactive">Quando verdadeiro, inclui as pausadas.</param>
        /// <returns>Definições agrupadas por natureza.</returns>
        Task<IReadOnlyList<FixedEntryResponse>> GetAllAsync(Guid userId, bool includeInactive);

        /// <summary>
        /// Obtém um valor fixo do usuário.
        /// </summary>
        /// <param name="userId">Dono da definição.</param>
        /// <param name="fixedEntryId">Identificador da definição.</param>
        /// <returns>A definição, ou nulo se não existir para esse usuário.</returns>
        Task<FixedEntryResponse?> GetByIdAsync(Guid userId, Guid fixedEntryId);

        /// <summary>
        /// Cria um valor fixo.
        /// </summary>
        /// <param name="userId">Dono da definição.</param>
        /// <param name="request">Dados da definição.</param>
        /// <returns>A definição criada.</returns>
        /// <exception cref="Domain.Exceptions.BusinessRuleViolationException">Categoria ou carteira inválidas, descrição repetida ou vigência incoerente.</exception>
        Task<FixedEntryResponse> CreateAsync(Guid userId, FixedEntryRequest request);

        /// <summary>
        /// Atualiza um valor fixo.
        /// </summary>
        /// <param name="userId">Dono da definição.</param>
        /// <param name="fixedEntryId">Identificador da definição.</param>
        /// <param name="request">Novos dados.</param>
        /// <returns>A definição atualizada, ou nulo se não existir para esse usuário.</returns>
        /// <exception cref="Domain.Exceptions.BusinessRuleViolationException">Categoria ou carteira inválidas, descrição repetida ou vigência incoerente.</exception>
        Task<FixedEntryResponse?> UpdateAsync(Guid userId, Guid fixedEntryId, FixedEntryRequest request);

        /// <summary>
        /// Pausa ou retoma um valor fixo.
        /// </summary>
        /// <remarks>
        /// Pausar em vez de excluir preserva o histórico: o total já creditado em uma carteira de
        /// benefício é reconstruído a partir da definição, e apagá-la mudaria o saldo passado.
        /// </remarks>
        /// <param name="userId">Dono da definição.</param>
        /// <param name="fixedEntryId">Identificador da definição.</param>
        /// <param name="active">Verdadeiro para retomar; falso para pausar.</param>
        /// <returns>A definição atualizada, ou nulo se não existir para esse usuário.</returns>
        Task<FixedEntryResponse?> SetActiveAsync(Guid userId, Guid fixedEntryId, bool active);

        /// <summary>
        /// Remove um valor fixo.
        /// </summary>
        /// <param name="userId">Dono da definição.</param>
        /// <param name="fixedEntryId">Identificador da definição.</param>
        /// <returns>True se removeu; false se não existir para esse usuário.</returns>
        Task<bool> DeleteAsync(Guid userId, Guid fixedEntryId);

        #endregion
    }
}
