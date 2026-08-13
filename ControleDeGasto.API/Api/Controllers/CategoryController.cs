using ControleDeGasto.API.Api.Filters;
using ControleDeGasto.API.Application.DTOs;
using ControleDeGasto.API.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ControleDeGasto.API.Api.Controllers
{
    [Route("api/[Controller]")]
    public class CategoryController(
        ICategoryService service) : ApiControllerBase
    {
        #region Fields

        private readonly ICategoryService service = service;

        #endregion

        #region Actions :: HttpGet

        /// <summary>
        /// Lista as categorias do usuário autenticado.
        /// </summary>
        /// <param name="onlyActive">Quando verdadeiro, omite as categorias excluídas.</param>
        /// <returns>Categorias do usuário.</returns>
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> GetAll([FromQuery] bool onlyActive = true)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            return this.Ok(await this.service.GetAllAsync(userId, onlyActive));
        }

        /// <summary>
        /// Obtém uma categoria do usuário autenticado.
        /// </summary>
        /// <param name="id">Identificador da categoria.</param>
        /// <returns>A categoria.</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<CategoryResponse>> GetById(Guid id)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            CategoryResponse? category = await this.service.GetByIdAsync(userId, id);

            if (category is null)
                return this.NotFound(new { Message = "Categoria não encontrada." });

            return this.Ok(category);
        }

        #endregion

        #region Actions :: HttpPost, HttpPut, HttpDelete

        /// <summary>
        /// Cria uma categoria.
        /// </summary>
        /// <param name="request">Dados da categoria.</param>
        /// <returns>A categoria criada.</returns>
        [HttpPost]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult<CategoryResponse>> Create(CategoryRequest request)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            CategoryResponse category = await this.service.CreateAsync(userId, request);

            return this.CreatedAtAction(nameof(this.GetById), new { id = category.Id }, category);
        }

        /// <summary>
        /// Atualiza uma categoria.
        /// </summary>
        /// <param name="id">Identificador da categoria.</param>
        /// <param name="request">Novos dados.</param>
        /// <returns>A categoria atualizada.</returns>
        [HttpPut("{id:guid}")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult<CategoryResponse>> Update(Guid id, CategoryRequest request)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            CategoryResponse? category = await this.service.UpdateAsync(userId, id, request);

            if (category is null)
                return this.NotFound(new { Message = "Categoria não encontrada." });

            return this.Ok(category);
        }

        /// <summary>
        /// Exclui logicamente uma categoria, preservando o histórico de lançamentos.
        /// </summary>
        /// <param name="id">Identificador da categoria.</param>
        /// <returns>Sem conteúdo em caso de sucesso.</returns>
        [HttpDelete("{id:guid}")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult> Delete(Guid id)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            bool deleted = await this.service.DeactivateAsync(userId, id);

            if (!deleted)
                return this.NotFound(new { Message = "Categoria não encontrada." });

            return this.NoContent();
        }

        #endregion
    }
}
