using ControleDeGasto.API.Application.DTOs;

namespace ControleDeGasto.API.Application.Interfaces
{
    /// <summary>
    /// Regras de lançamento financeiro.
    /// </summary>
    public interface ITransactionService
    {
        #region Methods :: GetPagedAsync(), GetByIdAsync(), CreateAsync(), UpdateAsync(), DeleteAsync()

        /// <summary>
        /// Lista lançamentos do usuário aplicando filtros e paginação.
        /// </summary>
        /// <param name="userId">Dono dos lançamentos.</param>
        /// <param name="filter">Filtros da listagem.</param>
        /// <returns>Página de lançamentos.</returns>
        Task<PagedResponse<TransactionResponse>> GetPagedAsync(Guid userId, TransactionFilterRequest filter);

        /// <summary>
        /// Obtém um lançamento do usuário.
        /// </summary>
        /// <param name="userId">Dono do lançamento.</param>
        /// <param name="transactionId">Identificador do lançamento.</param>
        /// <returns>O lançamento, ou nulo se não existir para esse usuário.</returns>
        Task<TransactionResponse?> GetByIdAsync(Guid userId, Guid transactionId);

        /// <summary>
        /// Registra um lançamento.
        /// </summary>
        /// <param name="userId">Dono do lançamento.</param>
        /// <param name="request">Dados do lançamento.</param>
        /// <returns>O lançamento criado.</returns>
        /// <exception cref="Domain.Exceptions.BusinessRuleViolationException">Categoria inexistente, de outro usuário ou inativa.</exception>
        Task<TransactionResponse> CreateAsync(Guid userId, TransactionRequest request);

        /// <summary>
        /// Atualiza um lançamento.
        /// </summary>
        /// <param name="userId">Dono do lançamento.</param>
        /// <param name="transactionId">Identificador do lançamento.</param>
        /// <param name="request">Novos dados.</param>
        /// <returns>O lançamento atualizado, ou nulo se não existir para esse usuário.</returns>
        /// <exception cref="Domain.Exceptions.BusinessRuleViolationException">Categoria inexistente, de outro usuário ou inativa.</exception>
        Task<TransactionResponse?> UpdateAsync(Guid userId, Guid transactionId, TransactionRequest request);

        /// <summary>
        /// Remove um lançamento.
        /// </summary>
        /// <param name="userId">Dono do lançamento.</param>
        /// <param name="transactionId">Identificador do lançamento.</param>
        /// <returns>True se removeu; false se não existir para esse usuário.</returns>
        Task<bool> DeleteAsync(Guid userId, Guid transactionId);

        #endregion
    }
}
