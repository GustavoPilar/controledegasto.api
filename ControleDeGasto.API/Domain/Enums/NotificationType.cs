namespace ControleDeGasto.API.Domain.Enums
{
    /// <summary>
    /// Motivo que originou uma notificação.
    /// </summary>
    public enum NotificationType
    {
        /// <summary>Cofrinho atingiu o valor da meta.</summary>
        GoalAchieved = 1,

        /// <summary>Prazo do cofrinho está próximo e a meta ainda não foi atingida.</summary>
        GoalDeadlineNear = 2,

        /// <summary>Reserva de emergência abaixo do valor recomendado.</summary>
        EmergencyReserveLow = 3,

        /// <summary>Gasto de uma categoria cresceu de forma relevante em relação ao mês anterior.</summary>
        HighCategorySpending = 4,

        /// <summary>Saldo do mês ficou negativo (saídas maiores que entradas).</summary>
        NegativeMonthlyBalance = 5,

        /// <summary>Resumo mensal das finanças.</summary>
        MonthlySummary = 6
    }
}
