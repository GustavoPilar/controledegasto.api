using ControleDeGasto.API.Domain.ReadModels;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Dados públicos de um usuário nas telas sociais.
    /// </summary>
    /// <remarks>
    /// O e-mail não é devolvido de propósito. Ele serve para encontrar a pessoa na busca, mas
    /// exibi-lo depois entregaria o endereço de qualquer usuário a quem soubesse o apelido.
    /// </remarks>
    public class UserSummaryResponse
    {
        #region Constructors

        /// <summary>
        /// Cria a resposta a partir da projeção do repositório.
        /// </summary>
        /// <param name="summary">Dados públicos apurados.</param>
        public UserSummaryResponse(UserSummary summary)
        {
            ArgumentNullException.ThrowIfNull(summary);

            this.UserId = summary.UserId;
            this.FullName = summary.FullName;
            this.UserName = summary.UserName;
        }

        #endregion

        #region Properties :: UserId, FullName, UserName

        public Guid UserId { get; set; }

        public string FullName { get; set; }

        public string UserName { get; set; }

        #endregion
    }
}
