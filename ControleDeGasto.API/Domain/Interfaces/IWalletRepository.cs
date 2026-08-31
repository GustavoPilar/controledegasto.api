using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.ReadModels;

namespace ControleDeGasto.API.Domain.Interfaces
{
    /// <summary>
    /// Acesso a carteiras e transferências. Todo método recebe o dono e filtra por ele.
    /// </summary>
    public interface IWalletRepository
    {
        #region Methods :: GetAllAsync(), GetByIdAsync(), GetDefaultAsync(), ExistsByNameAsync()

        /// <summary>
        /// Lista as carteiras do usuário.
        /// </summary>
        /// <param name="userId">Dono das carteiras.</param>
        /// <param name="includeInactive">Quando verdadeiro, inclui as excluídas logicamente.</param>
        /// <returns>Carteiras com a padrão primeiro.</returns>
        Task<IReadOnlyList<Wallet>> GetAllAsync(Guid userId, bool includeInactive);

        /// <summary>
        /// Obtém uma carteira do usuário.
        /// </summary>
        /// <param name="userId">Dono da carteira.</param>
        /// <param name="walletId">Identificador da carteira.</param>
        /// <returns>A carteira, ou nulo se não existir ou não pertencer ao usuário.</returns>
        Task<Wallet?> GetByIdAsync(Guid userId, Guid walletId);

        /// <summary>
        /// Obtém a carteira padrão do usuário.
        /// </summary>
        /// <param name="userId">Dono da carteira.</param>
        /// <returns>A carteira padrão, ou nulo quando nenhuma foi marcada.</returns>
        Task<Wallet?> GetDefaultAsync(Guid userId);

        /// <summary>
        /// Verifica se já existe carteira ativa com o mesmo nome.
        /// </summary>
        /// <param name="userId">Dono da carteira.</param>
        /// <param name="name">Nome a verificar.</param>
        /// <param name="excludeWalletId">Carteira a ignorar na verificação (usada na edição).</param>
        /// <returns>True se já existir.</returns>
        Task<bool> ExistsByNameAsync(Guid userId, string name, Guid? excludeWalletId);

        #endregion

        #region Methods :: CreateAsync(), UpdateAsync(), ClearDefaultAsync(), HasMovementAsync()

        /// <summary>
        /// Persiste uma carteira nova.
        /// </summary>
        /// <param name="wallet">Carteira a gravar.</param>
        /// <returns>True se a gravação afetou alguma linha.</returns>
        Task<bool> CreateAsync(Wallet wallet);

        /// <summary>
        /// Persiste alterações de uma carteira.
        /// </summary>
        /// <param name="wallet">Carteira alterada.</param>
        /// <returns>True se a gravação afetou alguma linha.</returns>
        Task<bool> UpdateAsync(Wallet wallet);

        /// <summary>
        /// Desmarca a carteira padrão atual do usuário.
        /// </summary>
        /// <remarks>
        /// Executado antes de marcar outra: o índice único filtrado recusaria duas padrões, e
        /// a ordem inversa deixaria a gravação falhar por um instante em que as duas existem.
        /// </remarks>
        /// <param name="userId">Dono das carteiras.</param>
        /// <param name="exceptWalletId">Carteira a preservar como padrão.</param>
        /// <returns>Quantidade de carteiras desmarcadas.</returns>
        Task<int> ClearDefaultAsync(Guid userId, Guid? exceptWalletId);

        /// <summary>
        /// Verifica se a carteira tem lançamentos ou transferências.
        /// </summary>
        /// <param name="userId">Dono da carteira.</param>
        /// <param name="walletId">Identificador da carteira.</param>
        /// <returns>True quando existe movimento vinculado.</returns>
        Task<bool> HasMovementAsync(Guid userId, Guid walletId);

        #endregion

        #region Methods :: GetBalancesAsync(), GetTransferTotalsAsync()

        /// <summary>
        /// Apura, em uma consulta, o movimento liquidado e o previsto de cada carteira.
        /// </summary>
        /// <remarks>
        /// Agrupada para todas as carteiras de uma vez: uma consulta por carteira seria N+1 na
        /// listagem e no painel.
        /// </remarks>
        /// <param name="userId">Dono das carteiras.</param>
        /// <returns>Um registro por carteira que possua lançamentos.</returns>
        Task<IReadOnlyList<WalletBalance>> GetBalancesAsync(Guid userId);

        /// <summary>
        /// Apura, em uma consulta, o total transferido para dentro e para fora de cada carteira.
        /// </summary>
        /// <param name="userId">Dono das carteiras.</param>
        /// <returns>Um registro por carteira que possua transferências.</returns>
        Task<IReadOnlyList<WalletTransferTotal>> GetTransferTotalsAsync(Guid userId);

        #endregion

        #region Methods :: GetTransfersAsync(), GetTransferByIdAsync(), CreateTransferAsync(), DeleteTransferAsync()

        /// <summary>
        /// Lista as transferências do usuário.
        /// </summary>
        /// <param name="userId">Dono das transferências.</param>
        /// <param name="walletId">Carteira envolvida em qualquer um dos lados. Nulo traz todas.</param>
        /// <param name="limit">Quantidade máxima de itens, dos mais recentes.</param>
        /// <returns>Transferências da mais recente para a mais antiga.</returns>
        Task<IReadOnlyList<WalletTransfer>> GetTransfersAsync(Guid userId, Guid? walletId, int limit);

        /// <summary>
        /// Obtém uma transferência do usuário.
        /// </summary>
        /// <param name="userId">Dono da transferência.</param>
        /// <param name="transferId">Identificador da transferência.</param>
        /// <returns>A transferência, ou nulo se não existir ou não pertencer ao usuário.</returns>
        Task<WalletTransfer?> GetTransferByIdAsync(Guid userId, Guid transferId);

        /// <summary>
        /// Persiste uma transferência nova.
        /// </summary>
        /// <param name="transfer">Transferência a gravar.</param>
        /// <returns>True se a gravação afetou alguma linha.</returns>
        Task<bool> CreateTransferAsync(WalletTransfer transfer);

        /// <summary>
        /// Remove uma transferência.
        /// </summary>
        /// <param name="transfer">Transferência a remover.</param>
        /// <returns>True se a remoção afetou alguma linha.</returns>
        Task<bool> DeleteTransferAsync(WalletTransfer transfer);

        #endregion
    }
}
