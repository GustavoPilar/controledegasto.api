using ControleDeGasto.API.Application.DTOs;
using ControleDeGasto.API.Application.Interfaces;
using ControleDeGasto.API.Application.Utilities;
using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;
using ControleDeGasto.API.Domain.Interfaces;
using ControleDeGasto.API.Domain.Queries;
using ControleDeGasto.API.Domain.ReadModels;

namespace ControleDeGasto.API.Application.Services
{
    public class DashboardService(
        ITransactionRepository transactionRepository,
        ISavingsGoalRepository savingsGoalRepository,
        IFixedEntryRepository fixedEntryRepository,
        IWalletService walletService,
        ITagService tagService,
        IFriendshipRepository friendshipRepository) : IDashboardService
    {
        #region Constants :: EMERGENCY_RESERVE_MONTHS, AVERAGE_EXPENSE_WINDOW_MONTHS, TOP_CATEGORIES_LIMIT, RECENT_TRANSACTIONS_LIMIT, EVOLUTION_WINDOW_MONTHS, UPCOMING_WINDOW_DAYS, BILLS_LIMIT

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

        /// <summary>Janela de dias do bloco "contas a vencer".</summary>
        private const int UPCOMING_WINDOW_DAYS = 15;

        /// <summary>Quantidade de contas exibidas nos blocos de vencimento.</summary>
        private const int BILLS_LIMIT = 8;

        #endregion

        #region Fields

        private readonly ITransactionRepository transactionRepository = transactionRepository;
        private readonly ISavingsGoalRepository savingsGoalRepository = savingsGoalRepository;
        private readonly IFixedEntryRepository fixedEntryRepository = fixedEntryRepository;
        private readonly IWalletService walletService = walletService;
        private readonly ITagService tagService = tagService;
        private readonly IFriendshipRepository friendshipRepository = friendshipRepository;

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

            DateTime today = DateTimeHelper.ToUtcDate(DateTime.UtcNow);

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

            IReadOnlyList<Transaction> upcoming = await this.transactionRepository
                .GetUpcomingAsync(userId, today, today.AddDays(UPCOMING_WINDOW_DAYS), BILLS_LIMIT);

            IReadOnlyList<Transaction> overdue = await this.transactionRepository
                .GetOverdueAsync(userId, today, BILLS_LIMIT);

            IReadOnlyList<WalletResponse> wallets = await this.walletService.GetAllAsync(userId, includeInactive: false);

            IReadOnlyList<TagTotalResponse> tagTotals = await this.tagService.GetTotalsAsync(userId, start, end);

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
                    .Select(goal => new SavingsGoalResponse(goal, balanceByGoal.GetValueOrDefault(goal.Id), userId))
                    .ToList(),
                RecentTransactions = recent
                    .Select(transaction => new TransactionResponse(transaction, today))
                    .ToList(),

                Forecast = await this.GetForecastAsync(userId, start),

                UpcomingBills = upcoming
                    .Select(transaction => new TransactionResponse(transaction, today))
                    .ToList(),
                OverdueBills = overdue
                    .Select(transaction => new TransactionResponse(transaction, today))
                    .ToList(),

                // As carteiras de benefício vão em uma lista à parte: o saldo do vale não é
                // dinheiro livre, e somá-lo ao das contas inflaria o patrimônio exibido.
                Wallets = wallets.Where(wallet => !wallet.IsBenefit).ToList(),
                BenefitWallets = wallets.Where(wallet => wallet.IsBenefit).ToList(),

                Shared = await this.BuildSharedSummaryAsync(userId),
                TagTotals = tagTotals,
                OpenInstallmentPlans = await this.BuildOpenInstallmentPlansAsync(userId)
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

            decimal balance = await this.savingsGoalRepository.GetBalanceAsync(reserve.Id);

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

        #region Methods :: GetForecastAsync()

        /// <inheritdoc />
        public async Task<MonthlyForecastResponse> GetForecastAsync(Guid userId, DateTime? reference)
        {
            DateTime target = reference.HasValue ? DateTimeHelper.ToUtc(reference.Value) : DateTime.UtcNow;

            DateTime monthStart = DateTimeHelper.StartOfMonth(target);
            DateTime monthEnd = DateTimeHelper.EndOfMonth(target);
            DateTime today = DateTimeHelper.ToUtcDate(DateTime.UtcNow);

            IReadOnlyList<TypeTotal> settled = await this.transactionRepository.GetTotalsByTypeAsync(userId, monthStart, monthEnd);

            decimal settledIncome = SumOf(settled, TransactionType.Income);
            decimal settledExpense = SumOf(settled, TransactionType.Expense);

            IReadOnlyList<PendingTypeTotal> pending = await this.transactionRepository
                .GetPendingTotalsAsync(userId, monthStart, monthEnd, today);

            PendingTypeTotal? pendingIncome = pending.FirstOrDefault(item => item.Type == TransactionType.Income);
            PendingTypeTotal? pendingExpense = pending.FirstOrDefault(item => item.Type == TransactionType.Expense);

            IReadOnlyList<FixedEntry> fixedEntries = await this.fixedEntryRepository
                .GetActiveForMonthAsync(userId, monthStart, monthEnd);

            // O que já foi lançado por categoria abate a previsão fixa da mesma categoria. Sem
            // isso, quem registra o próprio aluguel veria a despesa contada duas vezes: uma pelo
            // lançamento e outra pela definição fixa.
            Dictionary<Guid, decimal> availableByCategory = await this.BuildRealizedByCategoryAsync(userId, monthStart, monthEnd);

            List<ForecastItemResponse> items = [];

            decimal fixedIncome = 0;
            decimal fixedExpense = 0;
            decimal remainingFixedIncome = 0;
            decimal remainingFixedExpense = 0;
            decimal benefitCredits = 0;

            foreach (FixedEntry entry in fixedEntries)
            {
                if (!FixedEntryHelper.AppliesToMonth(entry, monthStart, monthEnd))
                    continue;

                DateTime expectedOn = FixedEntryHelper.ResolveDateInMonth(entry.DayOfMonth, monthStart);

                if (entry.Kind == FixedEntryKind.BenefitCredit)
                {
                    benefitCredits += entry.Amount;

                    items.Add(BuildFixedItem(entry, expectedOn, entry.Amount, 0, TransactionType.Income, today, isBenefit: true));

                    continue;
                }

                bool isIncome = entry.Kind == FixedEntryKind.Income;

                if (isIncome)
                    fixedIncome += entry.Amount;
                else
                    fixedExpense += entry.Amount;

                decimal matched = 0;

                if (entry.CategoryId.HasValue && availableByCategory.TryGetValue(entry.CategoryId.Value, out decimal available))
                {
                    matched = Math.Min(entry.Amount, available);
                    availableByCategory[entry.CategoryId.Value] = available - matched;
                }

                decimal remaining = entry.Amount - matched;

                if (isIncome)
                    remainingFixedIncome += remaining;
                else
                    remainingFixedExpense += remaining;

                // O item entra na lista mesmo quando totalmente abatido: some da soma, mas
                // continua visível como "já lançado", que é a informação que evita o usuário
                // achar que esqueceu de registrar a conta.
                items.Add(BuildFixedItem(
                    entry,
                    expectedOn,
                    remaining,
                    matched,
                    isIncome ? TransactionType.Income : TransactionType.Expense,
                    today,
                    isBenefit: false));
            }

            IReadOnlyList<Transaction> pendingItems = await this.transactionRepository
                .GetUpcomingAsync(userId, monthStart, monthEnd, TransactionFilterRequest.MAX_PAGE_SIZE);

            items.AddRange(pendingItems.Select(transaction => new ForecastItemResponse()
            {
                Source = transaction.InstallmentPlanId.HasValue
                    ? ForecastSource.Installment
                    : ForecastSource.PendingTransaction,
                ReferenceId = transaction.Id,
                Description = transaction.Description,
                Amount = transaction.Amount,
                Type = transaction.Category?.Type ?? TransactionType.Expense,
                ExpectedOn = transaction.DueDate ?? transaction.OccurredOn,
                IsOverdue = (transaction.DueDate ?? transaction.OccurredOn) < today,
                CategoryId = transaction.CategoryId,
                CategoryName = transaction.Category?.Name,
                CategoryColor = transaction.Category?.Color,
                CategoryIcon = transaction.Category?.Icon,
                WalletId = transaction.WalletId,
                WalletName = transaction.Wallet?.Name,
                WalletColor = transaction.Wallet?.Color
            }));

            decimal projectedIncome = settledIncome + (pendingIncome?.Total ?? 0) + remainingFixedIncome;
            decimal projectedExpense = settledExpense + (pendingExpense?.Total ?? 0) + remainingFixedExpense;

            return new MonthlyForecastResponse()
            {
                Year = monthStart.Year,
                Month = monthStart.Month,
                PeriodStart = monthStart,
                PeriodEnd = monthEnd,

                SettledIncome = settledIncome,
                SettledExpense = settledExpense,
                SettledBalance = settledIncome - settledExpense,

                PendingIncome = pendingIncome?.Total ?? 0,
                PendingExpense = pendingExpense?.Total ?? 0,
                OverdueIncome = pendingIncome?.OverdueTotal ?? 0,
                OverdueExpense = pendingExpense?.OverdueTotal ?? 0,

                FixedIncome = fixedIncome,
                FixedExpense = fixedExpense,
                RemainingFixedIncome = remainingFixedIncome,
                RemainingFixedExpense = remainingFixedExpense,
                BenefitCredits = benefitCredits,

                ProjectedIncome = projectedIncome,
                ProjectedExpense = projectedExpense,
                ProjectedBalance = projectedIncome - projectedExpense,
                CommittedPercentage = projectedIncome <= 0
                    ? 0
                    : Math.Round(projectedExpense / projectedIncome * 100, 2),

                Items = items
                    .OrderBy(item => item.ExpectedOn)
                    .ThenBy(item => item.Description)
                    .ToList()
            };
        }

        #endregion

        #region Helpers :: BuildRealizedByCategoryAsync(), BuildFixedItem()

        /// <summary>
        /// Soma, por categoria, tudo o que o mês já tem de lançamento — liquidado ou previsto.
        /// </summary>
        /// <remarks>
        /// Inclui os previstos de propósito: uma conta fixa já cadastrada como conta a pagar
        /// aparece nos dois lugares, e contar as duas somaria a mesma despesa em dobro.
        /// </remarks>
        /// <param name="userId">Dono dos lançamentos.</param>
        /// <param name="monthStart">Primeiro instante do mês, em UTC.</param>
        /// <param name="monthEnd">Último instante do mês, em UTC.</param>
        /// <returns>Total lançado por categoria.</returns>
        private async Task<Dictionary<Guid, decimal>> BuildRealizedByCategoryAsync(Guid userId, DateTime monthStart, DateTime monthEnd)
        {
            IReadOnlyList<CategoryTotal> incomeTotals = await this.transactionRepository
                .GetTotalsByCategoryAsync(userId, TransactionType.Income, monthStart, monthEnd, limit: null);

            IReadOnlyList<CategoryTotal> expenseTotals = await this.transactionRepository
                .GetTotalsByCategoryAsync(userId, TransactionType.Expense, monthStart, monthEnd, limit: null);

            Dictionary<Guid, decimal> realized = [];

            foreach (CategoryTotal total in incomeTotals.Concat(expenseTotals))
                realized[total.CategoryId] = realized.GetValueOrDefault(total.CategoryId) + total.Total;

            IReadOnlyList<Transaction> pendingItems = await this.transactionRepository
                .GetUpcomingAsync(userId, monthStart, monthEnd, TransactionFilterRequest.MAX_PAGE_SIZE);

            foreach (Transaction transaction in pendingItems)
                realized[transaction.CategoryId] = realized.GetValueOrDefault(transaction.CategoryId) + transaction.Amount;

            return realized;
        }

        /// <summary>
        /// Converte uma definição fixa em item de previsão.
        /// </summary>
        /// <param name="entry">Definição de origem.</param>
        /// <param name="expectedOn">Data prevista no mês.</param>
        /// <param name="amount">Valor que ainda entra na soma.</param>
        /// <param name="alreadyRealized">Valor já coberto por lançamentos do mês.</param>
        /// <param name="type">Natureza a exibir.</param>
        /// <param name="today">Data de hoje, em UTC.</param>
        /// <param name="isBenefit">Indica crédito de benefício.</param>
        /// <returns>Item pronto para a resposta.</returns>
        private static ForecastItemResponse BuildFixedItem(
            FixedEntry entry,
            DateTime expectedOn,
            decimal amount,
            decimal alreadyRealized,
            TransactionType type,
            DateTime today,
            bool isBenefit)
        {
            return new ForecastItemResponse()
            {
                Source = ForecastSource.FixedEntry,
                ReferenceId = entry.Id,
                Description = entry.Description,
                Amount = amount,
                Type = type,
                IsBenefitCredit = isBenefit,
                ExpectedOn = expectedOn,

                // Um fixo já coberto por lançamento não está atrasado: o dinheiro passou, só
                // não por esta linha.
                IsOverdue = amount > 0 && expectedOn < today,
                AlreadyRealizedAmount = alreadyRealized,
                CategoryId = entry.CategoryId,
                CategoryName = entry.Category?.Name,
                CategoryColor = entry.Category?.Color,
                CategoryIcon = entry.Category?.Icon,
                WalletId = entry.WalletId,
                WalletName = entry.Wallet?.Name,
                WalletColor = entry.Wallet?.Color
            };
        }

        #endregion

        #region Helpers :: BuildSharedSummaryAsync(), BuildOpenInstallmentPlansAsync()

        /// <summary>
        /// Apura o resumo das divisões de compra em aberto.
        /// </summary>
        /// <param name="userId">Usuário de referência.</param>
        /// <returns>Totais a receber e a pagar.</returns>
        private async Task<SharedSummaryResponse> BuildSharedSummaryAsync(Guid userId)
        {
            IReadOnlyList<FriendBalance> balances = await this.transactionRepository.GetFriendBalancesAsync(userId);

            PagedResult<TransactionShare> openShares = await this.transactionRepository
                .GetSharedWithUserAsync(userId, onlyOpen: true, page: 1, pageSize: 1);

            decimal receivable = balances.Sum(item => item.Receivable);
            decimal payable = balances.Sum(item => item.Payable);

            return new SharedSummaryResponse()
            {
                Receivable = receivable,
                Payable = payable,
                NetBalance = receivable - payable,
                FriendCount = balances.Count(item => item.Receivable > 0 || item.Payable > 0),
                OpenShareCount = openShares.TotalCount
            };
        }

        /// <summary>
        /// Lista as compras parceladas com parcela em aberto.
        /// </summary>
        /// <param name="userId">Dono das compras.</param>
        /// <returns>Compras em andamento.</returns>
        private async Task<IReadOnlyList<InstallmentPlanResponse>> BuildOpenInstallmentPlansAsync(Guid userId)
        {
            IReadOnlyList<InstallmentPlan> plans = await this.transactionRepository
                .GetInstallmentPlansAsync(userId, onlyOpen: true);

            return plans
                .Select(plan => new InstallmentPlanResponse(plan))
                .ToList();
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
