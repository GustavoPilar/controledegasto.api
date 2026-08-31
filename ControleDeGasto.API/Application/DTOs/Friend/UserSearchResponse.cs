using ControleDeGasto.API.Domain.Enums;
using ControleDeGasto.API.Domain.ReadModels;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Usuário encontrado na busca, já com a situação da relação com quem procurou.
    /// </summary>
    /// <remarks>
    /// A situação acompanha o resultado para a tela decidir o botão certo ("Convidar",
    /// "Convite enviado", "Responder", "Já é amigo") sem uma segunda chamada por linha.
    /// </remarks>
    public class UserSearchResponse
    {
        #region Constructors

        /// <summary>
        /// Cria a resposta a partir do usuário encontrado e da relação existente.
        /// </summary>
        /// <param name="summary">Dados públicos do usuário.</param>
        /// <param name="status">Situação da relação, ou nulo quando os dois nunca interagiram.</param>
        /// <param name="isIncoming">Indica que o convite pendente foi recebido, não enviado.</param>
        public UserSearchResponse(UserSummary summary, FriendshipStatus? status, bool isIncoming)
        {
            ArgumentNullException.ThrowIfNull(summary);

            this.UserId = summary.UserId;
            this.FullName = summary.FullName;
            this.UserName = summary.UserName;
            this.Status = status;
            this.IsIncoming = isIncoming;
        }

        #endregion

        #region Properties :: UserId, FullName, UserName, Status, IsIncoming

        public Guid UserId { get; set; }

        public string FullName { get; set; }

        public string UserName { get; set; }

        /// <summary>Situação da relação. Nulo quando ainda não existe relação alguma.</summary>
        public FriendshipStatus? Status { get; set; }

        /// <summary>Verdadeiro quando o convite pendente veio deste usuário.</summary>
        public bool IsIncoming { get; set; }

        #endregion
    }
}
