using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ControleDeGasto.API.Api.Controllers
{
    /// <summary>
    /// Base dos controllers que operam sobre dados do usuário autenticado.
    /// </summary>
    /// <remarks>
    /// O identificador sai da claim do cookie de autenticação, não do corpo nem da query da
    /// requisição: é o que impede um cliente de pedir os dados de outra conta. Ler da claim
    /// também evita uma consulta ao banco por requisição só para descobrir quem está logado.
    /// </remarks>
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        #region Members :: TryGetUserId()

        /// <summary>
        /// Obtém o identificador do usuário autenticado.
        /// </summary>
        /// <param name="userId">Recebe o identificador quando a claim é válida.</param>
        /// <returns>True quando há um usuário autenticado identificável.</returns>
        protected bool TryGetUserId(out Guid userId)
        {
            string? value = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out userId);
        }

        #endregion
    }
}
