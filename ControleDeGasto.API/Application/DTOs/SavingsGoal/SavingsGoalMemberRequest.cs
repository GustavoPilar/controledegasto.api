using System.ComponentModel.DataAnnotations;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Amigo a adicionar como participante de um cofrinho.
    /// </summary>
    public class SavingsGoalMemberRequest
    {
        #region Properties :: FriendUserId

        [Required(ErrorMessage = "Informe o amigo a adicionar.")]
        public Guid FriendUserId { get; set; }

        #endregion
    }
}
