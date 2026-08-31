using ControleDeGasto.API.Domain.Enums;

namespace ControleDeGasto.API.Domain.Entities
{
    /// <summary>
    /// Participante de um cofrinho. O criador também tem uma linha, com papel
    /// <see cref="SavingsGoalMemberRole.Owner"/>.
    /// </summary>
    /// <remarks>
    /// Incluir o dono na tabela em vez de tratá-lo como caso especial faz o controle de acesso
    /// ser uma única pergunta ("existe participação deste usuário neste cofrinho?"), em vez de
    /// um OU entre dono e participante repetido em cada consulta.
    /// </remarks>
    public class SavingsGoalMember
    {
        #region Properties :: Id, SavingsGoalId, UserId, Role, JoinedAt, SavingsGoal, User

        public Guid Id { get; set; }

        public Guid SavingsGoalId { get; set; }

        public Guid UserId { get; set; }

        public SavingsGoalMemberRole Role { get; set; } = SavingsGoalMemberRole.Member;

        public DateTime JoinedAt { get; set; }

        public SavingsGoal? SavingsGoal { get; set; }

        public User? User { get; set; }

        #endregion
    }
}
