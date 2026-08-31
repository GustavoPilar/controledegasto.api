using ControleDeGasto.API.Api.Filters;
using ControleDeGasto.API.Application.DTOs;
using ControleDeGasto.API.Application.Interfaces;
using ControleDeGasto.API.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ControleDeGasto.API.Api.Controllers
{
    [Route("api/savings-goal")]
    public class SavingsGoalController(
        ISavingsGoalService service) : ApiControllerBase
    {
        #region Constants :: DEFAULT_CONTRIBUTIONS_LIMIT, MAX_CONTRIBUTIONS_LIMIT

        private const int DEFAULT_CONTRIBUTIONS_LIMIT = 20;
        private const int MAX_CONTRIBUTIONS_LIMIT = 100;

        #endregion

        #region Fields

        private readonly ISavingsGoalService service = service;

        #endregion

        #region Actions :: HttpGet

        /// <summary>
        /// Lista os cofrinhos do usuário autenticado.
        /// </summary>
        /// <param name="includeArchived">Quando verdadeiro, inclui os arquivados.</param>
        /// <returns>Cofrinhos com saldo e progresso.</returns>
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<SavingsGoalResponse>>> GetAll([FromQuery] bool includeArchived = false)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            return this.Ok(await this.service.GetAllAsync(userId, includeArchived));
        }

        /// <summary>
        /// Obtém um cofrinho do usuário autenticado.
        /// </summary>
        /// <param name="id">Identificador do cofrinho.</param>
        /// <returns>O cofrinho.</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<SavingsGoalResponse>> GetById(Guid id)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            SavingsGoalResponse? savingsGoal = await this.service.GetByIdAsync(userId, id);

            if (savingsGoal is null)
                return this.NotFound(new { Message = "Cofrinho não encontrado." });

            return this.Ok(savingsGoal);
        }

        /// <summary>
        /// Lista os movimentos de um cofrinho.
        /// </summary>
        /// <param name="id">Identificador do cofrinho.</param>
        /// <param name="limit">Quantidade máxima de movimentos.</param>
        /// <returns>Movimentos do mais recente para o mais antigo.</returns>
        [HttpGet("{id:guid}/contribution")]
        public async Task<ActionResult<IReadOnlyList<ContributionResponse>>> GetContributions(Guid id, [FromQuery] int limit = DEFAULT_CONTRIBUTIONS_LIMIT)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            int safeLimit = limit is < 1 or > MAX_CONTRIBUTIONS_LIMIT ? DEFAULT_CONTRIBUTIONS_LIMIT : limit;

            IReadOnlyList<ContributionResponse>? contributions = await this.service.GetContributionsAsync(userId, id, safeLimit);

            if (contributions is null)
                return this.NotFound(new { Message = "Cofrinho não encontrado." });

            return this.Ok(contributions);
        }

        /// <summary>
        /// Lista os participantes de um cofrinho.
        /// </summary>
        /// <param name="id">Identificador do cofrinho.</param>
        /// <returns>Participantes com quanto cada um aportou.</returns>
        [HttpGet("{id:guid}/member")]
        public async Task<ActionResult<IReadOnlyList<SavingsGoalMemberResponse>>> GetMembers(Guid id)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            IReadOnlyList<SavingsGoalMemberResponse>? members = await this.service.GetMembersAsync(userId, id);

            if (members is null)
                return this.NotFound(new { Message = "Cofrinho não encontrado." });

            return this.Ok(members);
        }

        #endregion

        #region Actions :: HttpPost, HttpDelete :: Member

        /// <summary>
        /// Adiciona um amigo como participante de um cofrinho.
        /// </summary>
        /// <param name="id">Identificador do cofrinho.</param>
        /// <param name="request">Amigo a adicionar.</param>
        /// <returns>O cofrinho atualizado.</returns>
        [HttpPost("{id:guid}/member")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult<SavingsGoalResponse>> AddMember(Guid id, SavingsGoalMemberRequest request)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            SavingsGoalResponse? savingsGoal = await this.service.AddMemberAsync(userId, id, request);

            if (savingsGoal is null)
                return this.NotFound(new { Message = "Cofrinho não encontrado." });

            return this.Ok(savingsGoal);
        }

        /// <summary>
        /// Remove um participante de um cofrinho.
        /// </summary>
        /// <param name="id">Identificador do cofrinho.</param>
        /// <param name="memberUserId">Participante a remover.</param>
        /// <returns>O cofrinho atualizado.</returns>
        [HttpDelete("{id:guid}/member/{memberUserId:guid}")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult<SavingsGoalResponse>> RemoveMember(Guid id, Guid memberUserId)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            SavingsGoalResponse? savingsGoal = await this.service.RemoveMemberAsync(userId, id, memberUserId);

            if (savingsGoal is null)
                return this.NotFound(new { Message = "Participante não encontrado." });

            return this.Ok(savingsGoal);
        }

        /// <summary>
        /// Sai de um cofrinho compartilhado.
        /// </summary>
        /// <param name="id">Identificador do cofrinho.</param>
        /// <returns>Sem conteúdo em caso de sucesso.</returns>
        [HttpDelete("{id:guid}/member")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult> Leave(Guid id)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            bool left = await this.service.LeaveAsync(userId, id);

            if (!left)
                return this.NotFound(new { Message = "Cofrinho não encontrado." });

            return this.NoContent();
        }

        #endregion

        #region Actions :: HttpPost, HttpPut, HttpPatch, HttpDelete

        /// <summary>
        /// Cria um cofrinho.
        /// </summary>
        /// <param name="request">Dados do cofrinho.</param>
        /// <returns>O cofrinho criado.</returns>
        [HttpPost]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult<SavingsGoalResponse>> Create(SavingsGoalRequest request)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            SavingsGoalResponse savingsGoal = await this.service.CreateAsync(userId, request);

            return this.CreatedAtAction(nameof(this.GetById), new { id = savingsGoal.Id }, savingsGoal);
        }

        /// <summary>
        /// Registra um depósito ou resgate em um cofrinho.
        /// </summary>
        /// <param name="id">Identificador do cofrinho.</param>
        /// <param name="request">Dados do movimento.</param>
        /// <returns>O cofrinho com o saldo atualizado.</returns>
        [HttpPost("{id:guid}/contribution")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult<SavingsGoalResponse>> AddContribution(Guid id, ContributionRequest request)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            SavingsGoalResponse? savingsGoal = await this.service.AddContributionAsync(userId, id, request);

            if (savingsGoal is null)
                return this.NotFound(new { Message = "Cofrinho não encontrado." });

            return this.Ok(savingsGoal);
        }

        /// <summary>
        /// Atualiza um cofrinho.
        /// </summary>
        /// <param name="id">Identificador do cofrinho.</param>
        /// <param name="request">Novos dados.</param>
        /// <returns>O cofrinho atualizado.</returns>
        [HttpPut("{id:guid}")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult<SavingsGoalResponse>> Update(Guid id, SavingsGoalRequest request)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            SavingsGoalResponse? savingsGoal = await this.service.UpdateAsync(userId, id, request);

            if (savingsGoal is null)
                return this.NotFound(new { Message = "Cofrinho não encontrado." });

            return this.Ok(savingsGoal);
        }

        /// <summary>
        /// Altera a situação de um cofrinho (arquivar ou reativar).
        /// </summary>
        /// <param name="id">Identificador do cofrinho.</param>
        /// <param name="status">Nova situação.</param>
        /// <returns>O cofrinho atualizado.</returns>
        [HttpPatch("{id:guid}/status")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult<SavingsGoalResponse>> SetStatus(Guid id, [FromQuery] SavingsGoalStatus status)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            if (!Enum.IsDefined(status))
                return this.BadRequest(new { Message = "Situação inválida." });

            SavingsGoalResponse? savingsGoal = await this.service.SetStatusAsync(userId, id, status);

            if (savingsGoal is null)
                return this.NotFound(new { Message = "Cofrinho não encontrado." });

            return this.Ok(savingsGoal);
        }

        /// <summary>
        /// Remove um cofrinho e seus movimentos.
        /// </summary>
        /// <param name="id">Identificador do cofrinho.</param>
        /// <returns>Sem conteúdo em caso de sucesso.</returns>
        [HttpDelete("{id:guid}")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult> Delete(Guid id)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            bool deleted = await this.service.DeleteAsync(userId, id);

            if (!deleted)
                return this.NotFound(new { Message = "Cofrinho não encontrado." });

            return this.NoContent();
        }

        /// <summary>
        /// Remove um movimento de cofrinho.
        /// </summary>
        /// <param name="contributionId">Identificador do movimento.</param>
        /// <returns>O cofrinho com o saldo atualizado.</returns>
        [HttpDelete("contribution/{contributionId:guid}")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult<SavingsGoalResponse>> DeleteContribution(Guid contributionId)
        {
            if (!this.TryGetUserId(out Guid userId))
                return this.Unauthorized(new { Message = "Credenciais inválidas." });

            SavingsGoalResponse? savingsGoal = await this.service.DeleteContributionAsync(userId, contributionId);

            if (savingsGoal is null)
                return this.NotFound(new { Message = "Movimento não encontrado." });

            return this.Ok(savingsGoal);
        }

        #endregion
    }
}
