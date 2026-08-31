using ControleDeGasto.API.Application.DTOs;

namespace ControleDeGasto.API.Application.Interfaces
{
    /// <summary>
    /// Regras de lançamento financeiro, divisão entre amigos e compra parcelada.
    /// </summary>
    public interface ITransactionService
    {
        #region Methods :: GetPagedAsync(), GetByIdAsync(), CreateAsync(), UpdateAsync(), DeleteAsync()

        /// <summary>
        /// Lista lançamentos do usuário aplicando filtros e paginação.
        /// </summary>
        /// <param name="userId">Dono dos lançamentos.</param>
        /// <param name="filter">Filtros da listagem.</param>
        /// <returns>Página de lançamentos.</returns>
        Task<PagedResponse<TransactionResponse>> GetPagedAsync(Guid userId, TransactionFilterRequest filter);

        /// <summary>
        /// Obtém um lançamento do usuário.
        /// </summary>
        /// <param name="userId">Dono do lançamento.</param>
        /// <param name="transactionId">Identificador do lançamento.</param>
        /// <returns>O lançamento, ou nulo se não existir para esse usuário.</returns>
        Task<TransactionResponse?> GetByIdAsync(Guid userId, Guid transactionId);

        /// <summary>
        /// Registra um lançamento.
        /// </summary>
        /// <param name="userId">Dono do lançamento.</param>
        /// <param name="request">Dados do lançamento.</param>
        /// <returns>O lançamento criado.</returns>
        /// <exception cref="Domain.Exceptions.BusinessRuleViolationException">Categoria, carteira, etiquetas ou divisão inválidas.</exception>
        Task<TransactionResponse> CreateAsync(Guid userId, TransactionRequest request);

        /// <summary>
        /// Atualiza um lançamento.
        /// </summary>
        /// <param name="userId">Dono do lançamento.</param>
        /// <param name="transactionId">Identificador do lançamento.</param>
        /// <param name="request">Novos dados.</param>
        /// <returns>O lançamento atualizado, ou nulo se não existir para esse usuário.</returns>
        /// <exception cref="Domain.Exceptions.BusinessRuleViolationException">Categoria, carteira, etiquetas ou divisão inválidas.</exception>
        Task<TransactionResponse?> UpdateAsync(Guid userId, Guid transactionId, TransactionRequest request);

        /// <summary>
        /// Remove um lançamento.
        /// </summary>
        /// <param name="userId">Dono do lançamento.</param>
        /// <param name="transactionId">Identificador do lançamento.</param>
        /// <returns>True se removeu; false se não existir para esse usuário.</returns>
        Task<bool> DeleteAsync(Guid userId, Guid transactionId);

        #endregion

        #region Methods :: SettleAsync()

        /// <summary>
        /// Liquida ou reabre um lançamento.
        /// </summary>
        /// <param name="userId">Dono do lançamento.</param>
        /// <param name="transactionId">Identificador do lançamento.</param>
        /// <param name="request">Situação desejada e data da liquidação.</param>
        /// <returns>O lançamento atualizado, ou nulo se não existir para esse usuário.</returns>
        Task<TransactionResponse?> SettleAsync(Guid userId, Guid transactionId, TransactionSettleRequest request);

        #endregion

        #region Methods :: GetSharedWithMeAsync(), SettleShareAsync()

        /// <summary>
        /// Lista as divisões que amigos atribuíram ao usuário.
        /// </summary>
        /// <param name="userId">Participante das divisões.</param>
        /// <param name="onlyOpen">Quando verdadeiro, traz apenas as ainda não acertadas.</param>
        /// <param name="page">Página solicitada, iniciando em 1.</param>
        /// <param name="pageSize">Itens por página.</param>
        /// <returns>Página de divisões.</returns>
        Task<PagedResponse<SharedWithMeResponse>> GetSharedWithMeAsync(Guid userId, bool onlyOpen, int page, int pageSize);

        /// <summary>
        /// Marca uma divisão como acertada, ou a reabre.
        /// </summary>
        /// <remarks>
        /// Os dois lados podem acertar: quem pagou confirma que recebeu, e quem devia confirma
        /// que pagou. Restringir a um dos lados travaria a conversa quando o outro não usa o
        /// sistema com a mesma frequência.
        /// </remarks>
        /// <param name="userId">Quem está acertando.</param>
        /// <param name="shareId">Identificador da divisão.</param>
        /// <param name="settled">Verdadeiro para acertar; falso para reabrir.</param>
        /// <returns>A divisão atualizada, ou nulo quando o usuário não participa dela.</returns>
        Task<TransactionShareResponse?> SettleShareAsync(Guid userId, Guid shareId, bool settled);

        #endregion

        #region Methods :: GetInstallmentPlansAsync(), GetInstallmentPlanByIdAsync(), CreateInstallmentPlanAsync(), DeleteInstallmentPlanAsync()

        /// <summary>
        /// Lista as compras parceladas do usuário.
        /// </summary>
        /// <param name="userId">Dono das compras.</param>
        /// <param name="onlyOpen">Quando verdadeiro, traz apenas as que ainda têm parcela em aberto.</param>
        /// <returns>Compras da mais recente para a mais antiga.</returns>
        Task<IReadOnlyList<InstallmentPlanResponse>> GetInstallmentPlansAsync(Guid userId, bool onlyOpen);

        /// <summary>
        /// Obtém uma compra parcelada do usuário.
        /// </summary>
        /// <param name="userId">Dono da compra.</param>
        /// <param name="installmentPlanId">Identificador da compra.</param>
        /// <returns>A compra, ou nulo se não existir para esse usuário.</returns>
        Task<InstallmentPlanResponse?> GetInstallmentPlanByIdAsync(Guid userId, Guid installmentPlanId);

        /// <summary>
        /// Registra uma compra parcelada, gerando as parcelas como lançamentos previstos.
        /// </summary>
        /// <param name="userId">Dono da compra.</param>
        /// <param name="request">Dados da compra.</param>
        /// <returns>A compra criada.</returns>
        /// <exception cref="Domain.Exceptions.BusinessRuleViolationException">Categoria, carteira ou etiquetas inválidas.</exception>
        Task<InstallmentPlanResponse> CreateInstallmentPlanAsync(Guid userId, InstallmentPlanRequest request);

        /// <summary>
        /// Cancela uma compra parcelada.
        /// </summary>
        /// <remarks>
        /// As parcelas já pagas impedem o cancelamento total: apagá-las removeria despesas que
        /// aconteceram de verdade e mudaria o fechamento de meses passados.
        /// </remarks>
        /// <param name="userId">Dono da compra.</param>
        /// <param name="installmentPlanId">Identificador da compra.</param>
        /// <returns>True se cancelou; false se não existir para esse usuário.</returns>
        /// <exception cref="Domain.Exceptions.BusinessRuleViolationException">Existem parcelas já pagas.</exception>
        Task<bool> DeleteInstallmentPlanAsync(Guid userId, Guid installmentPlanId);

        #endregion
    }
}
