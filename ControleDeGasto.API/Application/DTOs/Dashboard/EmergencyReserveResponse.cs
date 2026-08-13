namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Situação da reserva de emergência, incluindo o valor recomendado.
    /// </summary>
    /// <remarks>
    /// A recomendação usa a média de gastos mensais dos últimos meses multiplicada pelo número
    /// de meses de cobertura desejado. Fica no servidor porque é a mesma regra usada para
    /// decidir se um aviso deve ser disparado.
    /// </remarks>
    public class EmergencyReserveResponse
    {
        #region Properties :: Exists, SavingsGoalId, CurrentAmount, TargetAmount, RecommendedAmount, AverageMonthlyExpense, MonthsCovered, ProgressPercentage

        /// <summary>Falso quando o usuário ainda não criou a reserva.</summary>
        public bool Exists { get; set; }

        public Guid? SavingsGoalId { get; set; }

        /// <summary>Saldo atual da reserva.</summary>
        public decimal CurrentAmount { get; set; }

        /// <summary>Meta definida pelo usuário para a reserva.</summary>
        public decimal TargetAmount { get; set; }

        /// <summary>Valor sugerido pelo sistema com base na média de gastos.</summary>
        public decimal RecommendedAmount { get; set; }

        /// <summary>Média de saídas por mês no período analisado.</summary>
        public decimal AverageMonthlyExpense { get; set; }

        /// <summary>Quantos meses de gasto médio o saldo atual cobre.</summary>
        public decimal MonthsCovered { get; set; }

        /// <summary>Progresso em relação à meta definida, em percentual.</summary>
        public decimal ProgressPercentage { get; set; }

        #endregion
    }
}
