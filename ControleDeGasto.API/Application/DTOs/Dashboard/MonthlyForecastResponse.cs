namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Previsão de entradas e saídas de um mês.
    /// </summary>
    /// <remarks>
    /// Soma três origens: o que já foi liquidado, o que está lançado como previsto (contas a
    /// pagar, a receber e parcelas) e o que as definições fixas dizem que ainda vai acontecer.
    /// <para>
    /// Um valor fixo já lançado à mão não é somado de novo: para cada categoria, o que existe
    /// de lançamento no mês abate a previsão fixa correspondente. Sem esse abatimento, quem
    /// registrasse o próprio aluguel veria a despesa contada duas vezes.
    /// </para>
    /// </remarks>
    public class MonthlyForecastResponse
    {
        #region Properties :: Year, Month, PeriodStart, PeriodEnd

        public int Year { get; set; }

        public int Month { get; set; }

        public DateTime PeriodStart { get; set; }

        public DateTime PeriodEnd { get; set; }

        #endregion

        #region Properties :: SettledIncome, SettledExpense, SettledBalance

        /// <summary>Entradas que já caíram no mês.</summary>
        public decimal SettledIncome { get; set; }

        /// <summary>Saídas que já saíram no mês.</summary>
        public decimal SettledExpense { get; set; }

        /// <summary>Entradas menos saídas já realizadas.</summary>
        public decimal SettledBalance { get; set; }

        #endregion

        #region Properties :: PendingIncome, PendingExpense, OverdueIncome, OverdueExpense

        /// <summary>Entradas lançadas como previstas e ainda não recebidas.</summary>
        public decimal PendingIncome { get; set; }

        /// <summary>Saídas lançadas como previstas e ainda não pagas.</summary>
        public decimal PendingExpense { get; set; }

        /// <summary>Parte das entradas previstas cujo vencimento já passou.</summary>
        public decimal OverdueIncome { get; set; }

        /// <summary>Parte das saídas previstas cujo vencimento já passou.</summary>
        public decimal OverdueExpense { get; set; }

        #endregion

        #region Properties :: FixedIncome, FixedExpense, RemainingFixedIncome, RemainingFixedExpense, BenefitCredits

        /// <summary>Total das entradas fixas cadastradas para o mês.</summary>
        public decimal FixedIncome { get; set; }

        /// <summary>Total das saídas fixas cadastradas para o mês.</summary>
        public decimal FixedExpense { get; set; }

        /// <summary>Parte das entradas fixas que ainda não apareceu como lançamento no mês.</summary>
        public decimal RemainingFixedIncome { get; set; }

        /// <summary>Parte das saídas fixas que ainda não apareceu como lançamento no mês.</summary>
        public decimal RemainingFixedExpense { get; set; }

        /// <summary>
        /// Créditos de benefício previstos para o mês (VR, VA, VT, VC). Fora do saldo livre.
        /// </summary>
        public decimal BenefitCredits { get; set; }

        #endregion

        #region Properties :: ProjectedIncome, ProjectedExpense, ProjectedBalance, CommittedPercentage

        /// <summary>Liquidado mais previsto mais fixo restante, do lado das entradas.</summary>
        public decimal ProjectedIncome { get; set; }

        /// <summary>Liquidado mais previsto mais fixo restante, do lado das saídas.</summary>
        public decimal ProjectedExpense { get; set; }

        /// <summary>Quanto deve sobrar no fim do mês se tudo acontecer como previsto.</summary>
        public decimal ProjectedBalance { get; set; }

        /// <summary>
        /// Percentual das entradas previstas já comprometido com saídas. Acima de 100 significa
        /// que o mês fecha no negativo.
        /// </summary>
        public decimal CommittedPercentage { get; set; }

        #endregion

        #region Properties :: Items

        /// <summary>
        /// Linhas que compõem a previsão, do mais próximo para o mais distante. Não inclui o
        /// que já foi liquidado: o objetivo é mostrar o que ainda vai acontecer.
        /// </summary>
        public IReadOnlyList<ForecastItemResponse> Items { get; set; } = [];

        #endregion
    }
}
