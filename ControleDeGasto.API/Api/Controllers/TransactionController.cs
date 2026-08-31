using ControleDeGasto.API.Api.Filters;
using ControleDeGasto.API.Application.DTOs;
using ControleDeGasto.API.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ControleDeGasto.API.Api.Controllers
{
    [Route("api/[Controller]")]
    public class TransactionController(
        ITransactionService service) : ApiControllerBase
    {
        #region Fields

        private readonly ITransactionService service = service;

        #endregion

        #region Actions :: HttpGet

        /// <summary>
        /// Lista os lançamentos do usuário autenticado.
        /// </summary>
        /// <param name="filter">Filtros de período, categoria, natureza, busca e paginação.</param>
        /// <returns>Página de lançamentos.</returns>
        [HttpGet]
        public async Task<ActionResult<PagedResponse<TransactionResponse>>> GetAll([FromQuery] TransactionFilterRequest filter)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            return this.Ok(await this.service.GetPagedAsync(userId, filter));
        }

        /// <summary>
        /// Obtém um lançamento do usuário autenticado.
        /// </summary>
        /// <param name="id">Identificador do lançamento.</param>
        /// <returns>O lançamento.</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<TransactionResponse>> GetById(Guid id)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            TransactionResponse? transaction = await this.service.GetByIdAsync(userId, id);

            if (transaction is null)
                return this.NotFound(new { Message = "Lançamento não encontrado." });

            return this.Ok(transaction);
        }

        /// <summary>
        /// Lista as divisões de compra que amigos atribuíram ao usuário autenticado.
        /// </summary>
        /// <param name="onlyOpen">Quando verdadeiro, traz apenas as ainda não acertadas.</param>
        /// <param name="page">Página solicitada, iniciando em 1.</param>
        /// <param name="pageSize">Itens por página.</param>
        /// <returns>Página de divisões.</returns>
        [HttpGet("shared-with-me")]
        public async Task<ActionResult<PagedResponse<SharedWithMeResponse>>> GetSharedWithMe(
            [FromQuery] bool onlyOpen = true,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            return this.Ok(await this.service.GetSharedWithMeAsync(userId, onlyOpen, page, pageSize));
        }

        /// <summary>
        /// Lista as compras parceladas do usuário autenticado.
        /// </summary>
        /// <param name="onlyOpen">Quando verdadeiro, traz apenas as que ainda têm parcela em aberto.</param>
        /// <returns>Compras parceladas com o andamento apurado.</returns>
        [HttpGet("installment-plan")]
        public async Task<ActionResult<IReadOnlyList<InstallmentPlanResponse>>> GetInstallmentPlans([FromQuery] bool onlyOpen = false)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            return this.Ok(await this.service.GetInstallmentPlansAsync(userId, onlyOpen));
        }

        /// <summary>
        /// Obtém uma compra parcelada do usuário autenticado.
        /// </summary>
        /// <param name="installmentPlanId">Identificador da compra.</param>
        /// <returns>A compra parcelada.</returns>
        [HttpGet("installment-plan/{installmentPlanId:guid}")]
        public async Task<ActionResult<InstallmentPlanResponse>> GetInstallmentPlanById(Guid installmentPlanId)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            InstallmentPlanResponse? plan = await this.service.GetInstallmentPlanByIdAsync(userId, installmentPlanId);

            if (plan is null)
                return this.NotFound(new { Message = "Compra parcelada não encontrada." });

            return this.Ok(plan);
        }

        #endregion

        #region Actions :: HttpPost, HttpPut, HttpDelete

        /// <summary>
        /// Registra um lançamento.
        /// </summary>
        /// <param name="request">Dados do lançamento.</param>
        /// <returns>O lançamento criado.</returns>
        [HttpPost]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult<TransactionResponse>> Create(TransactionRequest request)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            TransactionResponse transaction = await this.service.CreateAsync(userId, request);

            return this.CreatedAtAction(nameof(this.GetById), new { id = transaction.Id }, transaction);
        }

        /// <summary>
        /// Atualiza um lançamento.
        /// </summary>
        /// <param name="id">Identificador do lançamento.</param>
        /// <param name="request">Novos dados.</param>
        /// <returns>O lançamento atualizado.</returns>
        [HttpPut("{id:guid}")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult<TransactionResponse>> Update(Guid id, TransactionRequest request)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            TransactionResponse? transaction = await this.service.UpdateAsync(userId, id, request);

            if (transaction is null)
                return this.NotFound(new { Message = "Lançamento não encontrado." });

            return this.Ok(transaction);
        }

        /// <summary>
        /// Remove um lançamento.
        /// </summary>
        /// <param name="id">Identificador do lançamento.</param>
        /// <returns>Sem conteúdo em caso de sucesso.</returns>
        [HttpDelete("{id:guid}")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult> Delete(Guid id)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            bool deleted = await this.service.DeleteAsync(userId, id);

            if (!deleted)
                return this.NotFound(new { Message = "Lançamento não encontrado." });

            return this.NoContent();
        }

        #endregion

        #region Actions :: HttpPatch

        /// <summary>
        /// Liquida ou reabre um lançamento previsto.
        /// </summary>
        /// <param name="id">Identificador do lançamento.</param>
        /// <param name="request">Situação desejada e data da liquidação.</param>
        /// <returns>O lançamento atualizado.</returns>
        [HttpPatch("{id:guid}/settle")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult<TransactionResponse>> Settle(Guid id, TransactionSettleRequest request)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            TransactionResponse? transaction = await this.service.SettleAsync(userId, id, request);

            if (transaction is null)
                return this.NotFound(new { Message = "Lançamento não encontrado." });

            return this.Ok(transaction);
        }

        /// <summary>
        /// Marca uma divisão de compra como acertada, ou a reabre.
        /// </summary>
        /// <param name="shareId">Identificador da divisão.</param>
        /// <param name="settled">Verdadeiro para acertar; falso para reabrir.</param>
        /// <returns>A divisão atualizada.</returns>
        [HttpPatch("share/{shareId:guid}/settle")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult<TransactionShareResponse>> SettleShare(Guid shareId, [FromQuery] bool settled = true)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            TransactionShareResponse? share = await this.service.SettleShareAsync(userId, shareId, settled);

            if (share is null)
                return this.NotFound(new { Message = "Divisão não encontrada." });

            return this.Ok(share);
        }

        #endregion

        #region Actions :: HttpPost, HttpDelete :: InstallmentPlan

        /// <summary>
        /// Registra uma compra parcelada, gerando as parcelas como lançamentos previstos.
        /// </summary>
        /// <param name="request">Dados da compra.</param>
        /// <returns>A compra criada.</returns>
        [HttpPost("installment-plan")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult<InstallmentPlanResponse>> CreateInstallmentPlan(InstallmentPlanRequest request)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            InstallmentPlanResponse plan = await this.service.CreateInstallmentPlanAsync(userId, request);

            return this.CreatedAtAction(nameof(this.GetInstallmentPlanById), new { installmentPlanId = plan.Id }, plan);
        }

        /// <summary>
        /// Cancela uma compra parcelada que ainda não tem parcela paga.
        /// </summary>
        /// <param name="installmentPlanId">Identificador da compra.</param>
        /// <returns>Sem conteúdo em caso de sucesso.</returns>
        [HttpDelete("installment-plan/{installmentPlanId:guid}")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult> DeleteInstallmentPlan(Guid installmentPlanId)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            bool deleted = await this.service.DeleteInstallmentPlanAsync(userId, installmentPlanId);

            if (!deleted)
                return this.NotFound(new { Message = "Compra parcelada não encontrada." });

            return this.NoContent();
        }

        #endregion
    }
}
