using ControleDeGasto.API.Application.DTOs;
using ControleDeGasto.API.Application.Interfaces;
using ControleDeGasto.API.Application.Utilities;
using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;
using ControleDeGasto.API.Domain.Exceptions;
using ControleDeGasto.API.Domain.Interfaces;
using ControleDeGasto.API.Domain.ReadModels;

namespace ControleDeGasto.API.Application.Services
{
    public class SavingsGoalService(
        ISavingsGoalRepository repository,
        INotificationService notificationService,
        ILogger<SavingsGoalService> logger) : ISavingsGoalService
    {
        #region Fields

        private readonly ISavingsGoalRepository repository = repository;
        private readonly INotificationService notificationService = notificationService;
        private readonly ILogger<SavingsGoalService> logger = logger;

        #endregion

        #region Helpers :: BuildResponseAsync(), EnsureNameIsFreeAsync(), EnsureSingleEmergencyReserveAsync()

        /// <summary>
        /// Monta a resposta de um cofrinho buscando o saldo atual.
        /// </summary>
        /// <param name="savingsGoal">Cofrinho de origem.</param>
        /// <returns>Resposta com saldo e progresso.</returns>
        private async Task<SavingsGoalResponse> BuildResponseAsync(SavingsGoal savingsGoal)
        {
            decimal balance = await this.repository.GetBalanceAsync(savingsGoal.UserId, savingsGoal.Id);

            return new SavingsGoalResponse(savingsGoal, balance);
        }

        /// <summary>
        /// Garante que o nome do cofrinho não está em uso.
        /// </summary>
        /// <param name="userId">Dono do cofrinho.</param>
        /// <param name="name">Nome desejado.</param>
        /// <param name="excludeSavingsGoalId">Cofrinho a ignorar (usado na edição).</param>
        /// <exception cref="BusinessRuleViolationException">Nome já usado.</exception>
        private async Task EnsureNameIsFreeAsync(Guid userId, string name, Guid? excludeSavingsGoalId)
        {
            bool exists = await this.repository.ExistsByNameAsync(userId, name, excludeSavingsGoalId);

            if (exists)
                throw new BusinessRuleViolationException("Já existe um cofrinho com esse nome.");
        }

        /// <summary>
        /// Garante que o usuário terá no máximo uma reserva de emergência.
        /// </summary>
        /// <param name="userId">Dono do cofrinho.</param>
        /// <param name="excludeSavingsGoalId">Cofrinho a ignorar (usado na edição).</param>
        /// <exception cref="BusinessRuleViolationException">Já existe uma reserva.</exception>
        private async Task EnsureSingleEmergencyReserveAsync(Guid userId, Guid? excludeSavingsGoalId)
        {
            SavingsGoal? reserve = await this.repository.GetEmergencyReserveAsync(userId);

            if (reserve is null)
                return;

            if (excludeSavingsGoalId.HasValue && reserve.Id == excludeSavingsGoalId.Value)
                return;

            throw new BusinessRuleViolationException("Você já possui uma reserva de emergência.");
        }

        #endregion

        #region Methods :: GetAllAsync(), GetByIdAsync()

        /// <inheritdoc />
        public async Task<IReadOnlyList<SavingsGoalResponse>> GetAllAsync(Guid userId, bool includeArchived)
        {
            IReadOnlyList<SavingsGoal> goals = await this.repository.GetAllAsync(userId, includeArchived);

            if (goals.Count == 0)
                return [];

            // Uma consulta agrupada para todos os saldos, em vez de uma por cofrinho.
            IReadOnlyList<GoalBalance> balances = await this.repository.GetBalancesAsync(userId);

            Dictionary<Guid, decimal> balanceByGoal = balances.ToDictionary(x => x.SavingsGoalId, x => x.Balance);

            return goals
                .Select(goal => new SavingsGoalResponse(goal, balanceByGoal.GetValueOrDefault(goal.Id)))
                .ToList();
        }

        /// <inheritdoc />
        public async Task<SavingsGoalResponse?> GetByIdAsync(Guid userId, Guid savingsGoalId)
        {
            SavingsGoal? savingsGoal = await this.repository.GetByIdAsync(userId, savingsGoalId);

            return savingsGoal is null ? null : await this.BuildResponseAsync(savingsGoal);
        }

        #endregion

        #region Methods :: CreateAsync(), UpdateAsync(), SetStatusAsync(), DeleteAsync()

        /// <inheritdoc />
        public async Task<SavingsGoalResponse> CreateAsync(Guid userId, SavingsGoalRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            string name = request.Name.Trim();

            await this.EnsureNameIsFreeAsync(userId, name, null);

            if (request.IsEmergencyReserve)
                await this.EnsureSingleEmergencyReserveAsync(userId, null);

            DateTime? deadline = request.Deadline.HasValue
                ? DateTimeHelper.ToUtcDate(request.Deadline.Value)
                : null;

            if (deadline.HasValue && deadline.Value < DateTimeHelper.ToUtcDate(DateTime.UtcNow))
                throw new BusinessRuleViolationException("O prazo não pode estar no passado.");

            SavingsGoal savingsGoal = new SavingsGoal()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = name,
                TargetAmount = request.TargetAmount,
                Deadline = deadline,
                Color = request.Color.ToUpperInvariant(),
                Icon = request.Icon.Trim(),
                Status = SavingsGoalStatus.Active,
                IsEmergencyReserve = request.IsEmergencyReserve,
                CreatedAt = DateTime.UtcNow
            };

            bool created = await this.repository.CreateAsync(savingsGoal);

            if (!created)
                throw new BusinessRuleViolationException("Não foi possível criar o cofrinho.");

            this.logger.LogInformation("Cofrinho {SavingsGoalId} criado para o usuário {UserId}.", savingsGoal.Id, userId);

            return new SavingsGoalResponse(savingsGoal, 0);
        }

