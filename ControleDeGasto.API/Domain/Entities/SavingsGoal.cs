using ControleDeGasto.API.Domain.Enums;

namespace ControleDeGasto.API.Domain.Entities
{
    /// <summary>
    /// Cofrinho: objetivo de acúmulo de dinheiro. A reserva de emergência é um cofrinho
    /// marcado com <see cref="IsEmergencyReserve"/>.
    /// </summary>
    /// <remarks>
    /// O saldo não é armazenado: é sempre a soma dos <see cref="SavingsGoalContribution"/>.
    /// Guardar um total mutável permitiria perda de atualização em aportes concorrentes
    /// (dois aportes lendo o mesmo saldo e gravando valores que se sobrescrevem).
    /// </remarks>
    public class SavingsGoal
    {
        #region Properties :: Id, UserId, Name, TargetAmount, Deadline, Color, Icon, Status, IsEmergencyReserve, CreatedAt, UpdatedAt, CompletedAt, User, Contributions, Members

        public Guid Id { get; set; }

        /// <summary>
        /// Criador do cofrinho. Continua sendo quem edita, convida e exclui; os demais
        /// participantes vivem em <see cref="Members"/>.
        /// </summary>
        public Guid UserId { get; set; }

        public string Name { get; set; } = string.Empty;

        /// <summary>Valor que se deseja alcançar. Sempre positivo.</summary>
        public decimal TargetAmount { get; set; }

        /// <summary>Prazo desejado, em UTC. Nulo quando o objetivo não tem data.</summary>
        public DateTime? Deadline { get; set; }

        /// <summary>Cor em hexadecimal (#RRGGBB), usada nos gráficos.</summary>
        public string Color { get; set; } = string.Empty;

        /// <summary>Nome do ícone exibido na interface.</summary>
        public string Icon { get; set; } = string.Empty;

        public SavingsGoalStatus Status { get; set; } = SavingsGoalStatus.Active;

        /// <summary>
        /// Marca o cofrinho como reserva de emergência. Cada usuário tem no máximo um,
        /// garantido por índice único filtrado.
        /// </summary>
        public bool IsEmergencyReserve { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        /// <summary>Momento em que a meta foi atingida, em UTC.</summary>
        public DateTime? CompletedAt { get; set; }

        public User? User { get; set; }

        public ICollection<SavingsGoalContribution>? Contributions { get; set; }

        /// <summary>
        /// Participantes, incluindo o criador. Um cofrinho individual tem exatamente um.
        /// </summary>
        public ICollection<SavingsGoalMember>? Members { get; set; }

        #endregion
    }
}
