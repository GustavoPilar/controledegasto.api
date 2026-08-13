using ControleDeGasto.API.Api.Filters;
using ControleDeGasto.API.Application.DTOs;
using ControleDeGasto.API.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ControleDeGasto.API.Api.Controllers
{
    [Route("api/[Controller]")]
    public class NotificationController(
        INotificationService service) : ApiControllerBase
    {
        #region Constants :: DEFAULT_PAGE, DEFAULT_PAGE_SIZE, MAX_PAGE_SIZE

        private const int DEFAULT_PAGE = 1;
        private const int DEFAULT_PAGE_SIZE = 20;
        private const int MAX_PAGE_SIZE = 50;

        #endregion

        #region Fields

        private readonly INotificationService service = service;

        #endregion

        #region Actions :: HttpGet

        /// <summary>
        /// Lista as notificações do usuário autenticado.
        /// </summary>
        /// <param name="onlyUnread">Quando verdadeiro, traz apenas as não lidas.</param>
        /// <param name="page">Página solicitada.</param>
        /// <param name="pageSize">Itens por página.</param>
        /// <returns>Página de notificações.</returns>
        [HttpGet]
        public async Task<ActionResult<PagedResponse<NotificationResponse>>> GetAll(
            [FromQuery] bool onlyUnread = false,
            [FromQuery] int page = DEFAULT_PAGE,
            [FromQuery] int pageSize = DEFAULT_PAGE_SIZE)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            int safePage = page < 1 ? DEFAULT_PAGE : page;
            int safePageSize = pageSize is < 1 or > MAX_PAGE_SIZE ? DEFAULT_PAGE_SIZE : pageSize;

            return this.Ok(await this.service.GetPagedAsync(userId, onlyUnread, safePage, safePageSize));
        }

        /// <summary>
        /// Conta as notificações não lidas, para o indicador da interface.
        /// </summary>
        /// <returns>Quantidade de não lidas.</returns>
        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> GetUnreadCount()
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            return this.Ok(await this.service.GetUnreadCountAsync(userId));
        }

        #endregion

        #region Actions :: HttpPatch

        /// <summary>
        /// Marca uma notificação como lida.
        /// </summary>
        /// <param name="id">Identificador da notificação.</param>
        /// <returns>Sem conteúdo em caso de sucesso.</returns>
        [HttpPatch("{id:guid}/read")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult> MarkAsRead(Guid id)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            bool updated = await this.service.MarkAsReadAsync(userId, id);

            if (!updated)
                return this.NotFound(new { Message = "Notificação não encontrada ou já lida." });

            return this.NoContent();
        }

        /// <summary>
        /// Marca todas as notificações do usuário como lidas.
        /// </summary>
        /// <returns>Quantidade de notificações marcadas.</returns>
        [HttpPatch("read-all")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult<int>> MarkAllAsRead()
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            return this.Ok(await this.service.MarkAllAsReadAsync(userId));
        }

        #endregion
    }
}
