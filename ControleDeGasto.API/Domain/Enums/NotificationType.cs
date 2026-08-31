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
        MonthlySummary = 6,

        /// <summary>Alguém enviou um convite de amizade.</summary>
        FriendRequestReceived = 7,

        /// <summary>Um convite de amizade enviado foi aceito.</summary>
        FriendRequestAccepted = 8,

        /// <summary>Um amigo dividiu uma compra e atribuiu uma parte ao usuário.</summary>
        ExpenseShared = 9,

        /// <summary>Uma divisão de compra foi marcada como acertada.</summary>
        ExpenseShareSettled = 10,

        /// <summary>Uma conta prevista está com vencimento próximo.</summary>
        BillDueSoon = 11,

        /// <summary>Uma conta prevista passou do vencimento sem ser liquidada.</summary>
        BillOverdue = 12,

        /// <summary>O usuário foi adicionado a um cofrinho compartilhado.</summary>
        SharedGoalJoined = 13,

        /// <summary>Saldo de uma carteira de benefício perto do fim antes do próximo crédito.</summary>
        BenefitBalanceLow = 14
    }
}
