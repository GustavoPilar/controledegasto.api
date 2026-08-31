namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Conteúdo completo do painel para um período.
    /// </summary>
    /// <remarks>
    /// Reunido em uma única resposta de propósito: o painel abre com uma dezena de blocos de
    /// informação, e uma requisição por bloco custaria mais latência e mais consultas de
    /// autenticação do que uma composição feita no servidor.
    /// </remarks>
    public class DashboardResponse
    {
        #region Properties :: PeriodStart, PeriodEnd, TotalIncome, TotalExpense, Balance, SavingsRate, TotalSaved

        public DateTime PeriodStart { get; set; }

        public DateTime PeriodEnd { get; set; }

        /// <summary>Total de entradas liquidadas no período.</summary>
        public decimal TotalIncome { get; set; }

        /// <summary>Total de saídas liquidadas no período.</summary>
        public decimal TotalExpense { get; set; }

        /// <summary>Entradas menos saídas no período, considerando só o que foi liquidado.</summary>
        public decimal Balance { get; set; }

        /// <summary>Percentual das entradas que sobrou no período.</summary>
        public decimal SavingsRate { get; set; }

        /// <summary>Soma dos saldos de todos os cofrinhos em que o usuário participa.</summary>
        public decimal TotalSaved { get; set; }

        #endregion

        #region Properties :: EmergencyReserve, TopExpenseCategories, TopIncomeCategories, MonthlyEvolution, Goals, RecentTransactions

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

        #region Properties :: Forecast, UpcomingBills, OverdueBills

        /// <summary>Previsão do mês a partir das definições fixas e dos lançamentos previstos.</summary>
        public MonthlyForecastResponse Forecast { get; set; } = new MonthlyForecastResponse();

        /// <summary>Contas a vencer nos próximos dias.</summary>
        public IReadOnlyList<TransactionResponse> UpcomingBills { get; set; } = [];

        /// <summary>Contas previstas cujo vencimento já passou.</summary>
        public IReadOnlyList<TransactionResponse> OverdueBills { get; set; } = [];

        #endregion

        #region Properties :: Wallets, BenefitWallets, Shared, TagTotals, OpenInstallmentPlans

        /// <summary>Carteiras de dinheiro (conta, espécie, cartão) com saldo apurado.</summary>
        public IReadOnlyList<WalletResponse> Wallets { get; set; } = [];

        /// <summary>
        /// Carteiras de benefício (VR, VA, VT, VC) separadas das demais: o saldo delas não se
        /// soma ao dinheiro livre, e misturá-las inflaria o patrimônio exibido.
        /// </summary>
        public IReadOnlyList<WalletResponse> BenefitWallets { get; set; } = [];

        /// <summary>Resumo das divisões de compra em aberto com amigos.</summary>
        public SharedSummaryResponse Shared { get; set; } = new SharedSummaryResponse();

        /// <summary>Totais por etiqueta no período.</summary>
        public IReadOnlyList<TagTotalResponse> TagTotals { get; set; } = [];

        /// <summary>Compras parceladas que ainda têm parcela em aberto.</summary>
        public IReadOnlyList<InstallmentPlanResponse> OpenInstallmentPlans { get; set; } = [];

        #endregion
    }
}
