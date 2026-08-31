using ControleDeGasto.API.Application.DTOs;
using ControleDeGasto.API.Domain.Enums;

namespace ControleDeGasto.API.Application.Interfaces
{
    /// <summary>
    /// Composição dos indicadores do painel.
    /// </summary>
    public interface IDashboardService
    {
        #region Methods :: GetAsync(), GetCategoryBreakdownAsync(), GetEmergencyReserveAsync()

        /// <summary>
        /// Monta o painel do período.
        /// </summary>
        /// <param name="userId">Dono dos dados.</param>
        /// <param name="from">Início do período. Nulo assume o primeiro dia do mês atual.</param>
        /// <param name="to">Fim do período. Nulo assume o último dia do mês atual.</param>
        /// <returns>Indicadores, séries e listas do painel.</returns>
        Task<DashboardResponse> GetAsync(Guid userId, DateTime? from, DateTime? to);

        /// <summary>
        /// Detalha o quanto cada categoria movimentou no período.
        /// </summary>
        /// <param name="userId">Dono dos dados.</param>
        /// <param name="type">Natureza a considerar.</param>
        /// <param name="from">Início do período. Nulo assume o primeiro dia do mês atual.</param>
        /// <param name="to">Fim do período. Nulo assume o último dia do mês atual.</param>
        /// <returns>Categorias ordenadas da que mais movimentou para a que menos movimentou.</returns>
        Task<IReadOnlyList<CategorySpendingResponse>> GetCategoryBreakdownAsync(Guid userId, TransactionType type, DateTime? from, DateTime? to);

        /// <summary>
        /// Apura a situação da reserva de emergência, incluindo o valor recomendado.
        /// </summary>
        /// <param name="userId">Dono da reserva.</param>
        /// <returns>Situação da reserva.</returns>
        Task<EmergencyReserveResponse> GetEmergencyReserveAsync(Guid userId);

        #endregion

        #region Methods :: GetForecastAsync()

        /// <summary>
        /// Monta a previsão de entradas e saídas de um mês.
        /// </summary>
        /// <remarks>
        /// Soma o que já foi liquidado, o que está lançado como previsto e o que as definições
        /// fixas dizem que ainda vai acontecer, abatendo dos fixos o que já apareceu como
        /// lançamento no mês para nada ser contado duas vezes.
        /// </remarks>
        /// <param name="userId">Dono dos dados.</param>
        /// <param name="reference">Qualquer data do mês desejado. Nulo assume o mês atual.</param>
        /// <returns>Previsão do mês com os itens que a compõem.</returns>
        Task<MonthlyForecastResponse> GetForecastAsync(Guid userId, DateTime? reference);

        #endregion
    }
}
