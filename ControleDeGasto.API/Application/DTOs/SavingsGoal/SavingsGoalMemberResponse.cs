using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Participante de um cofrinho, com quanto aportou.
    /// </summary>
    public class SavingsGoalMemberResponse
    {
        #region Constructors

        /// <summary>
        /// Cria a resposta a partir da participação e do saldo aportado.
        /// </summary>
        /// <param name="member">Participação de origem, com o usuário carregado.</param>
        /// <param name="contributedAmount">Depósitos menos resgates deste participante.</param>
        public SavingsGoalMemberResponse(SavingsGoalMember member, decimal contributedAmount)
        {
            ArgumentNullException.ThrowIfNull(member);

            this.UserId = member.UserId;
            this.FullName = member.User?.FullName ?? string.Empty;
            this.UserName = member.User?.UserName ?? string.Empty;
            this.Role = member.Role;
            this.JoinedAt = member.JoinedAt;
            this.ContributedAmount = contributedAmount;
        }

        #endregion

        #region Properties :: UserId, FullName, UserName, Role, JoinedAt, ContributedAmount

        public Guid UserId { get; set; }

        public string FullName { get; set; }

        public string UserName { get; set; }

        public SavingsGoalMemberRole Role { get; set; }

        public DateTime JoinedAt { get; set; }

        /// <summary>
        /// Quanto este participante colocou. Exibido para que um cofrinho compartilhado deixe
        /// claro a contribuição de cada um, e não apenas o total.
        /// </summary>
        public decimal ContributedAmount { get; set; }

        #endregion
    }
}
