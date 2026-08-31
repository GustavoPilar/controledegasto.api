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
        IFriendshipRepository friendshipRepository,
        INotificationService notificationService,
        ILogger<SavingsGoalService> logger) : ISavingsGoalService
    {
        #region Fields

        private readonly ISavingsGoalRepository repository = repository;
        private readonly IFriendshipRepository friendshipRepository = friendshipRepository;
        private readonly INotificationService notificationService = notificationService;
        private readonly ILogger<SavingsGoalService> logger = logger;

        #endregion

        #region Helpers :: BuildResponseAsync(), EnsureOwner(), EnsureNameIsFreeAsync(), EnsureSingleEmergencyReserveAsync()

        /// <summary>
        /// Monta a resposta de um cofrinho buscando o saldo e o aporte de cada participante.
        /// </summary>
        /// <param name="savingsGoal">Cofrinho de origem.</param>
        /// <param name="currentUserId">Usuário que consulta.</param>
        /// <returns>Resposta com saldo, progresso e participantes.</returns>
        private async Task<SavingsGoalResponse> BuildResponseAsync(SavingsGoal savingsGoal, Guid currentUserId)
        {
            decimal balance = await this.repository.GetBalanceAsync(savingsGoal.Id);

            IReadOnlyDictionary<Guid, decimal> memberBalances = await this.repository.GetMemberBalancesAsync(savingsGoal.Id);

            return new SavingsGoalResponse(savingsGoal, balance, currentUserId, memberBalances);
        }

        /// <summary>
        /// Garante que quem está agindo é o criador do cofrinho.
        /// </summary>
        /// <remarks>
        /// Editar, convidar, arquivar e excluir mexem na configuração do cofrinho: liberar isso
        /// para qualquer participante permitiria que um convidado arquivasse o cofrinho de outra
        /// pessoa ou trocasse a meta que ela definiu.
        /// </remarks>
        /// <param name="savingsGoal">Cofrinho alvo.</param>
        /// <param name="userId">Usuário que está agindo.</param>
        /// <exception cref="BusinessRuleViolationException">O usuário não é o criador.</exception>
        private static void EnsureOwner(SavingsGoal savingsGoal, Guid userId)
        {
            if (savingsGoal.UserId != userId)
                throw new BusinessRuleViolationException("Apenas quem criou o cofrinho pode fazer essa alteração.");
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

            List<SavingsGoalResponse> responses = new List<SavingsGoalResponse>(goals.Count);

            foreach (SavingsGoal goal in goals)
            {
                // O aporte por participante só é apurado nos compartilhados: em cofrinho
                // individual o valor de cada um é o saldo total, e a consulta seria desperdício.
                IReadOnlyDictionary<Guid, decimal>? memberBalances = (goal.Members?.Count ?? 0) > 1
                    ? await this.repository.GetMemberBalancesAsync(goal.Id)
                    : null;

                responses.Add(new SavingsGoalResponse(
                    goal,
                    balanceByGoal.GetValueOrDefault(goal.Id),
                    userId,
                    memberBalances));
            }

            return responses;
        }

        /// <inheritdoc />
        public async Task<SavingsGoalResponse?> GetByIdAsync(Guid userId, Guid savingsGoalId)
        {
            SavingsGoal? savingsGoal = await this.repository.GetByIdAsync(userId, savingsGoalId);

            return savingsGoal is null ? null : await this.BuildResponseAsync(savingsGoal, userId);
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

            SavingsGoal? reloaded = await this.repository.GetByIdAsync(userId, savingsGoal.Id);

            return new SavingsGoalResponse(reloaded ?? savingsGoal, 0, userId);
        }

        /// <inheritdoc />
        public async Task<SavingsGoalResponse?> UpdateAsync(Guid userId, Guid savingsGoalId, SavingsGoalRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            SavingsGoal? savingsGoal = await this.repository.GetByIdAsync(userId, savingsGoalId);

            if (savingsGoal is null)
                return null;

            EnsureOwner(savingsGoal, userId);

            string name = request.Name.Trim();

            await this.EnsureNameIsFreeAsync(userId, name, savingsGoalId);

            if (request.IsEmergencyReserve && !savingsGoal.IsEmergencyReserve)
            {
                await this.EnsureSingleEmergencyReserveAsync(userId, savingsGoalId);

                // Reserva de emergência é individual: transformar em reserva um cofrinho que
                // outras pessoas movimentam misturaria o dinheiro delas no cálculo da reserva.
                if ((savingsGoal.Members?.Count ?? 0) > 1)
                    throw new BusinessRuleViolationException("Um cofrinho compartilhado não pode ser a reserva de emergência.");
            }

            savingsGoal.Name = name;
            savingsGoal.TargetAmount = request.TargetAmount;
            savingsGoal.Deadline = request.Deadline.HasValue ? DateTimeHelper.ToUtcDate(request.Deadline.Value) : null;
            savingsGoal.Color = request.Color.ToUpperInvariant();
            savingsGoal.Icon = request.Icon.Trim();
            savingsGoal.IsEmergencyReserve = request.IsEmergencyReserve;
            savingsGoal.UpdatedAt = DateTime.UtcNow;

            // A meta pode ter subido ou descido: a situação é reavaliada contra o saldo real.
            decimal balance = await this.repository.GetBalanceAsync(savingsGoalId);

            this.ApplyCompletionState(savingsGoal, balance);

            bool updated = await this.repository.UpdateAsync(savingsGoal);

            if (!updated)
                throw new BusinessRuleViolationException("Não foi possível atualizar o cofrinho.");

            this.logger.LogInformation("Cofrinho {SavingsGoalId} atualizado pelo usuário {UserId}.", savingsGoalId, userId);

            return await this.BuildResponseAsync(savingsGoal, userId);
        }

        /// <inheritdoc />
        public async Task<SavingsGoalResponse?> SetStatusAsync(Guid userId, Guid savingsGoalId, SavingsGoalStatus status)
        {
            SavingsGoal? savingsGoal = await this.repository.GetByIdAsync(userId, savingsGoalId);

            if (savingsGoal is null)
                return null;

            EnsureOwner(savingsGoal, userId);

            savingsGoal.Status = status;
            savingsGoal.UpdatedAt = DateTime.UtcNow;

            if (status != SavingsGoalStatus.Completed)
                savingsGoal.CompletedAt = null;

            bool updated = await this.repository.UpdateAsync(savingsGoal);

            if (!updated)
                throw new BusinessRuleViolationException("Não foi possível alterar a situação do cofrinho.");

            return await this.BuildResponseAsync(savingsGoal, userId);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(Guid userId, Guid savingsGoalId)
        {
            SavingsGoal? savingsGoal = await this.repository.GetByIdAsync(userId, savingsGoalId);

            if (savingsGoal is null)
                return false;

            EnsureOwner(savingsGoal, userId);

            // Excluir levaria os aportes dos outros participantes junto. Enquanto houver mais
            // gente dentro, a saída é remover os participantes ou arquivar.
            if ((savingsGoal.Members?.Count ?? 0) > 1)
                throw new BusinessRuleViolationException("Remova os outros participantes antes de excluir o cofrinho.");

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

            IReadOnlyList<SavingsGoalContribution> contributions = await this.repository.GetContributionsAsync(savingsGoalId, limit);

            return contributions
                .Select(contribution => new ContributionResponse(contribution, userId))
                .ToList();
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

            decimal balance = await this.repository.GetBalanceAsync(savingsGoalId);

            // O resgate é limitado pelo saldo do cofrinho, não pelo que o participante aportou:
            // em cofrinho compartilhado o dinheiro é comum, e travar por participante impediria
            // um casal de usar a própria poupança conjunta.
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
                "Movimento {Kind} de {Amount} registrado no cofrinho {SavingsGoalId} pelo usuário {UserId}.",
                contribution.Kind,
                contribution.Amount,
                savingsGoalId,
                userId);

            return await this.BuildResponseAsync(savingsGoal, userId);
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

            decimal balance = await this.repository.GetBalanceAsync(contribution.SavingsGoalId);

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

            return await this.BuildResponseAsync(savingsGoal, userId);
        }

        #endregion

        #region Methods :: GetMembersAsync(), AddMemberAsync(), RemoveMemberAsync(), LeaveAsync()

        /// <inheritdoc />
        public async Task<IReadOnlyList<SavingsGoalMemberResponse>?> GetMembersAsync(Guid userId, Guid savingsGoalId)
        {
            bool participates = await this.repository.IsMemberAsync(savingsGoalId, userId);

            if (!participates)
                return null;

            IReadOnlyList<SavingsGoalMember> members = await this.repository.GetMembersAsync(savingsGoalId);

            IReadOnlyDictionary<Guid, decimal> balances = await this.repository.GetMemberBalancesAsync(savingsGoalId);

            return members
                .Select(member => new SavingsGoalMemberResponse(member, balances.GetValueOrDefault(member.UserId)))
                .ToList();
        }

        /// <inheritdoc />
        public async Task<SavingsGoalResponse?> AddMemberAsync(Guid userId, Guid savingsGoalId, SavingsGoalMemberRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            SavingsGoal? savingsGoal = await this.repository.GetByIdAsync(userId, savingsGoalId);

            if (savingsGoal is null)
                return null;

            EnsureOwner(savingsGoal, userId);

            if (savingsGoal.IsEmergencyReserve)
                throw new BusinessRuleViolationException("A reserva de emergência é individual e não pode ser compartilhada.");

            if (request.FriendUserId == userId)
                throw new BusinessRuleViolationException("Você já participa deste cofrinho.");

            bool areFriends = await this.friendshipRepository.AreFriendsAsync(userId, request.FriendUserId);

            if (!areFriends)
                throw new BusinessRuleViolationException("Só é possível compartilhar um cofrinho com amigos.");

            bool alreadyMember = await this.repository.IsMemberAsync(savingsGoalId, request.FriendUserId);

            if (alreadyMember)
                throw new BusinessRuleViolationException("Este amigo já participa do cofrinho.");

            SavingsGoalMember member = new SavingsGoalMember()
            {
                Id = Guid.NewGuid(),
                SavingsGoalId = savingsGoalId,
                UserId = request.FriendUserId,
                Role = SavingsGoalMemberRole.Member,
                JoinedAt = DateTime.UtcNow
            };

            bool added = await this.repository.AddMemberAsync(member);

            if (!added)
                throw new BusinessRuleViolationException("Não foi possível adicionar o participante.");

            IReadOnlyList<UserSummary> summaries = await this.friendshipRepository.GetUserSummariesAsync([userId]);

            string ownerName = summaries.FirstOrDefault()?.FullName ?? "Um amigo";

            await this.notificationService.CreateAsync(
                request.FriendUserId,
                NotificationType.SharedGoalJoined,
                "Você entrou em um cofrinho compartilhado",
                $"{ownerName} adicionou você ao cofrinho \"{savingsGoal.Name}\". Agora vocês guardam juntos.",
                savingsGoalId,
                dedupeWindow: TimeSpan.FromDays(1));

            this.logger.LogInformation(
                "Participante {MemberUserId} adicionado ao cofrinho {SavingsGoalId} por {UserId}.",
                request.FriendUserId,
                savingsGoalId,
                userId);

            SavingsGoal? reloaded = await this.repository.GetByIdAsync(userId, savingsGoalId);

            return await this.BuildResponseAsync(reloaded ?? savingsGoal, userId);
        }

        /// <inheritdoc />
        public async Task<SavingsGoalResponse?> RemoveMemberAsync(Guid userId, Guid savingsGoalId, Guid memberUserId)
        {
            SavingsGoal? savingsGoal = await this.repository.GetByIdAsync(userId, savingsGoalId);

            if (savingsGoal is null)
                return null;

            EnsureOwner(savingsGoal, userId);

            SavingsGoalMember? member = await this.repository.GetMemberAsync(savingsGoalId, memberUserId);

            if (member is null)
                return null;

            if (member.Role == SavingsGoalMemberRole.Owner)
                throw new BusinessRuleViolationException("O criador não pode ser removido do cofrinho.");

            await this.EnsureMemberHasNoContributionsAsync(savingsGoalId, memberUserId);

            bool removed = await this.repository.RemoveMemberAsync(member);

            if (!removed)
                throw new BusinessRuleViolationException("Não foi possível remover o participante.");

            this.logger.LogInformation(
                "Participante {MemberUserId} removido do cofrinho {SavingsGoalId} por {UserId}.",
                memberUserId,
                savingsGoalId,
                userId);

            SavingsGoal? reloaded = await this.repository.GetByIdAsync(userId, savingsGoalId);

            return await this.BuildResponseAsync(reloaded ?? savingsGoal, userId);
        }

        /// <inheritdoc />
        public async Task<bool> LeaveAsync(Guid userId, Guid savingsGoalId)
        {
            SavingsGoalMember? member = await this.repository.GetMemberAsync(savingsGoalId, userId);

            if (member is null)
                return false;

            if (member.Role == SavingsGoalMemberRole.Owner)
                throw new BusinessRuleViolationException("O criador não pode sair do cofrinho. Exclua-o ou remova os participantes.");

            await this.EnsureMemberHasNoContributionsAsync(savingsGoalId, userId);

            bool removed = await this.repository.RemoveMemberAsync(member);

            if (removed)
                this.logger.LogInformation("Usuário {UserId} saiu do cofrinho {SavingsGoalId}.", userId, savingsGoalId);

            return removed;
        }

        #endregion

        #region Helpers :: EnsureMemberHasNoContributionsAsync(), ApplyCompletionState(), NotifyGoalAchievedAsync()

        /// <summary>
        /// Garante que o participante não tem dinheiro dentro do cofrinho.
        /// </summary>
        /// <remarks>
        /// Sair com saldo aportado deixaria dinheiro de alguém em um cofrinho a que essa pessoa
        /// não tem mais acesso. O caminho é resgatar a parte antes de sair.
        /// </remarks>
        /// <param name="savingsGoalId">Identificador do cofrinho.</param>
        /// <param name="memberUserId">Participante a verificar.</param>
        /// <exception cref="BusinessRuleViolationException">O participante ainda tem saldo aportado.</exception>
        private async Task EnsureMemberHasNoContributionsAsync(Guid savingsGoalId, Guid memberUserId)
        {
            IReadOnlyDictionary<Guid, decimal> balances = await this.repository.GetMemberBalancesAsync(savingsGoalId);

            decimal contributed = balances.GetValueOrDefault(memberUserId);

            if (contributed != 0)
                throw new BusinessRuleViolationException("Este participante ainda tem aportes no cofrinho. Resgate a parte dele antes de removê-lo.");
        }

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
        /// Cria a notificação de meta atingida para todos os participantes.
        /// </summary>
        /// <param name="savingsGoal">Cofrinho que atingiu a meta.</param>
        private async Task NotifyGoalAchievedAsync(SavingsGoal savingsGoal)
        {
            string title = savingsGoal.IsEmergencyReserve
                ? "Reserva de emergência completa!"
                : $"Meta do cofrinho \"{savingsGoal.Name}\" atingida!";

            string message = $"Você alcançou {savingsGoal.TargetAmount:C2} em \"{savingsGoal.Name}\". Parabéns pela disciplina!";

            // Em cofrinho compartilhado a conquista é de todos: avisar só o criador esconderia o
            // resultado de quem também guardou.
            IReadOnlyList<SavingsGoalMember> members = await this.repository.GetMembersAsync(savingsGoal.Id);

            foreach (SavingsGoalMember member in members)
            {
                await this.notificationService.CreateAsync(
                    member.UserId,
                    NotificationType.GoalAchieved,
                    title,
                    message,
                    savingsGoal.Id,
                    dedupeWindow: null);
            }
        }

        #endregion
    }
}
