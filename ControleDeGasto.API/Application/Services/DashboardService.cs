using ControleDeGasto.API.Application.DTOs;
using ControleDeGasto.API.Application.Interfaces;
using ControleDeGasto.API.Application.Utilities;
using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;
using ControleDeGasto.API.Domain.Interfaces;
using ControleDeGasto.API.Domain.ReadModels;

namespace ControleDeGasto.API.Application.Services
{
    public class DashboardService(
        ITransactionRepository transactionRepository,
        ISavingsGoalRepository savingsGoalRepository) : IDashboardService
    {
        #region Constants :: EMERGENCY_RESERVE_MONTHS, AVERAGE_EXPENSE_WINDOW_MONTHS, TOP_CATEGORIES_LIMIT, RECENT_TRANSACTIONS_LIMIT, EVOLUTION_WINDOW_MONTHS

        /// <summary>Meses de gasto médio que a reserva de emergência deve cobrir.</summary>
        public const int EMERGENCY_RESERVE_MONTHS = 6;

        /// <summary>Janela usada para calcular o gasto médio mensal.</summary>
        private const int AVERAGE_EXPENSE_WINDOW_MONTHS = 6;

        /// <summary>Quantidade de categorias destacadas no painel.</summary>
        private const int TOP_CATEGORIES_LIMIT = 5;

        /// <summary>Quantidade de lançamentos recentes exibidos no painel.</summary>
        private const int RECENT_TRANSACTIONS_LIMIT = 5;

        /// <summary>Meses exibidos no gráfico de evolução.</summary>
        private const int EVOLUTION_WINDOW_MONTHS = 6;

        #endregion

        #region Fields

        private readonly ITransactionRepository transactionRepository = transactionRepository;
        private readonly ISavingsGoalRepository savingsGoalRepository = savingsGoalRepository;

        #endregion

        #region Helpers :: ResolvePeriod(), SumOf()

        /// <summary>
        /// Resolve o período da consulta, assumindo o mês atual quando não informado.
        /// </summary>
        /// <param name="from">Início solicitado.</param>
        /// <param name="to">Fim solicitado.</param>
        /// <returns>Início e fim normalizados em UTC.</returns>
        private static (DateTime Start, DateTime End) ResolvePeriod(DateTime? from, DateTime? to)
        {
            DateTime now = DateTime.UtcNow;

            DateTime start = from.HasValue
                ? DateTimeHelper.ToUtcDate(from.Value)
                : DateTimeHelper.StartOfMonth(now);

            DateTime end = to.HasValue
                ? DateTimeHelper.ToUtcEndOfDay(to.Value)
                : DateTimeHelper.EndOfMonth(now);

            return (start, end);
        }

        /// <summary>
        /// Extrai o total de uma natureza da lista de totais.
        /// </summary>
        /// <param name="totals">Totais apurados.</param>
        /// <param name="type">Natureza desejada.</param>
        /// <returns>Total da natureza, ou zero quando não houve movimento.</returns>
        private static decimal SumOf(IReadOnlyList<TypeTotal> totals, TransactionType type)
        {
            return totals.FirstOrDefault(x => x.Type == type)?.Total ?? 0;
        }

        #endregion

        #region Methods :: GetAsync()

        /// <inheritdoc />
        public async Task<DashboardResponse> GetAsync(Guid userId, DateTime? from, DateTime? to)
        {
            (DateTime start, DateTime end) = ResolvePeriod(from, to);

            IReadOnlyList<TypeTotal> typeTotals = await this.transactionRepository.GetTotalsByTypeAsync(userId, start, end);

            decimal totalIncome = SumOf(typeTotals, TransactionType.Income);
            decimal totalExpense = SumOf(typeTotals, TransactionType.Expense);

            IReadOnlyList<CategoryTotal> topExpenses = await this.transactionRepository
                .GetTotalsByCategoryAsync(userId, TransactionType.Expense, start, end, TOP_CATEGORIES_LIMIT);

            IReadOnlyList<CategoryTotal> topIncomes = await this.transactionRepository
                .GetTotalsByCategoryAsync(userId, TransactionType.Income, start, end, TOP_CATEGORIES_LIMIT);

            // A evolução ignora o período filtrado: a série só faz sentido como histórico dos
            // últimos meses, terminando no mês final do filtro.
            DateTime evolutionStart = DateTimeHelper.StartOfMonth(end).AddMonths(-(EVOLUTION_WINDOW_MONTHS - 1));
            DateTime evolutionEnd = DateTimeHelper.EndOfMonth(end);

            IReadOnlyList<MonthlyTotal> monthlyTotals = await this.transactionRepository
                .GetMonthlyTotalsAsync(userId, evolutionStart, evolutionEnd);

            IReadOnlyList<SavingsGoal> goals = await this.savingsGoalRepository.GetAllAsync(userId, includeArchived: false);
            IReadOnlyList<GoalBalance> balances = await this.savingsGoalRepository.GetBalancesAsync(userId);
            IReadOnlyList<Transaction> recent = await this.transactionRepository.GetRecentAsync(userId, RECENT_TRANSACTIONS_LIMIT);

            Dictionary<Guid, decimal> balanceByGoal = balances.ToDictionary(x => x.SavingsGoalId, x => x.Balance);

            return new DashboardResponse()
            {
                PeriodStart = start,
                PeriodEnd = end,
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                Balance = totalIncome - totalExpense,
                SavingsRate = totalIncome <= 0
                    ? 0
                    : Math.Round((totalIncome - totalExpense) / totalIncome * 100, 2),
                TotalSaved = balances.Sum(x => x.Balance),
                EmergencyReserve = await this.GetEmergencyReserveAsync(userId),
                TopExpenseCategories = topExpenses
                    .Select(item => new CategorySpendingResponse(item, totalExpense))
                    .ToList(),
                TopIncomeCategories = topIncomes
                    .Select(item => new CategorySpendingResponse(item, totalIncome))
                    .ToList(),
                MonthlyEvolution = BuildEvolution(monthlyTotals, evolutionStart),
                Goals = goals
                    .Select(goal => new SavingsGoalResponse(goal, balanceByGoal.GetValueOrDefault(goal.Id)))
                    .ToList(),
                RecentTransactions = recent
                    .Select(transaction => new TransactionResponse(transaction))
                    .ToList()
            };
        }

        #endregion

        #region Methods :: GetCategoryBreakdownAsync(), GetEmergencyReserveAsync()

        /// <inheritdoc />
        public async Task<IReadOnlyList<CategorySpendingResponse>> GetCategoryBreakdownAsync(Guid userId, TransactionType type, DateTime? from, DateTime? to)
        {
            (DateTime start, DateTime end) = ResolvePeriod(from, to);

            IReadOnlyList<CategoryTotal> totals = await this.transactionRepository
                .GetTotalsByCategoryAsync(userId, type, start, end, limit: null);

            decimal periodTotal = totals.Sum(x => x.Total);

            return totals
                .Select(item => new CategorySpendingResponse(item, periodTotal))
                .ToList();
        }

        /// <inheritdoc />
        public async Task<EmergencyReserveResponse> GetEmergencyReserveAsync(Guid userId)
        {
            decimal averageMonthlyExpense = await this.GetAverageMonthlyExpenseAsync(userId);
            decimal recommended = Math.Round(averageMonthlyExpense * EMERGENCY_RESERVE_MONTHS, 2);

            SavingsGoal? reserve = await this.savingsGoalRepository.GetEmergencyReserveAsync(userId);

            if (reserve is null)
            {
                return new EmergencyReserveResponse()
                {
                    Exists = false,
                    AverageMonthlyExpense = averageMonthlyExpense,
                    RecommendedAmount = recommended
                };
            }

            decimal balance = await this.savingsGoalRepository.GetBalanceAsync(userId, reserve.Id);

            return new EmergencyReserveResponse()
            {
                Exists = true,
                SavingsGoalId = reserve.Id,
                CurrentAmount = balance,
                TargetAmount = reserve.TargetAmount,
                RecommendedAmount = recommended,
                AverageMonthlyExpense = averageMonthlyExpense,
                MonthsCovered = averageMonthlyExpense <= 0 ? 0 : Math.Round(balance / averageMonthlyExpense, 1),
                ProgressPercentage = reserve.TargetAmount <= 0
                    ? 0
                    : Math.Round(Math.Min(100, balance / reserve.TargetAmount * 100), 2)
            };
        }

        #endregion

        #region Helpers :: GetAverageMonthlyExpenseAsync(), BuildEvolution()

        /// <summary>
        /// Calcula o gasto médio mensal na janela de análise.
        /// </summary>
        /// <remarks>
        /// A média divide pelos meses que tiveram movimento, não pelo tamanho fixo da janela:
        /// quem usa o sistema há duas semanas veria uma média artificialmente baixa e uma
        /// recomendação de reserva menor do que a realidade.
        /// </remarks>
        /// <param name="userId">Dono dos lançamentos.</param>
        /// <returns>Média de saídas por mês, ou zero sem histórico.</returns>
        private async Task<decimal> GetAverageMonthlyExpenseAsync(Guid userId)
        {
            DateTime now = DateTime.UtcNow;
            DateTime start = DateTimeHelper.StartOfMonth(now).AddMonths(-(AVERAGE_EXPENSE_WINDOW_MONTHS - 1));
            DateTime end = DateTimeHelper.EndOfMonth(now);

            IReadOnlyList<MonthlyTotal> totals = await this.transactionRepository.GetMonthlyTotalsAsync(userId, start, end);

            List<MonthlyTotal> expenses = totals
                .Where(x => x.Type == TransactionType.Expense && x.Total > 0)
                .ToList();

            if (expenses.Count == 0)
                return 0;

            return Math.Round(expenses.Sum(x => x.Total) / expenses.Count, 2);
        }

        /// <summary>
        /// Converte os totais mensais em série contínua, preenchendo meses sem movimento.
        /// </summary>
        /// <remarks>
        /// Um mês sem lançamento não vem do banco. Sem o preenchimento, o gráfico ligaria
        /// março a maio como se abril não existisse.
        /// </remarks>
        /// <param name="totals">Totais apurados por mês e natureza.</param>
        /// <param name="evolutionStart">Primeiro mês da série.</param>
        /// <returns>Série mensal completa em ordem cronológica.</returns>
        private static List<MonthlyEvolutionResponse> BuildEvolution(IReadOnlyList<MonthlyTotal> totals, DateTime evolutionStart)
        {
            List<MonthlyEvolutionResponse> evolution = new List<MonthlyEvolutionResponse>(EVOLUTION_WINDOW_MONTHS);

            for (int offset = 0; offset < EVOLUTION_WINDOW_MONTHS; offset++)
            {
                DateTime month = evolutionStart.AddMonths(offset);

                decimal income = totals
                    .FirstOrDefault(x => x.Year == month.Year && x.Month == month.Month && x.Type == TransactionType.Income)?.Total ?? 0;

                decimal expense = totals
                    .FirstOrDefault(x => x.Year == month.Year && x.Month == month.Month && x.Type == TransactionType.Expense)?.Total ?? 0;

                evolution.Add(new MonthlyEvolutionResponse(month.Year, month.Month, income, expense));
            }

            return evolution;
        }

        #endregion
    }
}
