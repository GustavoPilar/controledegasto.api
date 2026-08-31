using ControleDeGasto.API.Api.Filters;
using ControleDeGasto.API.Application.DTOs;
using ControleDeGasto.API.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ControleDeGasto.API.Api.Controllers
{
    [Route("api/[Controller]")]
    public class FriendController(
        IFriendshipService service) : ApiControllerBase
    {
        #region Fields

        private readonly IFriendshipService service = service;

        #endregion

        #region Actions :: HttpGet

        /// <summary>
        /// Lista os amigos do usuário autenticado.
        /// </summary>
        /// <returns>Amigos com o saldo de divisões em aberto.</returns>
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<FriendResponse>>> GetAll()
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            return this.Ok(await this.service.GetFriendsAsync(userId));
        }

        /// <summary>
        /// Lista os convites pendentes, recebidos e enviados.
        /// </summary>
        /// <returns>Convites pendentes.</returns>
        [HttpGet("pending")]
        public async Task<ActionResult<IReadOnlyList<FriendResponse>>> GetPending()
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            return this.Ok(await this.service.GetPendingAsync(userId));
        }

        /// <summary>
        /// Procura usuários para convidar.
        /// </summary>
        /// <param name="term">Trecho do apelido, ou e-mail exato.</param>
        /// <returns>Usuários encontrados com a situação da relação.</returns>
        [HttpGet("search")]
        public async Task<ActionResult<IReadOnlyList<UserSearchResponse>>> Search([FromQuery] string term)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            return this.Ok(await this.service.SearchUsersAsync(userId, term));
        }

        #endregion

        #region Actions :: HttpPost, HttpPatch, HttpDelete

        /// <summary>
        /// Envia um convite de amizade.
        /// </summary>
        /// <param name="request">Usuário a convidar.</param>
        /// <returns>A relação resultante.</returns>
        [HttpPost]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult<FriendResponse>> Invite(FriendInviteRequest request)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            return this.Ok(await this.service.InviteAsync(userId, request));
        }

        /// <summary>
        /// Aceita ou recusa um convite recebido.
        /// </summary>
        /// <param name="id">Identificador da relação.</param>
        /// <param name="accept">Verdadeiro para aceitar; falso para recusar.</param>
        /// <returns>A relação atualizada.</returns>
        [HttpPatch("{id:guid}/respond")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult<FriendResponse>> Respond(Guid id, [FromQuery] bool accept)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            FriendResponse? friendship = await this.service.RespondAsync(userId, id, accept);

            if (friendship is null)
                return this.NotFound(new { Message = "Convite não encontrado." });

            return this.Ok(friendship);
        }

        /// <summary>
        /// Bloqueia um usuário.
        /// </summary>
        /// <param name="targetUserId">Usuário a bloquear.</param>
        /// <returns>A relação bloqueada.</returns>
        [HttpPost("block/{targetUserId:guid}")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult<FriendResponse>> Block(Guid targetUserId)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            return this.Ok(await this.service.BlockAsync(userId, targetUserId));
        }

        /// <summary>
        /// Desfaz um bloqueio aplicado pelo próprio usuário.
        /// </summary>
        /// <param name="id">Identificador da relação.</param>
        /// <returns>Sem conteúdo em caso de sucesso.</returns>
        [HttpDelete("{id:guid}/block")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult> Unblock(Guid id)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            bool unblocked = await this.service.UnblockAsync(userId, id);

            if (!unblocked)
                return this.NotFound(new { Message = "Bloqueio não encontrado." });

            return this.NoContent();
        }

        /// <summary>
        /// Desfaz uma amizade ou cancela um convite enviado.
        /// </summary>
        /// <param name="id">Identificador da relação.</param>
        /// <returns>Sem conteúdo em caso de sucesso.</returns>
        [HttpDelete("{id:guid}")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult> Remove(Guid id)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            bool removed = await this.service.RemoveAsync(userId, id);

            if (!removed)
                return this.NotFound(new { Message = "Amizade não encontrada." });

            return this.NoContent();
        }

        #endregion
    }
}
