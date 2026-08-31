using ControleDeGasto.API.Application.DTOs;

namespace ControleDeGasto.API.Application.Interfaces
{
    /// <summary>
    /// Regras de amizade entre usuários.
    /// </summary>
    public interface IFriendshipService
    {
        #region Methods :: SearchUsersAsync(), GetFriendsAsync(), GetPendingAsync()

        /// <summary>
        /// Procura usuários para convidar.
        /// </summary>
        /// <param name="userId">Usuário que procura.</param>
        /// <param name="term">Trecho do apelido, ou e-mail exato.</param>
        /// <returns>Usuários encontrados com a situação da relação.</returns>
        /// <exception cref="Domain.Exceptions.BusinessRuleViolationException">Termo curto demais.</exception>
        Task<IReadOnlyList<UserSearchResponse>> SearchUsersAsync(Guid userId, string term);

        /// <summary>
        /// Lista os amigos do usuário com o saldo de divisões em aberto.
        /// </summary>
        /// <param name="userId">Usuário de referência.</param>
        /// <returns>Amigos ordenados por nome.</returns>
        Task<IReadOnlyList<FriendResponse>> GetFriendsAsync(Guid userId);

        /// <summary>
        /// Lista os convites pendentes, recebidos e enviados.
        /// </summary>
        /// <param name="userId">Usuário de referência.</param>
        /// <returns>Convites pendentes, os recebidos primeiro.</returns>
        Task<IReadOnlyList<FriendResponse>> GetPendingAsync(Guid userId);

        #endregion

        #region Methods :: InviteAsync(), RespondAsync(), RemoveAsync(), BlockAsync(), UnblockAsync()

        /// <summary>
        /// Envia um convite de amizade.
        /// </summary>
        /// <remarks>
        /// Um convite recebido do outro lado é aceito em vez de duplicado: convidar quem já
        /// convidou é, na prática, aceitar.
        /// </remarks>
        /// <param name="userId">Quem convida.</param>
        /// <param name="request">Usuário a convidar.</param>
        /// <returns>A relação resultante.</returns>
        /// <exception cref="Domain.Exceptions.BusinessRuleViolationException">Usuário inexistente, já amigo ou relação bloqueada.</exception>
        Task<FriendResponse> InviteAsync(Guid userId, FriendInviteRequest request);

        /// <summary>
        /// Aceita ou recusa um convite recebido.
        /// </summary>
        /// <param name="userId">Quem responde.</param>
        /// <param name="friendshipId">Identificador da relação.</param>
        /// <param name="accept">Verdadeiro para aceitar; falso para recusar.</param>
        /// <returns>A relação atualizada, ou nulo quando o convite não existe para esse usuário.</returns>
        /// <exception cref="Domain.Exceptions.BusinessRuleViolationException">Convite já respondido ou enviado pelo próprio usuário.</exception>
        Task<FriendResponse?> RespondAsync(Guid userId, Guid friendshipId, bool accept);

        /// <summary>
        /// Desfaz uma amizade ou cancela um convite enviado.
        /// </summary>
        /// <param name="userId">Quem remove.</param>
        /// <param name="friendshipId">Identificador da relação.</param>
        /// <returns>True se removeu; false quando a relação não existe para esse usuário.</returns>
        /// <exception cref="Domain.Exceptions.BusinessRuleViolationException">Existem divisões de compra em aberto.</exception>
        Task<bool> RemoveAsync(Guid userId, Guid friendshipId);

        /// <summary>
        /// Bloqueia um usuário, impedindo novos convites em qualquer sentido.
        /// </summary>
        /// <param name="userId">Quem bloqueia.</param>
        /// <param name="targetUserId">Usuário a bloquear.</param>
        /// <returns>A relação bloqueada.</returns>
        /// <exception cref="Domain.Exceptions.BusinessRuleViolationException">Usuário inexistente ou é o próprio.</exception>
        Task<FriendResponse> BlockAsync(Guid userId, Guid targetUserId);

        /// <summary>
        /// Desfaz um bloqueio aplicado pelo próprio usuário.
        /// </summary>
        /// <param name="userId">Quem desbloqueia.</param>
        /// <param name="friendshipId">Identificador da relação.</param>
        /// <returns>True se desbloqueou; false quando a relação não existe para esse usuário.</returns>
        /// <exception cref="Domain.Exceptions.BusinessRuleViolationException">O bloqueio foi aplicado pelo outro lado.</exception>
        Task<bool> UnblockAsync(Guid userId, Guid friendshipId);

        #endregion
    }
}
