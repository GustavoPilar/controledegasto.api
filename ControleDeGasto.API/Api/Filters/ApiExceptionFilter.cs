using ControleDeGasto.API.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ControleDeGasto.API.Api.Filters
{
    /// <summary>
    /// Converte exceções em respostas HTTP coerentes.
    /// </summary>
    /// <remarks>
    /// Existe para que os controllers não repitam try/catch em toda action. Violação de regra
    /// de negócio vira 400 com a mensagem; qualquer outra falha vira 500 com mensagem genérica,
    /// registrando o detalhe no log — detalhe de exceção na resposta é vazamento de informação.
    /// </remarks>
    public class ApiExceptionFilter(
        ILogger<ApiExceptionFilter> logger) : IExceptionFilter
    {
        #region Fields

        private readonly ILogger<ApiExceptionFilter> logger = logger;

        #endregion

        #region Members :: OnException()

        /// <summary>
        /// Trata a exceção que escapou da action.
        /// </summary>
        /// <param name="context">Contexto da exceção.</param>
        public void OnException(ExceptionContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (context.Exception is DomainException domainException)
            {
                this.logger.LogWarning(
                    "Regra de negócio violada em {Path}: {Message}",
                    context.HttpContext.Request.Path,
                    domainException.Message);

                context.Result = new BadRequestObjectResult(new { Message = domainException.Message });
                context.ExceptionHandled = true;

                return;
            }

            if (context.Exception is ArgumentException argumentException)
            {
                this.logger.LogWarning(
                    "Argumento inválido em {Path}: {Message}",
                    context.HttpContext.Request.Path,
                    argumentException.Message);

                context.Result = new BadRequestObjectResult(new { Message = "Dados inválidos." });
                context.ExceptionHandled = true;

                return;
            }

            this.logger.LogError(
                context.Exception,
                "Falha não tratada em {Path}.",
                context.HttpContext.Request.Path);

            context.Result = new ObjectResult(new { Message = "Erro no servidor." })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };

            context.ExceptionHandled = true;
        }

        #endregion
    }
}
