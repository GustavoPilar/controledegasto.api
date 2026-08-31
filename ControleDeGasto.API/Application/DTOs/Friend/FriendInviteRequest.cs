using System.ComponentModel.DataAnnotations;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Convite de amizade a ser enviado.
    /// </summary>
    /// <remarks>
    /// Recebe o identificador, e não o e-mail: o cliente já obteve o usuário pela busca, e
    /// aceitar e-mail aqui transformaria o convite em um oráculo de "esta conta existe".
    /// </remarks>
    public class FriendInviteRequest
    {
        #region Properties :: TargetUserId

        [Required(ErrorMessage = "Informe o usuário.")]
        public Guid TargetUserId { get; set; }

        #endregion
    }
}
