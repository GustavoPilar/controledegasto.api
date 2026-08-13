using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;
using ControleDeGasto.API.Domain.Queries;
using ControleDeGasto.API.Domain.ReadModels;

namespace ControleDeGasto.API.Domain.Interfaces
{
    /// <summary>
    /// Acesso a lançamentos. Todo método recebe o dono e filtra por ele.
    /// </summary>
    public interface ITransactionRepository
    {
        #region Methods :: GetPagedAsync(), GetByIdAsync(), GetRecentAsync(), CreateAsync(), UpdateAsync(), DeleteAsync()

        /// <summary>
        /// Lista lançamentos aplicando filtros e paginação.
        /// </summary>
        /// <param name="query">Filtros da consulta.</param>
        /// <returns>Página de lançamentos e total de registros filtrados.</returns>
        Task<PagedResult<Transaction>> GetPagedAsync(TransactionQuery query);

        /// <summary>
        /// Obtém um lançamento do usuário.
        /// </summary>
        /// <param name="userId">Dono do lançamento.</param>
        /// <param name="transactionId">Identificador do lançamento.</param>
        /// <returns>O lançamento, ou nulo se não existir ou não pertencer ao usuário.</returns>
        Task<Transaction?> GetByIdAsync(Guid userId, Guid transactionId);

        /// <summary>
        /// Lista os lançamentos mais recentes do usuário.
        /// </summary>
        /// <param name="userId">Dono dos lançamentos.</param>
        /// <param name="count">Quantidade máxima de itens.</param>
        /// <returns>Lançamentos ordenados do mais recente para o mais antigo.</returns>
        Task<IReadOnlyList<Transaction>> GetRecentAsync(Guid userId, int count);

        /// <summary>
        /// Persiste um lançamento novo.
        /// </summary>
        /// <param name="transaction">Lançamento a gravar.</param>
        /// <returns>True se a gravação afetou alguma linha.</returns>
        Task<bool> CreateAsync(Transaction transaction);

        /// <summary>
        /// Persiste alterações de um lançamento.
        /// </summary>
        /// <param name="transaction">Lançamento alterado.</param>
        /// <returns>True se a gravação afetou alguma linha.</returns>
        Task<bool> UpdateAsync(Transaction transaction);

        /// <summary>
        /// Remove um lançamento.
        /// </summary>
        /// <param name="transaction">Lançamento a remover.</param>
        /// <returns>True se a remoção afetou alguma linha.</returns>
        Task<bool> DeleteAsync(Transaction transaction);

        #endregion

        #region Methods :: GetTotalsByTypeAsync(), GetTotalsByCategoryAsync(), GetMonthlyTotalsAsync()

        /// <summary>
        /// Soma os lançamentos do período agrupados por natureza.
        /// </summary>
        /// <param name="userId">Dono dos lançamentos.</param>
        /// <param name="from">Início do período, em UTC.</param>
        /// <param name="to">Fim do período, em UTC.</param>
        /// <returns>Um total por natureza presente no período.</returns>
        Task<IReadOnlyList<TypeTotal>> GetTotalsByTypeAsync(Guid userId, DateTime from, DateTime to);

        /// <summary>
        /// Soma os lançamentos do período agrupados por categoria.
        /// </summary>
        /// <param name="userId">Dono dos lançamentos.</param>
        /// <param name="type">Natureza a considerar.</param>
        /// <param name="from">Início do período, em UTC.</param>
        /// <param name="to">Fim do período, em UTC.</param>
        /// <param name="limit">Quantidade máxima de categorias, das que mais movimentaram. Nulo traz todas.</param>
        /// <returns>Totais por categoria, do maior para o menor.</returns>
        Task<IReadOnlyList<CategoryTotal>> GetTotalsByCategoryAsync(Guid userId, TransactionType type, DateTime from, DateTime to, int? limit);

        /// <summary>
        /// Soma os lançamentos agrupados por ano, mês e natureza.
        /// </summary>
        /// <param name="userId">Dono dos lançamentos.</param>
        /// <param name="from">Início do período, em UTC.</param>
        /// <param name="to">Fim do período, em UTC.</param>
        /// <returns>Totais mensais em ordem cronológica.</returns>
        Task<IReadOnlyList<MonthlyTotal>> GetMonthlyTotalsAsync(Guid userId, DateTime from, DateTime to);

        #endregion
    }
}
