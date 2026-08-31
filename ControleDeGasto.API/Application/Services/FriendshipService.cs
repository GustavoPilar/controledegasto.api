using ControleDeGasto.API.Application.DTOs;
using ControleDeGasto.API.Application.Interfaces;
using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;
using ControleDeGasto.API.Domain.Exceptions;
using ControleDeGasto.API.Domain.Interfaces;
using ControleDeGasto.API.Domain.ReadModels;

namespace ControleDeGasto.API.Application.Services
{
    public class FriendshipService(
        IFriendshipRepository repository,
        ITransactionRepository transactionRepository,
        INotificationService notificationService,
        ILogger<FriendshipService> logger) : IFriendshipService
    {
        #region Constants :: MIN_SEARCH_LENGTH, SEARCH_LIMIT

        /// <summary>
        /// Tamanho mínimo do termo de busca. Um termo de uma letra devolveria uma fatia grande
        /// da base de usuários a cada requisição.
        /// </summary>
        private const int MIN_SEARCH_LENGTH = 3;

        private const int SEARCH_LIMIT = 20;

        #endregion

        #region Fields

        private readonly IFriendshipRepository repository = repository;
        private readonly ITransactionRepository transactionRepository = transactionRepository;
        private readonly INotificationService notificationService = notificationService;
        private readonly ILogger<FriendshipService> logger = logger;

        #endregion

        #region Helpers :: BuildResponsesAsync(), ResolveFriendId()

        /// <summary>
        /// Resolve qual dos dois lados da relação é o amigo de quem consulta.
        /// </summary>
        /// <param name="friendship">Relação a avaliar.</param>
        /// <param name="userId">Usuário que consulta.</param>
        /// <returns>Identificador do outro lado.</returns>
        private static Guid ResolveFriendId(Friendship friendship, Guid userId)
        {
            return friendship.RequesterId == userId ? friendship.AddresseeId : friendship.RequesterId;
        }

        /// <summary>
        /// Monta as respostas de um conjunto de relações já com o saldo de divisões.
        /// </summary>
        /// <remarks>
        /// Os saldos vêm de uma consulta agrupada única: buscar por amigo transformaria a lista
        /// de amigos em N+1 consultas.
        /// </remarks>
        /// <param name="friendships">Relações a converter.</param>
        /// <param name="userId">Usuário que consulta.</param>
        /// <returns>Respostas ordenadas por nome.</returns>
        private async Task<IReadOnlyList<FriendResponse>> BuildResponsesAsync(IReadOnlyList<Friendship> friendships, Guid userId)
        {
            if (friendships.Count == 0)
                return [];

            IReadOnlyList<FriendBalance> balances = await this.transactionRepository.GetFriendBalancesAsync(userId);

            Dictionary<Guid, FriendBalance> balanceByFriend = balances.ToDictionary(x => x.FriendUserId);

            return friendships
                .Select(friendship =>
                {
                    Guid friendId = ResolveFriendId(friendship, userId);

                    FriendBalance? balance = balanceByFriend.GetValueOrDefault(friendId);

                    return new FriendResponse(friendship, userId, balance?.Receivable ?? 0, balance?.Payable ?? 0);
                })
                .OrderBy(item => item.FullName)
                .ToList();
        }

        #endregion

        #region Methods :: SearchUsersAsync(), GetFriendsAsync(), GetPendingAsync()

        /// <inheritdoc />
        public async Task<IReadOnlyList<UserSearchResponse>> SearchUsersAsync(Guid userId, string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Trim().Length < MIN_SEARCH_LENGTH)
                throw new BusinessRuleViolationException("Informe ao menos 3 caracteres para procurar.");

            IReadOnlyList<UserSummary> found = await this.repository.SearchUsersAsync(userId, term, SEARCH_LIMIT);

            if (found.Count == 0)
                return [];

            // Uma consulta para todas as relações de uma vez: a busca devolve até vinte linhas,
            // e uma verificação por linha transformaria isso em vinte idas ao banco.
            IReadOnlyList<Guid> foundIds = found.Select(item => item.UserId).ToList();

            IReadOnlyList<Friendship> relations = await this.repository.GetByUsersManyAsync(userId, foundIds);

            return found
                .Select(summary =>
                {
                    Friendship? friendship = relations.FirstOrDefault(item =>
                        item.RequesterId == summary.UserId || item.AddresseeId == summary.UserId);

                    bool isIncoming = friendship is not null && friendship.AddresseeId == userId;

                    return new UserSearchResponse(summary, friendship?.Status, isIncoming);
                })
                .ToList();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<FriendResponse>> GetFriendsAsync(Guid userId)
        {
            IReadOnlyList<Friendship> friendships = await this.repository.GetByStatusAsync(userId, FriendshipStatus.Accepted);

            return await this.BuildResponsesAsync(friendships, userId);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<FriendResponse>> GetPendingAsync(Guid userId)
        {
            IReadOnlyList<Friendship> friendships = await this.repository.GetByStatusAsync(userId, FriendshipStatus.Pending);

            IReadOnlyList<FriendResponse> responses = await this.BuildResponsesAsync(friendships, userId);

            // Os recebidos primeiro: são os que pedem ação do usuário.
            return responses
                .OrderByDescending(item => item.IsIncoming)
                .ThenBy(item => item.FullName)
                .ToList();
        }

        #endregion

        #region Methods :: InviteAsync(), RespondAsync()

        /// <inheritdoc />
        public async Task<FriendResponse> InviteAsync(Guid userId, FriendInviteRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.TargetUserId == userId)
                throw new BusinessRuleViolationException("Não é possível adicionar você mesmo.");

            IReadOnlyList<UserSummary> targets = await this.repository.GetUserSummariesAsync([request.TargetUserId]);

            if (targets.Count == 0)
                throw new BusinessRuleViolationException("Usuário não encontrado.");

            Friendship? existing = await this.repository.GetByUsersAsync(userId, request.TargetUserId);

            if (existing is not null)
            {
                switch (existing.Status)
                {
                    case FriendshipStatus.Accepted:
                        throw new BusinessRuleViolationException("Vocês já são amigos.");

                    case FriendshipStatus.Blocked:
                        throw new BusinessRuleViolationException("Não é possível enviar convite para este usuário.");

                    case FriendshipStatus.Pending when existing.RequesterId == userId:
                        throw new BusinessRuleViolationException("O convite já foi enviado e está aguardando resposta.");

                    case FriendshipStatus.Pending:
                        // Convidar quem já convidou é aceitar: evita dois convites pendentes
                        // entre as mesmas pessoas, um em cada sentido.
                        return await this.AcceptAsync(existing, userId);

                    case FriendshipStatus.Declined:
                        // Recusa não é definitiva: a relação é reaproveitada e volta a pendente,
                        // no sentido de quem está convidando agora.
                        existing.RequesterId = userId;
                        existing.AddresseeId = request.TargetUserId;
                        existing.Status = FriendshipStatus.Pending;
                        existing.RequestedAt = DateTime.UtcNow;
                        existing.RespondedAt = null;

                        await this.repository.UpdateAsync(existing);
                        await this.NotifyInviteAsync(existing, userId);

                        return await this.BuildSingleAsync(existing.Id, userId);
                }
            }

            Friendship friendship = new Friendship()
            {
                Id = Guid.NewGuid(),
                RequesterId = userId,
                AddresseeId = request.TargetUserId,
                Status = FriendshipStatus.Pending,
                RequestedAt = DateTime.UtcNow
            };

            bool created = await this.repository.CreateAsync(friendship);

            if (!created)
                throw new BusinessRuleViolationException("Não foi possível enviar o convite.");

            await this.NotifyInviteAsync(friendship, userId);

            this.logger.LogInformation(
                "Convite de amizade {FriendshipId} enviado por {UserId} para {TargetUserId}.",
                friendship.Id,
                userId,
                request.TargetUserId);

            return await this.BuildSingleAsync(friendship.Id, userId);
        }

        /// <inheritdoc />
        public async Task<FriendResponse?> RespondAsync(Guid userId, Guid friendshipId, bool accept)
        {
            Friendship? friendship = await this.repository.GetByIdAsync(userId, friendshipId);

            if (friendship is null)
                return null;

            if (friendship.Status != FriendshipStatus.Pending)
                throw new BusinessRuleViolationException("Este convite já foi respondido.");

            // Só o destinatário responde: sem essa checagem, quem enviou poderia aceitar o
            // próprio convite e criar uma amizade que o outro lado nunca aprovou.
            if (friendship.AddresseeId != userId)
                throw new BusinessRuleViolationException("Apenas quem recebeu o convite pode respondê-lo.");

            if (accept)
                return await this.AcceptAsync(friendship, userId);

            friendship.Status = FriendshipStatus.Declined;
            friendship.RespondedAt = DateTime.UtcNow;

            await this.repository.UpdateAsync(friendship);

            this.logger.LogInformation("Convite de amizade {FriendshipId} recusado por {UserId}.", friendshipId, userId);

            return await this.BuildSingleAsync(friendship.Id, userId);
        }

        #endregion

        #region Methods :: RemoveAsync(), BlockAsync(), UnblockAsync()

        /// <inheritdoc />
        public async Task<bool> RemoveAsync(Guid userId, Guid friendshipId)
        {
            Friendship? friendship = await this.repository.GetByIdAsync(userId, friendshipId);

            if (friendship is null)
                return false;

            Guid friendId = ResolveFriendId(friendship, userId);

            // Desfazer a amizade com dívida em aberto apagaria o registro de quem deve o quê:
            // a divisão referencia o amigo, e o vínculo é o que dá sentido ao valor.
            IReadOnlyList<FriendBalance> balances = await this.transactionRepository.GetFriendBalancesAsync(userId);

            FriendBalance? balance = balances.FirstOrDefault(item => item.FriendUserId == friendId);

            if (balance is not null && (balance.Receivable > 0 || balance.Payable > 0))
                throw new BusinessRuleViolationException("Existem divisões de compra em aberto com este amigo. Acerte-as antes de remover.");

            bool deleted = await this.repository.DeleteAsync(friendship);

            if (deleted)
                this.logger.LogInformation("Amizade {FriendshipId} removida por {UserId}.", friendshipId, userId);

            return deleted;
        }

        /// <inheritdoc />
        public async Task<FriendResponse> BlockAsync(Guid userId, Guid targetUserId)
        {
            if (targetUserId == userId)
                throw new BusinessRuleViolationException("Não é possível bloquear você mesmo.");

            IReadOnlyList<UserSummary> targets = await this.repository.GetUserSummariesAsync([targetUserId]);

            if (targets.Count == 0)
                throw new BusinessRuleViolationException("Usuário não encontrado.");

            Friendship? friendship = await this.repository.GetByUsersAsync(userId, targetUserId);

            if (friendship is null)
            {
                friendship = new Friendship()
                {
                    Id = Guid.NewGuid(),
                    RequesterId = userId,
                    AddresseeId = targetUserId,
                    Status = FriendshipStatus.Blocked,
                    RequestedAt = DateTime.UtcNow,
                    RespondedAt = DateTime.UtcNow,
                    BlockedByUserId = userId
                };

                bool created = await this.repository.CreateAsync(friendship);

                if (!created)
                    throw new BusinessRuleViolationException("Não foi possível bloquear o usuário.");
            }
            else
            {
                friendship.Status = FriendshipStatus.Blocked;
                friendship.RespondedAt = DateTime.UtcNow;
                friendship.BlockedByUserId = userId;

                await this.repository.UpdateAsync(friendship);
            }

            this.logger.LogInformation("Usuário {TargetUserId} bloqueado por {UserId}.", targetUserId, userId);

            return await this.BuildSingleAsync(friendship.Id, userId);
        }

        /// <inheritdoc />
        public async Task<bool> UnblockAsync(Guid userId, Guid friendshipId)
        {
            Friendship? friendship = await this.repository.GetByIdAsync(userId, friendshipId);

            if (friendship is null)
                return false;

            if (friendship.Status != FriendshipStatus.Blocked)
                throw new BusinessRuleViolationException("Esta relação não está bloqueada.");

            // Só quem bloqueou desfaz: caso contrário o bloqueado se desbloquearia sozinho.
            if (friendship.BlockedByUserId != userId)
                throw new BusinessRuleViolationException("Apenas quem aplicou o bloqueio pode desfazê-lo.");

            bool deleted = await this.repository.DeleteAsync(friendship);

            if (deleted)
                this.logger.LogInformation("Bloqueio {FriendshipId} desfeito por {UserId}.", friendshipId, userId);

            return deleted;
        }

        #endregion

        #region Helpers :: AcceptAsync(), BuildSingleAsync(), NotifyInviteAsync()

        /// <summary>
        /// Aceita um convite pendente e avisa quem enviou.
        /// </summary>
        /// <param name="friendship">Relação pendente.</param>
        /// <param name="userId">Quem está aceitando.</param>
        /// <returns>A relação atualizada.</returns>
        private async Task<FriendResponse> AcceptAsync(Friendship friendship, Guid userId)
        {
            friendship.Status = FriendshipStatus.Accepted;
            friendship.RespondedAt = DateTime.UtcNow;

            bool updated = await this.repository.UpdateAsync(friendship);

            if (!updated)
                throw new BusinessRuleViolationException("Não foi possível aceitar o convite.");

            IReadOnlyList<UserSummary> summaries = await this.repository.GetUserSummariesAsync([userId]);

            string acceptedByName = summaries.FirstOrDefault()?.FullName ?? "Um usuário";

            await this.notificationService.CreateAsync(
                friendship.RequesterId == userId ? friendship.AddresseeId : friendship.RequesterId,
                NotificationType.FriendRequestAccepted,
                "Convite de amizade aceito",
                $"{acceptedByName} aceitou seu convite. Agora vocês podem dividir compras e compartilhar cofrinhos.",
                friendship.Id,
                dedupeWindow: TimeSpan.FromDays(1));

            this.logger.LogInformation("Convite de amizade {FriendshipId} aceito por {UserId}.", friendship.Id, userId);

            return await this.BuildSingleAsync(friendship.Id, userId);
        }

        /// <summary>
        /// Recarrega uma relação já com os usuários e monta a resposta.
        /// </summary>
        /// <remarks>
        /// Recarrega de propósito: a entidade gravada não traz os usuários das navegações, e a
        /// resposta precisa do nome do amigo.
        /// </remarks>
        /// <param name="friendshipId">Identificador da relação.</param>
        /// <param name="userId">Usuário que consulta.</param>
        /// <returns>A resposta montada.</returns>
        private async Task<FriendResponse> BuildSingleAsync(Guid friendshipId, Guid userId)
        {
            Friendship? reloaded = await this.repository.GetByIdAsync(userId, friendshipId);

            if (reloaded is null)
                throw new BusinessRuleViolationException("Não foi possível carregar a amizade.");

            Guid friendId = ResolveFriendId(reloaded, userId);

            IReadOnlyList<UserSummary> summaries = await this.repository.GetUserSummariesAsync([friendId]);

            UserSummary? friend = summaries.FirstOrDefault();

            // Preenche as navegações à mão para reaproveitar o construtor da resposta sem uma
            // consulta com Include só para dois nomes.
            User friendUser = new User()
            {
                Id = friendId,
                FullName = friend?.FullName ?? string.Empty,
                UserName = friend?.UserName ?? string.Empty
            };

            if (reloaded.RequesterId == userId)
                reloaded.Addressee = friendUser;
            else
                reloaded.Requester = friendUser;

            IReadOnlyList<FriendBalance> balances = await this.transactionRepository.GetFriendBalancesAsync(userId);

            FriendBalance? balance = balances.FirstOrDefault(item => item.FriendUserId == friendId);

            return new FriendResponse(reloaded, userId, balance?.Receivable ?? 0, balance?.Payable ?? 0);
        }

        /// <summary>
        /// Cria a notificação de convite recebido.
        /// </summary>
        /// <param name="friendship">Relação do convite.</param>
        /// <param name="requesterId">Quem enviou.</param>
        private async Task NotifyInviteAsync(Friendship friendship, Guid requesterId)
        {
            IReadOnlyList<UserSummary> summaries = await this.repository.GetUserSummariesAsync([requesterId]);

            string requesterName = summaries.FirstOrDefault()?.FullName ?? "Um usuário";

            await this.notificationService.CreateAsync(
                friendship.AddresseeId,
                NotificationType.FriendRequestReceived,
                "Novo convite de amizade",
                $"{requesterName} quer se conectar com você para dividir compras e cofrinhos.",
                friendship.Id,
                // Janela de um dia em vez de "avisar só uma vez": a relação é reaproveitada
                // depois de uma recusa, e avisar apenas na primeira vez esconderia um convite
                // novo enviado semanas depois.
                dedupeWindow: TimeSpan.FromDays(1));
        }

        #endregion
    }
}
