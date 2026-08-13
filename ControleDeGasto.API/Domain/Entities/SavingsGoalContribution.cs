using ControleDeGasto.API.Domain.Enums;

namespace ControleDeGasto.API.Domain.Entities
{
    /// <summary>
    /// Movimento de dinheiro em um cofrinho (depósito ou resgate).
    /// </summary>
    public class SavingsGoalContribution
    {
        #region Properties :: Id, SavingsGoalId, UserId, Amount, Kind, OccurredOn, Note, CreatedAt, SavingsGoal, User

        public Guid Id { get; set; }

        public Guid SavingsGoalId { get; set; }

        /// <summary>
        /// Dono do movimento. Redundante em relação ao cofrinho, mas permite filtrar por
        /// usuário sem join — o que toda consulta faz, por segurança.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>Valor sempre positivo. O sentido vem de <see cref="Kind"/>.</summary>
        public decimal Amount { get; set; }

        public ContributionKind Kind { get; set; }

        /// <summary>Data do movimento, em UTC.</summary>
        public DateTime OccurredOn { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }

        public SavingsGoal? SavingsGoal { get; set; }

        public User? User { get; set; }

        #endregion
    }
}
