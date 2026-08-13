using ControleDeGasto.API.Api.Filters;
using ControleDeGasto.API.Application.Configuration;
using ControleDeGasto.API.Application.DTOs;
using ControleDeGasto.API.Application.Interfaces;
using ControleDeGasto.API.Domain.Entities;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace ControleDeGasto.API.Api.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class AuthController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IAntiforgery antiforgery,
        ILogger<AuthController> logger,
        IConfiguration configuration,
        IEmailSender emailSender,
        IOptions<AppSettings> appSettings,
        IUserService userService,
        ICategoryService categoryService) : ControllerBase
    {
        #region Fields

        private readonly string XSRF_COOKIE_NAME = configuration["XSRF:XSRF_COOKIE_NAME"]!;
        private readonly UserManager<User> UserManager = userManager;
        private readonly SignInManager<User> SignInManager = signInManager;
        private readonly IAntiforgery Antiforgery = antiforgery;
        private readonly ILogger<AuthController> Logger = logger;
        private readonly IEmailSender EmailSender = emailSender;
        private readonly AppSettings AppSettings = appSettings.Value;
        private readonly IUserService userService = userService;
        private readonly ICategoryService categoryService = categoryService;

        private string emailConfirmationBaseUrl => $"{this.AppSettings.FrontendBaseUrl}/confirmEmail";

        #endregion

        #region Members NonAction

        [NonAction]
        private void IssueAntiforgeryCookie()
        {
            AntiforgeryTokenSet tokenSet = this.Antiforgery.GetAndStoreTokens(this.HttpContext);

            this.Response.Cookies.Append(XSRF_COOKIE_NAME, tokenSet.RequestToken!, new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            });
        }

        #endregion

        // A atualização de perfil vive em PUT /api/user/profile, junto das demais operações
        // sobre o usuário. Este controller cuida apenas de autenticação.
        #region Members Actions :: HttpGet, HttpPost

        #region HttpGet

        [HttpGet("csrf-token")]
        [AllowAnonymous]
        public IActionResult GetCsrfToken()
        {
            try
            {
                this.IssueAntiforgeryCookie();

                return this.Ok();
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, ex.Message);
                return this.StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Erro no servidor." });
            }
        }

        [HttpGet("confirm-email")]
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string token)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                {
                    return this.BadRequest(new { Message = "Parâmetros inválidos." });
                }

                User? user = await this.UserManager.FindByIdAsync(userId);

                if (user is null)
                {
                    return this.BadRequest(new { Message = "Link de confirmação inválido." });
                }

                // O token chega já decodificado: o binding de query string do ASP.NET
                // desfaz o escape aplicado na montagem do link.
                IdentityResult result = await this.UserManager.ConfirmEmailAsync(user, token);

                if (!result.Succeeded)
                {
                    this.Logger.LogWarning("Falha ao confirmar e-mail do usuário {UserId}", user.Id);
                    return this.BadRequest(new { Message = "Link de confirmação inválido ou expirado." });
                }

                this.Logger.LogInformation("E-mail confirmado para usuário {UserId}", user.Id);
                return this.Ok(new { Message = "E-mail confirmado com sucesso!" });
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, ex.Message);
                return this.StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Erro no servidor" });
            }
        }

        #endregion

        #region HttpPost

        [HttpPost("register")]
        [AllowAnonymous]
        [ValidateAntiforgeryToken]
        [EnableRateLimiting("RegisterPolicy")]
        public async Task<ActionResult> Register(RegisterRequest request)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(request, nameof(request));
                ArgumentNullException.ThrowIfNull(request.UserRequest, nameof(request.UserRequest));

                if (!this.ModelState.IsValid)
                    return this.BadRequest(new { Message = "Cadastro inválido." });

                User? user = await this.UserManager.FindByNameAsync(request.UserRequest.UserName);

                if (user is not null)
                    return BadRequest(new { Message = "Cadastro inválido." });

                user = await this.UserManager.FindByEmailAsync(request.UserRequest.Email);

                if (user is not null)
                    return BadRequest(new { Message = "Cadastro inválido." });

                user = new User()
                {
                    FullName = request.UserRequest.FullName,
                    Email = request.UserRequest.Email,
                    EmailConfirmed = false,
                    UserName = request.UserRequest.UserName,
                    Active = true,
                    CreatedAt = DateTime.UtcNow,
                };

                IdentityResult result = await this.UserManager.CreateAsync(user, request.UserRequest.Password);

                if (!result.Succeeded)
                    return BadRequest(new { Message = result.Errors.First().Description });

                IdentityResult roleResult = await this.UserManager.AddToRoleAsync(user, RoleSeender.STANDARD_ROLE);

                if (!roleResult.Succeeded)
                {
                    await this.UserManager.DeleteAsync(user);

                    return BadRequest(new { Message = roleResult.Errors.First().Description });
                }

                // A preferência é opcional no payload: quando o visitante não escolheu tema na
                // tela de login, o serviço assume o tema claro.
                UserPreference? userPreference = await this.userService.CreateUserPreferenceAsync(user, request.UserPreference);

                if (userPreference is null)
                {
                    // Compensação: o Identity já commitou o usuário e o papel em transações
                    // próprias, então a única forma de não deixar conta órfã de preferência é
                    // desfazer o cadastro.
                    await this.UserManager.DeleteAsync(user);

                    this.Logger.LogError("Cadastro desfeito: falha ao criar preferência do usuário {UserId}.", user.Id);
                    return BadRequest(new { Message = "Erro ao salvar preferência de usuário" });
                }

                // Categorias padrão: sem elas a conta nova não conseguiria lançar nada antes de
                // cadastrar categorias na mão. A falha aqui não desfaz o cadastro — o usuário
                // consegue criar as próprias categorias — mas é registrada para investigação.
                int defaultCategories = await this.categoryService.CreateDefaultsAsync(user.Id);

                if (defaultCategories == 0)
                    this.Logger.LogWarning("Nenhuma categoria padrão criada para o usuário {UserId}.", user.Id);

                string token = await this.UserManager.GenerateEmailConfirmationTokenAsync(user);

                string encodedToken = Uri.EscapeDataString(token);

                string confirmationLink =
                    $"{this.emailConfirmationBaseUrl}?userId={user.Id}&token={encodedToken}";

                string emailBody = $"""
                    <h1>Bem-vindo ao Controle de Gasto!</h1>
                    <p>Confirme seu e-mail clicando no link abaixo:</p>
                    <a href="{confirmationLink}">Confirmar E-mail</a>
                    <p>Se você não criou essa conta, ignore este e-mail.<p>
                    """;

                await this.EmailSender.SendEmailAsync(user.Email, "Confirmação de conta - Controle de Gasto", emailBody);

                this.Logger.LogInformation("Usuário {user.Id} registrado, e-mail de confirmação eviado.", user.Id);
                return this.Created(string.Empty, new { Succeeded = true });
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, ex.Message);
                return this.StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Erro no servidor" });
            }
        }

        [HttpPost("login")]
        [ValidateAntiforgeryToken]
        [AllowAnonymous]
        [EnableRateLimiting("LoginPolicy")]
        public async Task<ActionResult<UserResponse>> Login(LoginRequest request)
        {
            try
            {
                User? user = await this.UserManager.FindByEmailAsync(request.Email);

                if (user is null)
                    return this.Unauthorized(new { Message = "Credenciais inválidas." });

                Microsoft.AspNetCore.Identity.SignInResult result = await this.SignInManager.PasswordSignInAsync(user, request.Password, isPersistent: request.RememberMe, lockoutOnFailure: true);

                if (result.IsLockedOut)
                {
                    this.Logger.LogWarning("Usuário {UserId} bloqueado por excesso de tentativas.", user.Id);
                    return this.StatusCode(StatusCodes.Status423Locked, new { Message = "Conta temporária bloqueada" });
                }

                if (!result.Succeeded)
                {
                    if (result.IsNotAllowed)
                    {
                        return this.Unauthorized(new { Message = "Confirme seu e-mail" });
                    }

                    return this.Unauthorized(new { Message = "Credenciais inválidas." });
                }

                this.IssueAntiforgeryCookie();

                // Carrega a preferência para o cliente aplicar o tema já na resposta do login,
                // sem um segundo round-trip que causaria flash do tema anterior.
                user.UserPreference = await this.userService.GetUserPreferenceAsync(user.Id);

                this.Logger.LogInformation("Usuário {UserId} autenticado com sucesso.", user.Id);
                return this.Ok(new UserResponse(user));
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, ex.Message);
                return this.StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Erro no servidor." });
            }
        }

        [HttpPost("logout")]
        [ValidateAntiforgeryToken]
        public async Task<ActionResult> Logout()
        {
            try
            {
                await this.SignInManager.SignOutAsync();

                this.Response.Cookies.Delete(XSRF_COOKIE_NAME);

                return this.NoContent();
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, ex.Message);
                return this.StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Erro no servidor" });
            }
        }

        #endregion

        #endregion
    }
}
