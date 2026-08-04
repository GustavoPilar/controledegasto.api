using ControleDeGasto.API.Api.Filters;
using ControleDeGasto.API.Application.Configuration;
using ControleDeGasto.API.Application.DTOs;
using ControleDeGasto.API.Domain.Entities;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace ControleDeGasto.API.Api.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class AuthController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IAntiforgery antiforgery,
        ILogger<AuthController> logger) : ControllerBase
    {
        private readonly UserManager<User> UserManager = userManager;
        private readonly SignInManager<User> SignInManager = signInManager;
        private readonly IAntiforgery antiforgery = antiforgery;
        private readonly ILogger<AuthController> Logger = logger;

        [HttpGet("csrf-token")]
        public IActionResult GetCsrfToken()
        {
            AntiforgeryTokenSet tokenSet = this.antiforgery.GetAndStoreTokens(this.HttpContext);

            this.Response.Cookies.Append("XSRF-TOKEN", tokenSet.RequestToken!, new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            return this.Ok();
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(UserRequest request)
        {
            try
            {
                User? user = await this.UserManager.FindByNameAsync(request.UserName);

                if (user is not null)
                    return BadRequest(new { Message = "Nome de usuário já cadastrado." });

                user = new User()
                {
                    UserName = request.UserName,
                    Active = true,
                    CreatedAt = DateTime.UtcNow,
                };

                IdentityResult result = await this.UserManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                    return BadRequest(new { Message = result.Errors.First().Description });

                IdentityResult roleResult = await this.UserManager.AddToRoleAsync(user, RoleSeender.STANDARD_ROLE);

                if (!roleResult.Succeeded)
                {
                    await this.UserManager.DeleteAsync(user);

                    return BadRequest(new { Message = roleResult.Errors.First().Description });
                }

                return this.Ok(new { Message = $"Usuário {user.UserName} foi criado." });
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, ex.Message);
                return this.StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Erro no servidor" });
            }
        }

        [HttpPost("login")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult> Login(Application.DTOs.LoginRequest request)
        {
            try
            {
                User? user = await this.UserManager.FindByNameAsync(request.UserName);

                if (user is null)
                    return this.Unauthorized(new { Message = "Credenciais inválidas." });

                Microsoft.AspNetCore.Identity.SignInResult result = await this.SignInManager.PasswordSignInAsync(user, request.Password, isPersistent: true, lockoutOnFailure: true);

                if (result.IsLockedOut)
                {
                    this.Logger.LogWarning("Usuário {UserId} bloqueado por excesso de tentativas.", user.Id);
                    return this.StatusCode(StatusCodes.Status423Locked, new { Message = "Conta temporária bloqueada" });
                }

                if (!result.Succeeded)
                {
                    return this.Unauthorized(new { Message = "Credenciais inválidas." });
                }

                this.Logger.LogInformation("Usuário {UserId} autenticado com sucesso.", user.Id);
                return this.Ok(new { Message = "Login realizado com sucesso" });
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, ex.Message);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

    }
}