        /// <inheritdoc />
        public async Task<SavingsGoalResponse?> UpdateAsync(Guid userId, Guid savingsGoalId, SavingsGoalRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            SavingsGoal? savingsGoal = await this.repository.GetByIdAsync(userId, savingsGoalId);

            if (savingsGoal is null)
                return null;

            string name = request.Name.Trim();

            await this.EnsureNameIsFreeAsync(userId, name, savingsGoalId);

            if (request.IsEmergencyReserve && !savingsGoal.IsEmergencyReserve)
                await this.EnsureSingleEmergencyReserveAsync(userId, savingsGoalId);

            savingsGoal.Name = name;
            savingsGoal.TargetAmount = request.TargetAmount;
            savingsGoal.Deadline = request.Deadline.HasValue ? DateTimeHelper.ToUtcDate(request.Deadline.Value) : null;
            savingsGoal.Color = request.Color.ToUpperInvariant();
            savingsGoal.Icon = request.Icon.Trim();
            savingsGoal.IsEmergencyReserve = request.IsEmergencyReserve;
            savingsGoal.UpdatedAt = DateTime.UtcNow;

            // A meta pode ter subido ou descido: a situação é reavaliada contra o saldo real.
            decimal balance = await this.repository.GetBalanceAsync(userId, savingsGoalId);

            this.ApplyCompletionState(savingsGoal, balance);

            bool updated = await this.repository.UpdateAsync(savingsGoal);

            if (!updated)
                throw new BusinessRuleViolationException("Não foi possível atualizar o cofrinho.");

            this.logger.LogInformation("Cofrinho {SavingsGoalId} atualizado pelo usuário {UserId}.", savingsGoalId, userId);

            return new SavingsGoalResponse(savingsGoal, balance);
        }

