using ControleDeGasto.API.Api.Filters;
using ControleDeGasto.API.Application.DTOs;
using ControleDeGasto.API.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ControleDeGasto.API.Api.Controllers
{
    [Route("api/[Controller]")]
    public class WalletController(
        IWalletService service) : ApiControllerBase
    {
        #region Constants :: DEFAULT_TRANSFERS_LIMIT, MAX_TRANSFERS_LIMIT

        private const int DEFAULT_TRANSFERS_LIMIT = 20;
        private const int MAX_TRANSFERS_LIMIT = 100;

        #endregion

        #region Fields

        private readonly IWalletService service = service;

        #endregion

        #region Actions :: HttpGet

        /// <summary>
        /// Lista as carteiras do usuário autenticado.
        /// </summary>
        /// <param name="includeInactive">Quando verdadeiro, inclui as excluídas.</param>
        /// <returns>Carteiras com saldo apurado.</returns>
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<WalletResponse>>> GetAll([FromQuery] bool includeInactive = false)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            return this.Ok(await this.service.GetAllAsync(userId, includeInactive));
        }

        /// <summary>
        /// Obtém uma carteira do usuário autenticado.
        /// </summary>
        /// <param name="id">Identificador da carteira.</param>
        /// <returns>A carteira.</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<WalletResponse>> GetById(Guid id)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            WalletResponse? wallet = await this.service.GetByIdAsync(userId, id);

            if (wallet is null)
                return this.NotFound(new { Message = "Carteira não encontrada." });

            return this.Ok(wallet);
        }

        /// <summary>
        /// Lista as transferências entre carteiras.
        /// </summary>
        /// <param name="walletId">Carteira envolvida. Nulo traz todas.</param>
        /// <param name="limit">Quantidade máxima de itens.</param>
        /// <returns>Transferências da mais recente para a mais antiga.</returns>
        [HttpGet("transfer")]
        public async Task<ActionResult<IReadOnlyList<WalletTransferResponse>>> GetTransfers(
            [FromQuery] Guid? walletId = null,
            [FromQuery] int limit = DEFAULT_TRANSFERS_LIMIT)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            int safeLimit = limit is < 1 or > MAX_TRANSFERS_LIMIT ? DEFAULT_TRANSFERS_LIMIT : limit;

            return this.Ok(await this.service.GetTransfersAsync(userId, walletId, safeLimit));
        }

        #endregion

        #region Actions :: HttpPost, HttpPut, HttpDelete

        /// <summary>
        /// Cria uma carteira.
        /// </summary>
        /// <param name="request">Dados da carteira.</param>
        /// <returns>A carteira criada.</returns>
        [HttpPost]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult<WalletResponse>> Create(WalletRequest request)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            WalletResponse wallet = await this.service.CreateAsync(userId, request);

            return this.CreatedAtAction(nameof(this.GetById), new { id = wallet.Id }, wallet);
        }

        /// <summary>
        /// Registra uma transferência entre carteiras.
        /// </summary>
        /// <param name="request">Dados da transferência.</param>
        /// <returns>A transferência criada.</returns>
        [HttpPost("transfer")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult<WalletTransferResponse>> CreateTransfer(WalletTransferRequest request)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            return this.Ok(await this.service.CreateTransferAsync(userId, request));
        }

        /// <summary>
        /// Atualiza uma carteira.
        /// </summary>
        /// <param name="id">Identificador da carteira.</param>
        /// <param name="request">Novos dados.</param>
        /// <returns>A carteira atualizada.</returns>
        [HttpPut("{id:guid}")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult<WalletResponse>> Update(Guid id, WalletRequest request)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            WalletResponse? wallet = await this.service.UpdateAsync(userId, id, request);

            if (wallet is null)
                return this.NotFound(new { Message = "Carteira não encontrada." });

            return this.Ok(wallet);
        }

        /// <summary>
        /// Exclui uma carteira.
        /// </summary>
        /// <param name="id">Identificador da carteira.</param>
        /// <returns>Sem conteúdo em caso de sucesso.</returns>
        [HttpDelete("{id:guid}")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult> Delete(Guid id)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            bool deleted = await this.service.DeleteAsync(userId, id);

            if (!deleted)
                return this.NotFound(new { Message = "Carteira não encontrada." });

            return this.NoContent();
        }

        /// <summary>
        /// Remove uma transferência.
        /// </summary>
        /// <param name="transferId">Identificador da transferência.</param>
        /// <returns>Sem conteúdo em caso de sucesso.</returns>
        [HttpDelete("transfer/{transferId:guid}")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult> DeleteTransfer(Guid transferId)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            bool deleted = await this.service.DeleteTransferAsync(userId, transferId);

            if (!deleted)
                return this.NotFound(new { Message = "Transferência não encontrada." });

            return this.NoContent();
        }

        #endregion
    }
}
