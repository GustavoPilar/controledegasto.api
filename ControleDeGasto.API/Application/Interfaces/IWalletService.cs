using ControleDeGasto.API.Application.DTOs;

namespace ControleDeGasto.API.Application.Interfaces
{
    /// <summary>
    /// Regras de carteira e de transferência entre carteiras.
    /// </summary>
    public interface IWalletService
    {
        #region Methods :: GetAllAsync(), GetByIdAsync(), CreateAsync(), UpdateAsync(), DeleteAsync()

        /// <summary>
        /// Lista as carteiras do usuário com saldo apurado.
        /// </summary>
        /// <param name="userId">Dono das carteiras.</param>
        /// <param name="includeInactive">Quando verdadeiro, inclui as excluídas logicamente.</param>
        /// <returns>Carteiras com a padrão primeiro.</returns>
        Task<IReadOnlyList<WalletResponse>> GetAllAsync(Guid userId, bool includeInactive);

        /// <summary>
        /// Obtém uma carteira do usuário com saldo apurado.
        /// </summary>
        /// <param name="userId">Dono da carteira.</param>
        /// <param name="walletId">Identificador da carteira.</param>
        /// <returns>A carteira, ou nulo se não existir para esse usuário.</returns>
        Task<WalletResponse?> GetByIdAsync(Guid userId, Guid walletId);

        /// <summary>
        /// Cria uma carteira.
        /// </summary>
        /// <param name="userId">Dono da carteira.</param>
        /// <param name="request">Dados da carteira.</param>
        /// <returns>A carteira criada.</returns>
        /// <exception cref="Domain.Exceptions.BusinessRuleViolationException">Nome já usado ou dados incompatíveis com a natureza.</exception>
        Task<WalletResponse> CreateAsync(Guid userId, WalletRequest request);

        /// <summary>
        /// Atualiza uma carteira.
        /// </summary>
        /// <param name="userId">Dono da carteira.</param>
        /// <param name="walletId">Identificador da carteira.</param>
        /// <param name="request">Novos dados.</param>
        /// <returns>A carteira atualizada, ou nulo se não existir para esse usuário.</returns>
        /// <exception cref="Domain.Exceptions.BusinessRuleViolationException">Nome já usado ou dados incompatíveis com a natureza.</exception>
        Task<WalletResponse?> UpdateAsync(Guid userId, Guid walletId, WalletRequest request);

        /// <summary>
        /// Exclui uma carteira. A exclusão é lógica quando existe movimento vinculado.
        /// </summary>
        /// <param name="userId">Dono da carteira.</param>
        /// <param name="walletId">Identificador da carteira.</param>
        /// <returns>True se excluiu; false se não existir para esse usuário.</returns>
        Task<bool> DeleteAsync(Guid userId, Guid walletId);

        #endregion

        #region Methods :: EnsureWalletAsync(), ResolveWalletIdAsync()

        /// <summary>
        /// Garante que a carteira informada existe, pertence ao usuário e está ativa.
        /// </summary>
        /// <param name="userId">Dono da carteira.</param>
        /// <param name="walletId">Carteira escolhida.</param>
        /// <returns>A carteira validada.</returns>
        /// <exception cref="Domain.Exceptions.BusinessRuleViolationException">Carteira inválida para o usuário.</exception>
        Task<Domain.Entities.Wallet> EnsureWalletAsync(Guid userId, Guid walletId);

        /// <summary>
        /// Resolve qual carteira usar em um lançamento.
        /// </summary>
        /// <remarks>
        /// Quando o cliente não informa uma, cai na carteira padrão. Sem esse fallback, quem já
        /// tem carteiras cadastradas conseguiria lançar sem carteira sem perceber, e o saldo
        /// deixaria de fechar com o extrato.
        /// </remarks>
        /// <param name="userId">Dono da carteira.</param>
        /// <param name="walletId">Carteira informada. Nulo usa a padrão.</param>
        /// <returns>Carteira a usar, ou nulo quando o usuário não tem carteira alguma.</returns>
        /// <exception cref="Domain.Exceptions.BusinessRuleViolationException">Carteira informada inválida.</exception>
        Task<Domain.Entities.Wallet?> ResolveWalletAsync(Guid userId, Guid? walletId);

        #endregion

        #region Methods :: GetTransfersAsync(), CreateTransferAsync(), DeleteTransferAsync()

        /// <summary>
        /// Lista as transferências do usuário.
        /// </summary>
        /// <param name="userId">Dono das transferências.</param>
        /// <param name="walletId">Carteira envolvida. Nulo traz todas.</param>
        /// <param name="limit">Quantidade máxima de itens.</param>
        /// <returns>Transferências da mais recente para a mais antiga.</returns>
        Task<IReadOnlyList<WalletTransferResponse>> GetTransfersAsync(Guid userId, Guid? walletId, int limit);

        /// <summary>
        /// Registra uma transferência entre carteiras.
        /// </summary>
        /// <param name="userId">Dono das carteiras.</param>
        /// <param name="request">Dados da transferência.</param>
        /// <returns>A transferência criada.</returns>
        /// <exception cref="Domain.Exceptions.BusinessRuleViolationException">Carteiras iguais, inválidas ou saldo insuficiente.</exception>
        Task<WalletTransferResponse> CreateTransferAsync(Guid userId, WalletTransferRequest request);

        /// <summary>
        /// Remove uma transferência.
        /// </summary>
        /// <param name="userId">Dono da transferência.</param>
        /// <param name="transferId">Identificador da transferência.</param>
        /// <returns>True se removeu; false se não existir para esse usuário.</returns>
        Task<bool> DeleteTransferAsync(Guid userId, Guid transferId);

        #endregion
    }
}
