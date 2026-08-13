using ControleDeGasto.API.Application.DTOs;
using ControleDeGasto.API.Application.Interfaces;
using ControleDeGasto.API.Application.Utilities;
using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;
using ControleDeGasto.API.Domain.Interfaces;
using ControleDeGasto.API.Domain.ReadModels;
using System.Globalization;

namespace ControleDeGasto.API.Application.Services
{
    public class NotificationService(
        INotificationRepository repository,
        ISavingsGoalRepository savingsGoalRepository,
        ITransactionRepository transactionRepository,
        IDashboardService dashboardService,
        IEmailSender emailSender,
        ILogger<NotificationService> logger) : INotificationService
    {
        #region Constants :: DEADLINE_WARNING_DAYS, SPENDING_INCREASE_FACTOR, MINIMUM_DIFFERENCE_TO_ALERT, RESERVE_LOW_FACTOR, DEDUPE_*

        /// <summary>Dias de antecedência do aviso de prazo de cofrinho.</summary>
        private const int DEADLINE_WARNING_DAYS = 7;

        /// <summary>Crescimento a partir do qual o gasto de uma categoria vira aviso.</summary>
        private const decimal SPENDING_INCREASE_FACTOR = 1.3m;

        /// <summary>
        /// Diferença mínima em dinheiro para avisar. Sem ela, sair de R$ 10 para R$ 14 geraria
        /// alerta por ser +40%, o que só produziria ruído.
        /// </summary>
        private const decimal MINIMUM_DIFFERENCE_TO_ALERT = 50m;

        /// <summary>Fração do valor recomendado abaixo da qual a reserva é considerada baixa.</summary>
        private const decimal RESERVE_LOW_FACTOR = 0.5m;

        private static readonly TimeSpan DEDUPE_DEADLINE = TimeSpan.FromDays(DEADLINE_WARNING_DAYS);
        private static readonly TimeSpan DEDUPE_RESERVE = TimeSpan.FromDays(30);
        private static readonly TimeSpan DEDUPE_SPENDING = TimeSpan.FromDays(15);
        private static readonly TimeSpan DEDUPE_BALANCE = TimeSpan.FromDays(15);

        #endregion

        #region Fields

        private readonly INotificationRepository repository = repository;
        private readonly ISavingsGoalRepository savingsGoalRepository = savingsGoalRepository;
        private readonly ITransactionRepository transactionRepository = transactionRepository;
        private readonly IDashboardService dashboardService = dashboardService;
        private readonly IEmailSender emailSender = emailSender;
        private readonly ILogger<NotificationService> logger = logger;

        private static readonly CultureInfo BRAZILIAN_CULTURE = CultureInfo.GetCultureInfo("pt-BR");

        #endregion

        #region Methods :: GetPagedAsync(), GetUnreadCountAsync(), MarkAsReadAsync(), MarkAllAsReadAsync()

        /// <inheritdoc />
        public async Task<PagedResponse<NotificationResponse>> GetPagedAsync(Guid userId, bool onlyUnread, int page, int pageSize)
        {
            PagedResult<Notification> result = await this.repository.GetPagedAsync(userId, onlyUnread, page, pageSize);

            List<NotificationResponse> items = result.Items
                .Select(notification => new NotificationResponse(notification))
                .ToList();

            return new PagedResponse<NotificationResponse>(items, result.TotalCount, page, pageSize);
        }

        /// <inheritdoc />
        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await this.repository.GetUnreadCountAsync(userId);
        }

