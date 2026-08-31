using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;
using ControleDeGasto.API.Domain.ReadModels;

namespace ControleDeGasto.API.Domain.Interfaces
{
    /// <summary>
    /// Acesso às relações de amizade. Todo método recebe o usuário em nome de quem consulta.
    /// </summary>
    public interface IFriendshipRepository
    {
        #region Methods :: GetByUsersAsync(), GetByIdAsync(), GetByStatusAsync(), AreFriendsAsync()

        /// <summary>
        /// Obtém a relação entre dois usuários, em qualquer sentido.
        /// </summary>
        /// <remarks>
        /// Procura nas duas ordens de propósito: se A já convidou B, o convite de B para A não
        /// é uma relação nova, é a mesma.
        /// </remarks>
        /// <param name="userId">Um dos lados.</param>
        /// <param name="otherUserId">O outro lado.</param>
        /// <returns>A relação, ou nulo quando os dois nunca interagiram.</returns>
        Task<Friendship?> GetByUsersAsync(Guid userId, Guid otherUserId);

        /// <summary>
        /// Obtém, em uma consulta, as relações entre o usuário e vários outros.
        /// </summary>
        /// <remarks>
        /// Existe para a busca de usuários: cada resultado precisa saber se já há relação, e uma
        /// consulta por linha transformaria uma busca de vinte nomes em vinte idas ao banco.
        /// </remarks>
        /// <param name="userId">Usuário de referência.</param>
        /// <param name="otherUserIds">Identificadores do outro lado.</param>
        /// <returns>Relações encontradas, em qualquer sentido.</returns>
        Task<IReadOnlyList<Friendship>> GetByUsersManyAsync(Guid userId, IReadOnlyList<Guid> otherUserIds);

        /// <summary>
        /// Obtém uma relação em que o usuário informado participa.
        /// </summary>
        /// <param name="userId">Usuário que precisa fazer parte da relação.</param>
        /// <param name="friendshipId">Identificador da relação.</param>
        /// <returns>A relação, ou nulo quando não existe ou o usuário não participa dela.</returns>
        Task<Friendship?> GetByIdAsync(Guid userId, Guid friendshipId);

        /// <summary>
        /// Lista as relações do usuário em uma situação, já com os dois usuários carregados.
        /// </summary>
        /// <param name="userId">Usuário que participa das relações.</param>
        /// <param name="status">Situação desejada.</param>
        /// <returns>Relações encontradas, da mais recente para a mais antiga.</returns>
        Task<IReadOnlyList<Friendship>> GetByStatusAsync(Guid userId, FriendshipStatus status);

        /// <summary>
        /// Verifica se dois usuários são amigos.
        /// </summary>
        /// <remarks>
        /// É a checagem que autoriza dividir uma compra ou convidar para um cofrinho: sem ela,
        /// um cliente poderia atribuir dívidas a qualquer conta cujo identificador descobrisse.
        /// </remarks>
        /// <param name="userId">Um dos lados.</param>
        /// <param name="otherUserId">O outro lado.</param>
        /// <returns>True quando existe amizade aceita entre os dois.</returns>
        Task<bool> AreFriendsAsync(Guid userId, Guid otherUserId);

        /// <summary>
        /// Filtra, entre os identificadores informados, os que são amigos do usuário.
        /// </summary>
        /// <remarks>
        /// Uma consulta para o conjunto todo em vez de uma por amigo: a divisão de uma compra
        /// valida vários participantes de uma vez.
        /// </remarks>
        /// <param name="userId">Usuário de referência.</param>
        /// <param name="candidateUserIds">Identificadores a verificar.</param>
        /// <returns>Subconjunto que tem amizade aceita com o usuário.</returns>
        Task<IReadOnlyList<Guid>> FilterFriendsAsync(Guid userId, IReadOnlyList<Guid> candidateUserIds);

        #endregion

        #region Methods :: GetFriendIdsAsync(), CreateAsync(), UpdateAsync(), DeleteAsync()

        /// <summary>
        /// Lista os identificadores dos amigos do usuário.
        /// </summary>
        /// <param name="userId">Usuário de referência.</param>
        /// <returns>Identificadores dos amigos com amizade aceita.</returns>
        Task<IReadOnlyList<Guid>> GetFriendIdsAsync(Guid userId);

        /// <summary>
        /// Persiste uma relação nova.
        /// </summary>
        /// <param name="friendship">Relação a gravar.</param>
        /// <returns>True se a gravação afetou alguma linha.</returns>
        Task<bool> CreateAsync(Friendship friendship);

        /// <summary>
        /// Persiste alterações de uma relação.
        /// </summary>
        /// <param name="friendship">Relação alterada.</param>
        /// <returns>True se a gravação afetou alguma linha.</returns>
        Task<bool> UpdateAsync(Friendship friendship);

        /// <summary>
        /// Remove uma relação.
        /// </summary>
        /// <param name="friendship">Relação a remover.</param>
        /// <returns>True se a remoção afetou alguma linha.</returns>
        Task<bool> DeleteAsync(Friendship friendship);

        #endregion

        #region Methods :: SearchUsersAsync(), GetUserSummariesAsync()

        /// <summary>
        /// Procura usuários por apelido ou e-mail para o envio de convites.
        /// </summary>
        /// <remarks>
        /// Exige um termo com tamanho mínimo e devolve apenas nome e apelido: uma busca que
        /// aceitasse termo vazio permitiria listar a base inteira de usuários.
        /// </remarks>
        /// <param name="userId">Usuário que procura, sempre excluído do resultado.</param>
        /// <param name="term">Trecho do apelido ou e-mail exato.</param>
        /// <param name="limit">Quantidade máxima de resultados.</param>
        /// <returns>Usuários encontrados.</returns>
        Task<IReadOnlyList<UserSummary>> SearchUsersAsync(Guid userId, string term, int limit);

        /// <summary>
        /// Obtém os dados públicos de vários usuários em uma consulta.
        /// </summary>
        /// <param name="userIds">Identificadores desejados.</param>
        /// <returns>Dados públicos encontrados.</returns>
        Task<IReadOnlyList<UserSummary>> GetUserSummariesAsync(IReadOnlyList<Guid> userIds);

        #endregion
    }
}