        /// <inheritdoc />
        public async Task<SavingsGoalResponse?> SetStatusAsync(Guid userId, Guid savingsGoalId, SavingsGoalStatus status)
        {
            SavingsGoal? savingsGoal = await this.repository.GetByIdAsync(userId, savingsGoalId);

            if (savingsGoal is null)
                return null;

            savingsGoal.Status = status;
            savingsGoal.UpdatedAt = DateTime.UtcNow;

            if (status != SavingsGoalStatus.Completed)
                savingsGoal.CompletedAt = null;

            bool updated = await this.repository.UpdateAsync(savingsGoal);

            if (!updated)
                throw new BusinessRuleViolationException("Não foi possível alterar a situação do cofrinho.");

            return await this.BuildResponseAsync(savingsGoal);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(Guid userId, Guid savingsGoalId)
        {
            SavingsGoal? savingsGoal = await this.repository.GetByIdAsync(userId, savingsGoalId);

            if (savingsGoal is null)
                return false;

            bool deleted = await this.repository.DeleteAsync(savingsGoal);

            if (deleted)
                this.logger.LogInformation("Cofrinho {SavingsGoalId} removido pelo usuário {UserId}.", savingsGoalId, userId);

            return deleted;
        }

        #endregion

        #region Methods :: GetContributionsAsync(), AddContributionAsync(), DeleteContributionAsync()

        /// <inheritdoc />
        public async Task<IReadOnlyList<ContributionResponse>?> GetContributionsAsync(Guid userId, Guid savingsGoalId, int limit)
        {
            SavingsGoal? savingsGoal = await this.repository.GetByIdAsync(userId, savingsGoalId);

            if (savingsGoal is null)
                return null;

            IReadOnlyList<SavingsGoalContribution> contributions = await this.repository.GetContributionsAsync(userId, savingsGoalId, limit);

            return contributions.Select(contribution => new ContributionResponse(contribution)).ToList();
        }

        /// <inheritdoc />
        public async Task<SavingsGoalResponse?> AddContributionAsync(Guid userId, Guid savingsGoalId, ContributionRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            SavingsGoal? savingsGoal = await this.repository.GetByIdAsync(userId, savingsGoalId);

            if (savingsGoal is null)
                return null;

            if (savingsGoal.Status == SavingsGoalStatus.Archived)
                throw new BusinessRuleViolationException("Não é possível movimentar um cofrinho arquivado.");

            decimal balance = await this.repository.GetBalanceAsync(userId, savingsGoalId);

            if (request.Kind == ContributionKind.Withdrawal && request.Amount > balance)
                throw new BusinessRuleViolationException("O resgate é maior que o saldo do cofrinho.");

            SavingsGoalContribution contribution = new SavingsGoalContribution()
            {
                Id = Guid.NewGuid(),
                SavingsGoalId = savingsGoalId,
                UserId = userId,
                Amount = request.Amount,
                Kind = request.Kind,
                OccurredOn = DateTimeHelper.ToUtcDate(request.OccurredOn),
                Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            bool created = await this.repository.CreateContributionAsync(contribution);

            if (!created)
                throw new BusinessRuleViolationException("Não foi possível registrar o movimento.");

            decimal newBalance = request.Kind == ContributionKind.Deposit
                ? balance + request.Amount
                : balance - request.Amount;

            bool reachedNow = this.ApplyCompletionState(savingsGoal, newBalance);

            savingsGoal.UpdatedAt = DateTime.UtcNow;
            await this.repository.UpdateAsync(savingsGoal);

            if (reachedNow)
                await this.NotifyGoalAchievedAsync(savingsGoal);

            this.logger.LogInformation(
                "Movimento {Kind} de {Amount} registrado no cofrinho {SavingsGoalId} do usuário {UserId}.",
                contribution.Kind,
                contribution.Amount,
                savingsGoalId,
                userId);

            return new SavingsGoalResponse(savingsGoal, newBalance);
        }

        /// <inheritdoc />
        public async Task<SavingsGoalResponse?> DeleteContributionAsync(Guid userId, Guid contributionId)
        {
            SavingsGoalContribution? contribution = await this.repository.GetContributionByIdAsync(userId, contributionId);

            if (contribution is null)
                return null;

            SavingsGoal? savingsGoal = await this.repository.GetByIdAsync(userId, contribution.SavingsGoalId);

            if (savingsGoal is null)
                return null;

            decimal balance = await this.repository.GetBalanceAsync(userId, contribution.SavingsGoalId);

            // Remover um depósito reduz o saldo; se isso o deixaria negativo, existe um resgate
            // que dependia desse dinheiro e a remoção não pode acontecer.
            decimal balanceAfter = contribution.Kind == ContributionKind.Deposit
                ? balance - contribution.Amount
                : balance + contribution.Amount;

            if (balanceAfter < 0)
                throw new BusinessRuleViolationException("Remover este movimento deixaria o cofrinho com saldo negativo.");

            bool deleted = await this.repository.DeleteContributionAsync(contribution);

            if (!deleted)
                throw new BusinessRuleViolationException("Não foi possível remover o movimento.");

            this.ApplyCompletionState(savingsGoal, balanceAfter);

            savingsGoal.UpdatedAt = DateTime.UtcNow;
            await this.repository.UpdateAsync(savingsGoal);

            return new SavingsGoalResponse(savingsGoal, balanceAfter);
        }

        #endregion

        #region Helpers :: ApplyCompletionState(), NotifyGoalAchievedAsync()

        /// <summary>
        /// Sincroniza a situação do cofrinho com o saldo apurado.
        /// </summary>
        /// <param name="savingsGoal">Cofrinho a ajustar.</param>
        /// <param name="balance">Saldo atual.</param>
        /// <returns>True quando a meta passou a estar atingida nesta chamada.</returns>
        private bool ApplyCompletionState(SavingsGoal savingsGoal, decimal balance)
        {
            bool reachedTarget = balance >= savingsGoal.TargetAmount;

            if (savingsGoal.Status == SavingsGoalStatus.Archived)
                return false;

            if (reachedTarget && savingsGoal.Status != SavingsGoalStatus.Completed)
            {
                savingsGoal.Status = SavingsGoalStatus.Completed;
                savingsGoal.CompletedAt = DateTime.UtcNow;

                return true;
            }

            // Um resgate pode tirar o cofrinho da meta: a situação volta a ativa para o
            // progresso deixar de aparecer como concluído.
            if (!reachedTarget && savingsGoal.Status == SavingsGoalStatus.Completed)
            {
                savingsGoal.Status = SavingsGoalStatus.Active;
                savingsGoal.CompletedAt = null;
            }

            return false;
        }

        /// <summary>
        /// Cria a notificação de meta atingida.
        /// </summary>
        /// <param name="savingsGoal">Cofrinho que atingiu a meta.</param>
        private async Task NotifyGoalAchievedAsync(SavingsGoal savingsGoal)
        {
            string title = savingsGoal.IsEmergencyReserve
                ? "Reserva de emergência completa!"
                : $"Meta do cofrinho \"{savingsGoal.Name}\" atingida!";

            string message = $"Você alcançou {savingsGoal.TargetAmount:C2} em \"{savingsGoal.Name}\". Parabéns pela disciplina!";

            await this.notificationService.CreateAsync(
                savingsGoal.UserId,
                NotificationType.GoalAchieved,
                title,
                message,
                savingsGoal.Id,
                dedupeWindow: null);
        }

        #endregion
    }
}