        /// <inheritdoc />
        public async Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId)
        {
            return await this.repository.MarkAsReadAsync(userId, notificationId, DateTime.UtcNow);
        }

        /// <inheritdoc />
        public async Task<int> MarkAllAsReadAsync(Guid userId)
        {
            return await this.repository.MarkAllAsReadAsync(userId, DateTime.UtcNow);
        }

        #endregion

        #region Methods :: CreateAsync()

        /// <inheritdoc />
        public async Task<bool> CreateAsync(Guid userId, NotificationType type, string title, string message, Guid? referenceId, TimeSpan? dedupeWindow)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(title);
            ArgumentException.ThrowIfNullOrWhiteSpace(message);

            // Janela nula significa "uma vez na vida": a busca varre todo o histórico.
            DateTime since = dedupeWindow.HasValue
                ? DateTime.UtcNow.Subtract(dedupeWindow.Value)
                : DateTime.UnixEpoch;

            bool alreadyExists = await this.repository.ExistsRecentAsync(userId, type, referenceId, since);

            if (alreadyExists)
                return false;

            Notification notification = new Notification()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = type,
                Title = title,
                Message = message,
                ReferenceId = referenceId,
                CreatedAt = DateTime.UtcNow
            };

            bool created = await this.repository.CreateAsync(notification);

            if (created)
                this.logger.LogInformation("Notificação {Type} criada para o usuário {UserId}.", type, userId);

            return created;
        }

        #endregion

        #region Methods :: EvaluateForUserAsync()

        /// <inheritdoc />
        public async Task<int> EvaluateForUserAsync(Guid userId)
        {
            int created = 0;

            created += await this.EvaluateGoalsAsync(userId);
            created += await this.EvaluateEmergencyReserveAsync(userId);
            created += await this.EvaluateCategorySpendingAsync(userId);
            created += await this.EvaluateMonthlyBalanceAsync(userId);

            return created;
        }

        #endregion

        #region Helpers :: EvaluateGoalsAsync(), EvaluateEmergencyReserveAsync(), EvaluateCategorySpendingAsync(), EvaluateMonthlyBalanceAsync()

        /// <summary>
        /// Avisa sobre metas atingidas e prazos próximos.
        /// </summary>
        /// <param name="userId">Usuário a avaliar.</param>
        /// <returns>Quantidade de notificações criadas.</returns>
        private async Task<int> EvaluateGoalsAsync(Guid userId)
        {
            IReadOnlyList<SavingsGoal> goals = await this.savingsGoalRepository.GetAllAsync(userId, includeArchived: false);

            if (goals.Count == 0)
                return 0;

            IReadOnlyList<GoalBalance> balances = await this.savingsGoalRepository.GetBalancesAsync(userId);
            Dictionary<Guid, decimal> balanceByGoal = balances.ToDictionary(x => x.SavingsGoalId, x => x.Balance);

            int created = 0;
            DateTime today = DateTimeHelper.ToUtcDate(DateTime.UtcNow);

            foreach (SavingsGoal goal in goals)
            {
                decimal balance = balanceByGoal.GetValueOrDefault(goal.Id);

                if (balance >= goal.TargetAmount)
                {
                    bool notified = await this.CreateAsync(
                        userId,
                        NotificationType.GoalAchieved,
                        goal.IsEmergencyReserve ? "Reserva de emergência completa!" : $"Meta \"{goal.Name}\" atingida!",
                        $"Você alcançou {FormatMoney(goal.TargetAmount)} em \"{goal.Name}\".",
                        goal.Id,
                        dedupeWindow: null);

                    if (notified)
                        created++;

                    continue;
                }

                if (!goal.Deadline.HasValue)
                    continue;

                int daysRemaining = (int)Math.Ceiling((goal.Deadline.Value.Date - today).TotalDays);

                if (daysRemaining is < 0 or > DEADLINE_WARNING_DAYS)
                    continue;

                decimal missing = goal.TargetAmount - balance;

                bool deadlineNotified = await this.CreateAsync(
                    userId,
                    NotificationType.GoalDeadlineNear,
                    $"O prazo de \"{goal.Name}\" está chegando",
                    $"Faltam {daysRemaining} dia(s) e ainda {FormatMoney(missing)} para concluir \"{goal.Name}\".",
                    goal.Id,
                    DEDUPE_DEADLINE);

                if (deadlineNotified)
                    created++;
            }

            return created;
        }

        /// <summary>
        /// Avisa quando a reserva de emergência não existe ou está abaixo do recomendado.
        /// </summary>
        /// <param name="userId">Usuário a avaliar.</param>
        /// <returns>Quantidade de notificações criadas.</returns>
        private async Task<int> EvaluateEmergencyReserveAsync(Guid userId)
        {
            EmergencyReserveResponse reserve = await this.dashboardService.GetEmergencyReserveAsync(userId);

            // Sem histórico de gastos não há recomendação possível, então não há o que avisar.
            if (reserve.RecommendedAmount <= 0)
                return 0;

            if (!reserve.Exists)
            {
                bool createdReserveAlert = await this.CreateAsync(
                    userId,
                    NotificationType.EmergencyReserveLow,
                    "Você ainda não tem reserva de emergência",
                    $"Com seus gastos atuais, o recomendado é guardar {FormatMoney(reserve.RecommendedAmount)} " +
                    $"({DashboardService.EMERGENCY_RESERVE_MONTHS} meses de despesas). Crie um cofrinho de reserva para começar.",
                    referenceId: null,
                    DEDUPE_RESERVE);

                return createdReserveAlert ? 1 : 0;
            }

            if (reserve.CurrentAmount >= reserve.RecommendedAmount * RESERVE_LOW_FACTOR)
                return 0;

            bool notified = await this.CreateAsync(
                userId,
                NotificationType.EmergencyReserveLow,
                "Sua reserva de emergência está baixa",
                $"Você tem {FormatMoney(reserve.CurrentAmount)} guardados e o recomendado é " +
                $"{FormatMoney(reserve.RecommendedAmount)}, o equivalente a {DashboardService.EMERGENCY_RESERVE_MONTHS} meses de despesas.",
                reserve.SavingsGoalId,
                DEDUPE_RESERVE);

            return notified ? 1 : 0;
        }

        /// <summary>
        /// Avisa quando o gasto de uma categoria cresce de forma relevante em relação ao mês anterior.
        /// </summary>
        /// <param name="userId">Usuário a avaliar.</param>
        /// <returns>Quantidade de notificações criadas.</returns>
        private async Task<int> EvaluateCategorySpendingAsync(Guid userId)
        {
            DateTime now = DateTime.UtcNow;

            DateTime currentStart = DateTimeHelper.StartOfMonth(now);
            DateTime currentEnd = DateTimeHelper.EndOfMonth(now);
            DateTime previousStart = currentStart.AddMonths(-1);
            DateTime previousEnd = DateTimeHelper.EndOfMonth(previousStart);

            IReadOnlyList<CategoryTotal> current = await this.transactionRepository
                .GetTotalsByCategoryAsync(userId, TransactionType.Expense, currentStart, currentEnd, limit: null);

            if (current.Count == 0)
                return 0;

            IReadOnlyList<CategoryTotal> previous = await this.transactionRepository
                .GetTotalsByCategoryAsync(userId, TransactionType.Expense, previousStart, previousEnd, limit: null);

            Dictionary<Guid, decimal> previousByCategory = previous.ToDictionary(x => x.CategoryId, x => x.Total);

            int created = 0;

            foreach (CategoryTotal category in current)
            {
                decimal previousTotal = previousByCategory.GetValueOrDefault(category.CategoryId);

                if (previousTotal <= 0)
                    continue;

                decimal difference = category.Total - previousTotal;

                if (category.Total < previousTotal * SPENDING_INCREASE_FACTOR || difference < MINIMUM_DIFFERENCE_TO_ALERT)
                    continue;

                decimal increasePercentage = Math.Round(difference / previousTotal * 100, 0);

                bool notified = await this.CreateAsync(
                    userId,
                    NotificationType.HighCategorySpending,
                    $"Gasto com {category.CategoryName} subiu {increasePercentage}%",
                    $"Neste mês você gastou {FormatMoney(category.Total)} em {category.CategoryName}, " +
                    $"contra {FormatMoney(previousTotal)} no mês anterior.",
                    category.CategoryId,
                    DEDUPE_SPENDING);

                if (notified)
                    created++;
            }

            return created;
        }

        /// <summary>
        /// Avisa quando o mês fecha com saldo negativo.
        /// </summary>
        /// <param name="userId">Usuário a avaliar.</param>
        /// <returns>Quantidade de notificações criadas.</returns>
        private async Task<int> EvaluateMonthlyBalanceAsync(Guid userId)
        {
            DateTime now = DateTime.UtcNow;

            IReadOnlyList<TypeTotal> totals = await this.transactionRepository
                .GetTotalsByTypeAsync(userId, DateTimeHelper.StartOfMonth(now), DateTimeHelper.EndOfMonth(now));

            decimal income = totals.FirstOrDefault(x => x.Type == TransactionType.Income)?.Total ?? 0;
            decimal expense = totals.FirstOrDefault(x => x.Type == TransactionType.Expense)?.Total ?? 0;

            // Sem entradas registradas o saldo negativo não significa nada: provavelmente o
            // usuário ainda não lançou o salário do mês.
            if (income <= 0 || expense <= income)
                return 0;

            bool notified = await this.CreateAsync(
                userId,
                NotificationType.NegativeMonthlyBalance,
                "Suas saídas passaram as entradas neste mês",
                $"Entradas de {FormatMoney(income)} contra saídas de {FormatMoney(expense)}. " +
                $"O mês está {FormatMoney(expense - income)} no vermelho.",
                referenceId: null,
                DEDUPE_BALANCE);

            return notified ? 1 : 0;
        }

        #endregion

        #region Methods :: ProcessPendingEmailsAsync()

        /// <inheritdoc />
        public async Task<int> ProcessPendingEmailsAsync(int maxCount)
        {
            IReadOnlyList<PendingEmailNotification> pending = await this.repository.GetPendingEmailAsync(maxCount);

            if (pending.Count == 0)
                return 0;

            int sent = 0;

            foreach (PendingEmailNotification notification in pending)
            {
                try
                {
                    await this.emailSender.SendEmailAsync(
                        notification.Email,
                        notification.Title,
                        BuildEmailBody(notification));

                    await this.repository.MarkEmailSentAsync(notification.NotificationId, DateTime.UtcNow);

                    sent++;
                }
                catch (Exception ex)
                {
                    // Uma falha de envio não interrompe a fila: a notificação continua pendente
                    // e será tentada de novo na próxima execução.
                    this.logger.LogError(
                        ex,
                        "Falha ao enviar e-mail da notificação {NotificationId} para o usuário {UserId}.",
                        notification.NotificationId,
                        notification.UserId);
                }
            }

            this.logger.LogInformation("{Sent} de {Total} e-mails de notificação enviados.", sent, pending.Count);

            return sent;
        }

        #endregion

        #region Helpers :: BuildEmailBody(), FormatMoney()

        /// <summary>
        /// Monta o corpo HTML do e-mail de notificação.
        /// </summary>
        /// <param name="notification">Notificação pendente de envio.</param>
        /// <returns>HTML com estilos embutidos, exigência dos clientes de e-mail.</returns>
        private static string BuildEmailBody(PendingEmailNotification notification)
        {
            string firstName = notification.FullName.Split(' ').FirstOrDefault() ?? notification.FullName;

            return $"""
                <div style="font-family: Arial, Helvetica, sans-serif; max-width: 560px; margin: 0 auto; color: #33280F;">
                    <div style="background: #C5A44E; padding: 16px 24px; border-radius: 12px 12px 0 0;">
                        <h2 style="margin: 0; color: #ffffff;">Controle de Gasto</h2>
                    </div>
                    <div style="border: 1px solid #E9DFBF; border-top: none; padding: 24px; border-radius: 0 0 12px 12px;">
                        <p style="margin-top: 0;">Olá, {firstName}!</p>
                        <h3 style="color: #8F6F26;">{notification.Title}</h3>
                        <p style="line-height: 1.5;">{notification.Message}</p>
                        <p style="color: #72591F; font-size: 12px; margin-bottom: 0;">
                            Você recebeu este aviso porque tem uma conta no Controle de Gasto.
                        </p>
                    </div>
                </div>
                """;
        }

        /// <summary>
        /// Formata um valor como moeda brasileira.
        /// </summary>
        /// <remarks>
        /// A cultura é fixada em pt-BR porque o texto do aviso é sempre em português e não
        /// deve variar com a configuração do servidor.
        /// </remarks>
        /// <param name="value">Valor a formatar.</param>
        /// <returns>Valor formatado, por exemplo R$ 1.234,56.</returns>
        private static string FormatMoney(decimal value)
        {
            return value.ToString("C2", BRAZILIAN_CULTURE);
        }

        #endregion
    }
}
