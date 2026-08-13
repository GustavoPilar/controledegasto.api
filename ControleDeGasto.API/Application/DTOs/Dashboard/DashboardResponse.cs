namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Conteúdo completo do painel para um período.
    /// </summary>
    /// <remarks>
    /// Reunido em uma única resposta de propósito: o painel abre com sete blocos de
    /// informação e sete requisições paralelas custariam mais latência e mais consultas de
    /// autenticação do que uma composição feita no servidor.
    /// </remarks>
    public class DashboardResponse
    {
        #region Properties :: PeriodStart, PeriodEnd, TotalIncome, TotalExpense, Balance, SavingsRate, TotalSaved, EmergencyReserve, TopExpenseCategories, TopIncomeCategories, MonthlyEvolution, Goals, RecentTransactions

        public DateTime PeriodStart { get; set; }

        public DateTime PeriodEnd { get; set; }

        /// <summary>Total de entradas no período.</summary>
        public decimal TotalIncome { get; set; }

        /// <summary>Total de saídas no período.</summary>
        public decimal TotalExpense { get; set; }

        /// <summary>Entradas menos saídas no período.</summary>
        public decimal Balance { get; set; }

        /// <summary>Percentual das entradas que sobrou no período.</summary>
        public decimal SavingsRate { get; set; }

        /// <summary>Soma dos saldos de todos os cofrinhos.</summary>
        public decimal TotalSaved { get; set; }

        public EmergencyReserveResponse EmergencyReserve { get; set; } = new EmergencyReserveResponse();

        /// <summary>Categorias que mais consumiram dinheiro no período.</summary>
        public IReadOnlyList<CategorySpendingResponse> TopExpenseCategories { get; set; } = [];

        /// <summary>Categorias que mais trouxeram dinheiro no período.</summary>
        public IReadOnlyList<CategorySpendingResponse> TopIncomeCategories { get; set; } = [];

        /// <summary>Série mensal de entradas, saídas e saldo.</summary>
        public IReadOnlyList<MonthlyEvolutionResponse> MonthlyEvolution { get; set; } = [];

        /// <summary>Cofrinhos ativos com progresso calculado.</summary>
        public IReadOnlyList<SavingsGoalResponse> Goals { get; set; } = [];

        /// <summary>Últimos lançamentos, para conferência rápida.</summary>
        public IReadOnlyList<TransactionResponse> RecentTransactions { get; set; } = [];

        #endregion
    }
}
