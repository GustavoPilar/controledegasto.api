using ControleDeGasto.API.Application.Configuration;
using ControleDeGasto.API.Application.Interfaces;
using ControleDeGasto.API.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace ControleDeGasto.API.Infra.BackgroundServices
{
    /// <summary>
    /// Rotina periódica que avalia as regras de aviso de cada usuário e envia por e-mail as
    /// notificações pendentes.
    /// </summary>
    /// <remarks>
    /// Cada ciclo abre o próprio escopo de injeção: os serviços e o DbContext têm tempo de vida
    /// por requisição e não podem ser capturados por um serviço singleton.
    /// </remarks>
    public class NotificationWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<NotificationSettings> settings,
        ILogger<NotificationWorker> logger) : BackgroundService
    {
        #region Fields

        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly NotificationSettings settings = settings.Value;
        private readonly ILogger<NotificationWorker> logger = logger;

        #endregion

        #region Methods :: ExecuteAsync()

        /// <summary>
        /// Laço principal da rotina.
        /// </summary>
        /// <param name="stoppingToken">Sinal de desligamento da aplicação.</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!this.settings.Enabled)
            {
                this.logger.LogInformation("Rotina de notificações desligada por configuração.");
                return;
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(this.settings.StartupDelaySeconds), stoppingToken);

                using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromHours(this.settings.IntervalHours));

                do
                {
                    await this.RunCycleAsync(stoppingToken);
                }
                while (await timer.WaitForNextTickAsync(stoppingToken));
            }
            catch (OperationCanceledException)
            {
                // Desligamento normal da aplicação: não é erro.
                this.logger.LogInformation("Rotina de notificações encerrada.");
            }
        }

        #endregion

        #region Helpers :: RunCycleAsync()

        /// <summary>
        /// Executa um ciclo completo: avalia regras de todos os usuários e envia os e-mails.
        /// </summary>
        /// <param name="stoppingToken">Sinal de desligamento da aplicação.</param>
        private async Task RunCycleAsync(CancellationToken stoppingToken)
        {
            try
            {
                using IServiceScope scope = this.scopeFactory.CreateScope();

                IUserRepository userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                INotificationService notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                IReadOnlyList<Guid> userIds = await userRepository.GetActiveUserIdsAsync();

                int created = 0;

                foreach (Guid userId in userIds)
                {
                    if (stoppingToken.IsCancellationRequested)
                        return;

                    try
                    {
                        created += await notificationService.EvaluateForUserAsync(userId);
                    }
                    catch (Exception ex)
                    {
                        // A falha de um usuário não interrompe a avaliação dos demais.
                        this.logger.LogError(ex, "Falha ao avaliar notificações do usuário {UserId}.", userId);
                    }
                }

                int sent = await notificationService.ProcessPendingEmailsAsync(this.settings.EmailBatchSize);

                this.logger.LogInformation(
                    "Ciclo de notificações concluído: {Users} usuários avaliados, {Created} avisos criados, {Sent} e-mails enviados.",
                    userIds.Count,
                    created,
                    sent);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Um ciclo com falha não derruba a rotina: o próximo intervalo tenta de novo.
                this.logger.LogError(ex, "Falha no ciclo da rotina de notificações.");
            }
        }

        #endregion
    }
}
