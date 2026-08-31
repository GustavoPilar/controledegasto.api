using ControleDeGasto.API.Application.DTOs;
using ControleDeGasto.API.Application.Interfaces;
using ControleDeGasto.API.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ControleDeGasto.API.Api.Controllers
{
    [Route("api/[Controller]")]
    public class DashboardController(
        IDashboardService service) : ApiControllerBase
    {
        #region Fields

        private readonly IDashboardService service = service;

        #endregion

        #region Actions :: HttpGet

        /// <summary>
        /// Monta o painel do período informado.
        /// </summary>
        /// <param name="from">Início do período. Nulo assume o mês atual.</param>
        /// <param name="to">Fim do período. Nulo assume o mês atual.</param>
        /// <returns>Indicadores, séries e listas do painel.</returns>
        [HttpGet]
        public async Task<ActionResult<DashboardResponse>> Get([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            if (from.HasValue && to.HasValue && from.Value > to.Value)
                return this.BadRequest(new { Message = "O início do período não pode ser depois do fim." });

            return this.Ok(await this.service.GetAsync(userId, from, to));
        }

        /// <summary>
        /// Detalha quanto cada categoria movimentou no período.
        /// </summary>
        /// <param name="type">Natureza a considerar.</param>
        /// <param name="from">Início do período. Nulo assume o mês atual.</param>
        /// <param name="to">Fim do período. Nulo assume o mês atual.</param>
        /// <returns>Categorias ordenadas pela que mais movimentou.</returns>
        [HttpGet("category-breakdown")]
        public async Task<ActionResult<IReadOnlyList<CategorySpendingResponse>>> GetCategoryBreakdown(
            [FromQuery] TransactionType type = TransactionType.Expense,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            if (!Enum.IsDefined(type))
                return this.BadRequest(new { Message = "Tipo inválido." });

            return this.Ok(await this.service.GetCategoryBreakdownAsync(userId, type, from, to));
        }

        /// <summary>
        /// Apura a situação da reserva de emergência.
        /// </summary>
        /// <returns>Saldo, meta e valor recomendado da reserva.</returns>
        [HttpGet("emergency-reserve")]
        public async Task<ActionResult<EmergencyReserveResponse>> GetEmergencyReserve()
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            return this.Ok(await this.service.GetEmergencyReserveAsync(userId));
        }

        /// <summary>
        /// Monta a previsão de entradas e saídas de um mês.
        /// </summary>
        /// <param name="reference">Qualquer data do mês desejado. Nulo assume o mês atual.</param>
        /// <returns>Previsão do mês com os itens que a compõem.</returns>
        [HttpGet("forecast")]
        public async Task<ActionResult<MonthlyForecastResponse>> GetForecast([FromQuery] DateTime? reference = null)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            return this.Ok(await this.service.GetForecastAsync(userId, reference));
        }

        #endregion
    }
}
