using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Cofrinho devolvido ao cliente, com saldo e progresso já calculados.
    /// </summary>
    /// <remarks>
    /// O progresso é calculado aqui, e não no cliente, para que todas as telas mostrem o
    /// mesmo número e a regra de arredondamento viva em um só lugar.
    /// </remarks>
    public class SavingsGoalResponse
    {
        #region Constructors

        /// <summary>
        /// Cria a resposta a partir do cofrinho e do saldo apurado.
        /// </summary>
        /// <param name="savingsGoal">Cofrinho de origem.</param>
        /// <param name="currentAmount">Saldo acumulado (depósitos menos resgates).</param>
        public SavingsGoalResponse(SavingsGoal savingsGoal, decimal currentAmount)
        {
            ArgumentNullException.ThrowIfNull(savingsGoal);

            this.Id = savingsGoal.Id;
            this.Name = savingsGoal.Name;
            this.TargetAmount = savingsGoal.TargetAmount;
            this.CurrentAmount = currentAmount;
            this.Deadline = savingsGoal.Deadline;
            this.Color = savingsGoal.Color;
            this.Icon = savingsGoal.Icon;
            this.Status = savingsGoal.Status;
            this.IsEmergencyReserve = savingsGoal.IsEmergencyReserve;
            this.CreatedAt = savingsGoal.CreatedAt;
            this.CompletedAt = savingsGoal.CompletedAt;

            this.RemainingAmount = Math.Max(0, savingsGoal.TargetAmount - currentAmount);

            this.ProgressPercentage = savingsGoal.TargetAmount <= 0
                ? 0
                : Math.Round(Math.Min(100, currentAmount / savingsGoal.TargetAmount * 100), 2);

            this.DaysRemaining = savingsGoal.Deadline.HasValue
                ? (int)Math.Ceiling((savingsGoal.Deadline.Value.Date - DateTime.UtcNow.Date).TotalDays)
                : null;
        }

        #endregion

        #region Properties :: Id, Name, TargetAmount, CurrentAmount, RemainingAmount, ProgressPercentage, Deadline, DaysRemaining, Color, Icon, Status, IsEmergencyReserve, CreatedAt, CompletedAt

        public Guid Id { get; set; }

        public string Name { get; set; }

        public decimal TargetAmount { get; set; }

        /// <summary>Saldo acumulado no cofrinho.</summary>
        public decimal CurrentAmount { get; set; }

        /// <summary>Quanto falta para a meta. Zero quando já foi atingida.</summary>
        public decimal RemainingAmount { get; set; }

        /// <summary>Percentual concluído, limitado a 100.</summary>
        public decimal ProgressPercentage { get; set; }

        public DateTime? Deadline { get; set; }

        /// <summary>Dias até o prazo. Negativo quando o prazo passou; nulo sem prazo definido.</summary>
        public int? DaysRemaining { get; set; }

        public string Color { get; set; }

        public string Icon { get; set; }

        public SavingsGoalStatus Status { get; set; }

        public bool IsEmergencyReserve { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        #endregion
    }
}
