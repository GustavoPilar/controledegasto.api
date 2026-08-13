namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Entradas, saídas e saldo de um mês, para o gráfico de evolução.
    /// </summary>
    public class MonthlyEvolutionResponse
    {
        #region Constructors

        /// <summary>
        /// Cria o ponto do gráfico calculando o saldo do mês.
        /// </summary>
        /// <param name="year">Ano de competência.</param>
        /// <param name="month">Mês de competência (1 a 12).</param>
        /// <param name="income">Total de entradas.</param>
        /// <param name="expense">Total de saídas.</param>
        public MonthlyEvolutionResponse(int year, int month, decimal income, decimal expense)
        {
            this.Year = year;
            this.Month = month;
            this.Income = income;
            this.Expense = expense;
            this.Balance = income - expense;
        }

        #endregion

        #region Properties :: Year, Month, Income, Expense, Balance

        public int Year { get; set; }

        public int Month { get; set; }

        public decimal Income { get; set; }

        public decimal Expense { get; set; }

        /// <summary>Entradas menos saídas do mês. Negativo quando gastou mais do que recebeu.</summary>
        public decimal Balance { get; set; }

        #endregion
    }
}
