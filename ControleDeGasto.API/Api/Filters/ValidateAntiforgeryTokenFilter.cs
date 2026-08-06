using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ControleDeGasto.API.Api.Filters
{
    /// <summary>
    /// Valida o token antiforgery (CSRF) das requisições que alteram estado.
    /// </summary>
    public class ValidateAntiforgeryTokenFilter(IAntiforgery antiforgery) : IAsyncActionFilter
    {
        #region Constants

        /// <summary>
        /// Métodos HTTP considerados seguros pela RFC 9110: não alteram estado e, portanto,
        /// não são alvo de CSRF. Precisam ser ignorados porque o interceptor nativo do Angular
        /// (<c>withXsrfConfiguration</c>) se recusa a enviar o header X-XSRF-TOKEN neles.
        /// </summary>
        private static readonly HashSet<string> SAFE_HTTP_METHODS = new(StringComparer.OrdinalIgnoreCase)
        {
            HttpMethods.Get,
            HttpMethods.Head,
            HttpMethods.Options,
            HttpMethods.Trace
        };

        #endregion

        #region Fields

        private readonly IAntiforgery antiforgery = antiforgery;

        #endregion

        #region Members :: OnActionExecutionAsync(), TryValidateAntiforgeryTokenAsync()

        /// <summary>
        /// Intercepta a action e bloqueia a execução quando o token CSRF é inválido ou ausente.
        /// </summary>
        /// <param name="context">Contexto da action em execução.</param>
        /// <param name="next">Delegate que executa o restante do pipeline.</param>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(next);

            if (SAFE_HTTP_METHODS.Contains(context.HttpContext.Request.Method))
            {
                await next();
                return;
            }

            bool isTokenValid = await this.TryValidateAntiforgeryTokenAsync(context.HttpContext);

            if (isTokenValid)
            {
                await next();
            }
            else
            {
                context.Result = new BadRequestObjectResult(new { Message = "Token CSRF inválido ou ausente." });
            }
        }

        /// <summary>
        /// Tenta validar o token antiforgery da requisição.
        /// </summary>
        /// <param name="httpContext">Contexto HTTP da requisição.</param>
        /// <returns><c>true</c> quando o token é válido; caso contrário, <c>false</c>.</returns>
        private async Task<bool> TryValidateAntiforgeryTokenAsync(HttpContext httpContext)
        {
            try
            {
                await this.antiforgery.ValidateRequestAsync(httpContext);
                return true;
            }
            catch (AntiforgeryValidationException)
            {
                return false;
            }
        }

        #endregion
    }
}
