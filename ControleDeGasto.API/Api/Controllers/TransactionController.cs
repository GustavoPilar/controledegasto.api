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
    }
}
