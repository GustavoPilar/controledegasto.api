using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ControleDeGasto.API.Api.Filters
{
    public class ValidateAntiforgeryTokenFilter(IAntiforgery antiforgery) : IAsyncActionFilter
    {
        private readonly IAntiforgery antiforgery = antiforgery;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            bool isTokenValid = await this.TryValidateAntiforgeryTokenAsync(context.HttpContext);

            if (isTokenValid)
            {
                await next();
            }
            else
            {
                context.Result = new BadRequestObjectResult(new { message = "Token CSRF inválido ou ausente." });
            }
        }

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
    }
}
