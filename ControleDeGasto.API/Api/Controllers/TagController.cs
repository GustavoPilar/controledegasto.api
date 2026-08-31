using ControleDeGasto.API.Api.Filters;
using ControleDeGasto.API.Application.DTOs;
using ControleDeGasto.API.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ControleDeGasto.API.Api.Controllers
{
    [Route("api/[Controller]")]
    public class TagController(
        ITagService service) : ApiControllerBase
    {
        #region Fields

        private readonly ITagService service = service;

        #endregion

        #region Actions :: HttpGet

        /// <summary>
        /// Lista as etiquetas do usuário autenticado.
        /// </summary>
        /// <returns>Etiquetas com a quantidade de lançamentos marcados.</returns>
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<TagResponse>>> GetAll()
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            return this.Ok(await this.service.GetAllAsync(userId));
        }

        /// <summary>
        /// Soma, por etiqueta, o que foi movimentado no período.
        /// </summary>
        /// <param name="from">Início do período. Nulo assume o mês atual.</param>
        /// <param name="to">Fim do período. Nulo assume o mês atual.</param>
        /// <returns>Totais por etiqueta.</returns>
        [HttpGet("total")]
        public async Task<ActionResult<IReadOnlyList<TagTotalResponse>>> GetTotals(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            if (from.HasValue && to.HasValue && from.Value > to.Value)
                return this.BadRequest(new { Message = "O início do período não pode ser depois do fim." });

            return this.Ok(await this.service.GetTotalsAsync(userId, from, to));
        }

        #endregion

        #region Actions :: HttpPost, HttpPut, HttpDelete

        /// <summary>
        /// Cria uma etiqueta.
        /// </summary>
        /// <param name="request">Dados da etiqueta.</param>
        /// <returns>A etiqueta criada.</returns>
        [HttpPost]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult<TagResponse>> Create(TagRequest request)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            return this.Ok(await this.service.CreateAsync(userId, request));
        }

        /// <summary>
        /// Atualiza uma etiqueta.
        /// </summary>
        /// <param name="id">Identificador da etiqueta.</param>
        /// <param name="request">Novos dados.</param>
        /// <returns>A etiqueta atualizada.</returns>
        [HttpPut("{id:guid}")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult<TagResponse>> Update(Guid id, TagRequest request)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            TagResponse? tag = await this.service.UpdateAsync(userId, id, request);

            if (tag is null)
                return this.NotFound(new { Message = "Etiqueta não encontrada." });

            return this.Ok(tag);
        }

        /// <summary>
        /// Remove uma etiqueta e a desmarca dos lançamentos.
        /// </summary>
        /// <param name="id">Identificador da etiqueta.</param>
        /// <returns>Sem conteúdo em caso de sucesso.</returns>
        [HttpDelete("{id:guid}")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult> Delete(Guid id)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            bool deleted = await this.service.DeleteAsync(userId, id);

            if (!deleted)
                return this.NotFound(new { Message = "Etiqueta não encontrada." });

            return this.NoContent();
        }

        #endregion
    }
}
