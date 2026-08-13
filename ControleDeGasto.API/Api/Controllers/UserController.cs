using ControleDeGasto.API.Api.Filters;
using ControleDeGasto.API.Application.DTOs;
using ControleDeGasto.API.Application.Interfaces;
using ControleDeGasto.API.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ControleDeGasto.API.Api.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class UserController(
        UserManager<User> userManager,
        IUserService userService,
        ILogger<UserController> logger) : ControllerBase
    {
        #region Fields

        private readonly UserManager<User> userManager = userManager;
        private readonly IUserService userService = userService;
        private readonly ILogger<UserController> logger = logger;

        #endregion

        #region Actions :: HttpPut

        #region HttpPut

        /// <summary>
        /// Atualiza o perfil do usuário autenticado.
        /// </summary>
        /// <remarks>
        /// Substitui o antigo PATCH em /api/auth/profile, que o cliente nunca chamava: o front
        /// apontava para este endereço, que não existia, e a tela de perfil devolvia 404.
        /// </remarks>
        /// <param name="request">Novos dados do perfil.</param>
        /// <returns>O usuário atualizado.</returns>
        [HttpPut("profile")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult<UserResponse>> PutProfile(ProfileRequest request)
        {
            try
            {
                if (request is null)
                    return this.BadRequest(new { Message = "Dados inválidos." });

                if (!this.ModelState.IsValid)
                    return this.BadRequest(new { Message = "Dados inválidos." });

                User? user = await this.userManager.GetUserAsync(this.User);

                if (user is null)
                    return this.Unauthorized(new { Message = "Credenciais inválidas." });

                user.FullName = request.FullName.Trim();
                user.UserName = request.UserName.Trim();
                user.UpdatedAt = DateTime.UtcNow;

                IdentityResult result = await this.userManager.UpdateAsync(user);

                if (!result.Succeeded)
                    return this.BadRequest(new { Message = result.Errors.First().Description });

                // A preferência acompanha a resposta para o cliente manter o perfil completo em
                // memória, no mesmo formato devolvido pelo login.
                user.UserPreference = await this.userService.GetUserPreferenceAsync(user.Id);

                this.logger.LogInformation("Perfil do usuário {UserId} atualizado.", user.Id);

                return this.Ok(new UserResponse(user));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
                return this.StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Erro interno" });
            }
        }

        /// <summary>
        /// Atualiza as preferências do usuário autenticado.
        /// </summary>
        /// <param name="request">Novas preferências.</param>
        /// <returns>As preferências atualizadas.</returns>
        [HttpPut("preference")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult<UserPreferenceResponse>> PutPreference(UserPreferenceRequest request)
        {
            try
            {
                if (request is null)
                    return this.BadRequest(new { Message = "Dados inválidos." });

                if (!this.ModelState.IsValid)
                    return this.BadRequest(new { Message = "Aparência inválida." });

                User? user = await this.userManager.GetUserAsync(this.User);

                if (user is null)
                    return this.Unauthorized(new { Message = "Credenciais inválidas." });

                UserPreferenceResponse? response = await this.userService.UpdateUserPreferenceAsync(user.Id, request);

                if (response is null)
                    return this.BadRequest(new { Message = "Erro ao atualizar preferência." });

                return this.Ok(response);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
                return this.StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Erro interno" });
            }
        }

        #endregion

        #endregion
    }
}
