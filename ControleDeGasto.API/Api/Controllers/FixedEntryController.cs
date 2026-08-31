using ControleDeGasto.API.Api.Filters;
using ControleDeGasto.API.Application.DTOs;
using ControleDeGasto.API.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ControleDeGasto.API.Api.Controllers
{
    [Route("api/fixed-entry")]
    public class FixedEntryController(
        IFixedEntryService service) : ApiControllerBase
    {
        #region Fields

        private readonly IFixedEntryService service = service;

        #endregion

        #region Actions :: HttpGet

        /// <summary>
        /// Lista os valores fixos mensais do usuário autenticado.
        /// </summary>
        /// <param name="includeInactive">Quando verdadeiro, inclui os pausados.</param>
        /// <returns>Valores fixos agrupados por natureza.</returns>
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<FixedEntryResponse>>> GetAll([FromQuery] bool includeInactive = false)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            return this.Ok(await this.service.GetAllAsync(userId, includeInactive));
        }

        /// <summary>
        /// Obtém um valor fixo do usuário autenticado.
        /// </summary>
        /// <param name="id">Identificador do valor fixo.</param>
        /// <returns>O valor fixo.</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<FixedEntryResponse>> GetById(Guid id)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            FixedEntryResponse? entry = await this.service.GetByIdAsync(userId, id);

            if (entry is null)
                return this.NotFound(new { Message = "Valor fixo não encontrado." });

            return this.Ok(entry);
        }

        #endregion

        #region Actions :: HttpPost, HttpPut, HttpPatch, HttpDelete

        /// <summary>
        /// Cria um valor fixo mensal.
        /// </summary>
        /// <param name="request">Dados do valor fixo.</param>
        /// <returns>O valor fixo criado.</returns>
        [HttpPost]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult<FixedEntryResponse>> Create(FixedEntryRequest request)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            FixedEntryResponse entry = await this.service.CreateAsync(userId, request);

            return this.CreatedAtAction(nameof(this.GetById), new { id = entry.Id }, entry);
        }

        /// <summary>
        /// Atualiza um valor fixo mensal.
        /// </summary>
        /// <param name="id">Identificador do valor fixo.</param>
        /// <param name="request">Novos dados.</param>
        /// <returns>O valor fixo atualizado.</returns>
        [HttpPut("{id:guid}")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult<FixedEntryResponse>> Update(Guid id, FixedEntryRequest request)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            FixedEntryResponse? entry = await this.service.UpdateAsync(userId, id, request);

            if (entry is null)
                return this.NotFound(new { Message = "Valor fixo não encontrado." });

            return this.Ok(entry);
        }

        /// <summary>
        /// Pausa ou retoma um valor fixo mensal.
        /// </summary>
        /// <param name="id">Identificador do valor fixo.</param>
        /// <param name="active">Verdadeiro para retomar; falso para pausar.</param>
        /// <returns>O valor fixo atualizado.</returns>
        [HttpPatch("{id:guid}/active")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult<FixedEntryResponse>> SetActive(Guid id, [FromQuery] bool active)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            FixedEntryResponse? entry = await this.service.SetActiveAsync(userId, id, active);

            if (entry is null)
                return this.NotFound(new { Message = "Valor fixo não encontrado." });

            return this.Ok(entry);
        }

        /// <summary>
        /// Remove um valor fixo mensal.
        /// </summary>
        /// <param name="id">Identificador do valor fixo.</param>
        /// <returns>Sem conteúdo em caso de sucesso.</returns>
        [HttpDelete("{id:guid}")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult> Delete(Guid id)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            bool deleted = await this.service.DeleteAsync(userId, id);

            if (!deleted)
                return this.NotFound(new { Message = "Valor fixo não encontrado." });

            return this.NoContent();
        }

        #endregion
    }
}
