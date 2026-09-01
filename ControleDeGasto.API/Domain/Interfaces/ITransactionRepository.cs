using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;
using ControleDeGasto.API.Domain.Queries;
using ControleDeGasto.API.Domain.ReadModels;

namespace ControleDeGasto.API.Domain.Interfaces
{
    /// <summary>
    /// Acesso a lançamentos. Todo método recebe o dono e filtra por ele.
    /// </summary>
    public interface ITransactionRepository
    {
        #region Methods :: GetPagedAsync(), GetByIdAsync(), GetRecentAsync(), CreateAsync(), UpdateAsync(), DeleteAsync()

        /// <summary>
        /// Lista lançamentos aplicando filtros e paginação.
        /// </summary>
        /// <param name="query">Filtros da consulta.</param>
        /// <returns>Página de lançamentos e total de registros filtrados.</returns>
        Task<PagedResult<Transaction>> GetPagedAsync(TransactionQuery query);

        /// <summary>
        /// Obtém um lançamento do usuário, com categoria, carteira, etiquetas e divisões.
        /// </summary>
        /// <param name="userId">Dono do lançamento.</param>
        /// <param name="transactionId">Identificador do lançamento.</param>
        /// <returns>O lançamento, ou nulo se não existir ou não pertencer ao usuário.</returns>
        Task<Transaction?> GetByIdAsync(Guid userId, Guid transactionId);

        /// <summary>
        /// Lista os lançamentos mais recentes do usuário.
        /// </summary>
        /// <param name="userId">Dono dos lançamentos.</param>
        /// <param name="count">Quantidade máxima de itens.</param>
        /// <returns>Lançamentos ordenados do mais recente para o mais antigo.</returns>
        Task<IReadOnlyList<Transaction>> GetRecentAsync(Guid userId, int count);

        /// <summary>
        /// Persiste um lançamento novo.
        /// </summary>
        /// <param name="transaction">Lançamento a gravar.</param>
        /// <returns>True se a gravação afetou alguma linha.</returns>
        Task<bool> CreateAsync(Transaction transaction);

        /// <summary>
        /// Persiste alterações de um lançamento.
        /// </summary>
        /// <param name="transaction">Lançamento alterado.</param>
        /// <returns>True se a gravação afetou alguma linha.</returns>
        Task<bool> UpdateAsync(Transaction transaction);

        /// <summary>
        /// Remove um lançamento.
        /// </summary>
        /// <param name="transaction">Lançamento a remover.</param>
        /// <returns>True se a remoção afetou alguma linha.</returns>
        Task<bool> DeleteAsync(Transaction transaction);

        #endregion

        #region Methods :: GetTotalsByTypeAsync(), GetTotalsByCategoryAsync(), GetMonthlyTotalsAsync(), GetPendingTotalsAsync()

        /// <summary>
        /// Soma os lançamentos liquidados do período agrupados por natureza.
        /// </summary>
        /// <param name="userId">Dono dos lançamentos.</param>
        /// <param name="from">Início do período, em UTC.</param>
        /// <param name="to">Fim do período, em UTC.</param>
        /// <returns>Um total por natureza presente no período.</returns>
        Task<IReadOnlyList<TypeTotal>> GetTotalsByTypeAsync(Guid userId, DateTime from, DateTime to);

        /// <summary>
        /// Soma os lançamentos liquidados do período agrupados por categoria.
        /// </summary>
        /// <param name="userId">Dono dos lançamentos.</param>
        /// <param name="type">Natureza a considerar.</param>
        /// <param name="from">Início do período, em UTC.</param>
        /// <param name="to">Fim do período, em UTC.</param>
        /// <param name="limit">Quantidade máxima de categorias, das que mais movimentaram. Nulo traz todas.</param>
        /// <param name="walletId">Restringe a uma carteira. Nulo considera todas.</param>
        /// <returns>Totais por categoria, do maior para o menor.</returns>
        Task<IReadOnlyList<CategoryTotal>> GetTotalsByCategoryAsync(Guid userId, TransactionType type, DateTime from, DateTime to, int? limit, Guid? walletId = null);

        /// <summary>
        /// Soma os lançamentos liquidados agrupados por ano, mês e natureza.
        /// </summary>
        /// <param name="userId">Dono dos lançamentos.</param>
        /// <param name="from">Início do período, em UTC.</param>
        /// <param name="to">Fim do período, em UTC.</param>
        /// <returns>Totais mensais em ordem cronológica.</returns>
        Task<IReadOnlyList<MonthlyTotal>> GetMonthlyTotalsAsync(Guid userId, DateTime from, DateTime to);

        /// <summary>
        /// Soma os lançamentos ainda previstos do período, destacando os vencidos.
        /// </summary>
        /// <remarks>
        /// É a base do bloco "a pagar e a receber" do painel. Separado dos totais liquidados
        /// porque misturar o que aconteceu com o que está previsto no mesmo indicador esconde
        /// exatamente a informação que o usuário procura.
        /// </remarks>
        /// <param name="userId">Dono dos lançamentos.</param>
        /// <param name="from">Início do período, em UTC.</param>
        /// <param name="to">Fim do período, em UTC.</param>
        /// <param name="reference">Momento usado como "hoje" ao decidir o que está vencido, em UTC.</param>
        /// <returns>Um total por natureza que tenha lançamento previsto no período.</returns>
        Task<IReadOnlyList<PendingTypeTotal>> GetPendingTotalsAsync(Guid userId, DateTime from, DateTime to, DateTime reference);

        #endregion

        #region Methods :: GetUpcomingAsync(), GetOverdueAsync()

        /// <summary>
        /// Lista as contas previstas com vencimento dentro de uma janela à frente.
        /// </summary>
        /// <param name="userId">Dono dos lançamentos.</param>
        /// <param name="from">Início da janela, em UTC.</param>
        /// <param name="to">Fim da janela, em UTC.</param>
        /// <param name="limit">Quantidade máxima de itens.</param>
        /// <returns>Contas do vencimento mais próximo para o mais distante.</returns>
        Task<IReadOnlyList<Transaction>> GetUpcomingAsync(Guid userId, DateTime from, DateTime to, int limit);

        /// <summary>
        /// Lista as contas previstas cujo vencimento já passou.
        /// </summary>
        /// <param name="userId">Dono dos lançamentos.</param>
        /// <param name="reference">Momento usado como "hoje", em UTC.</param>
        /// <param name="limit">Quantidade máxima de itens.</param>
        /// <returns>Contas da mais atrasada para a menos atrasada.</returns>
        Task<IReadOnlyList<Transaction>> GetOverdueAsync(Guid userId, DateTime reference, int limit);

        #endregion

        #region Methods :: ReplaceTagsAsync(), ReplaceSharesAsync(), GetShareByIdAsync(), UpdateShareAsync()

        /// <summary>
        /// Substitui as etiquetas de um lançamento pelo conjunto informado.
        /// </summary>
        /// <remarks>
        /// Substituição em vez de adição incremental: o formulário envia o estado final das
        /// etiquetas, e calcular a diferença no cliente deixaria a remoção de uma etiqueta
        /// dependente de o cliente lembrar de informá-la.
        /// </remarks>
        /// <param name="transactionId">Lançamento a atualizar.</param>
        /// <param name="tagIds">Etiquetas que devem ficar aplicadas.</param>
        /// <returns>Quantidade de vínculos gravados.</returns>
        Task<int> ReplaceTagsAsync(Guid transactionId, IReadOnlyList<Guid> tagIds);

        /// <summary>
        /// Substitui as divisões de um lançamento pelo conjunto informado.
        /// </summary>
        /// <remarks>
        /// Preserva a liquidação já registrada de um participante que continua na divisão: uma
        /// substituição cega faria o amigo que já pagou voltar a dever.
        /// </remarks>
        /// <param name="transactionId">Lançamento a atualizar.</param>
        /// <param name="shares">Divisões que devem ficar aplicadas.</param>
        /// <returns>Quantidade de divisões gravadas.</returns>
        Task<int> ReplaceSharesAsync(Guid transactionId, IReadOnlyList<TransactionShare> shares);

        /// <summary>
        /// Obtém uma divisão que o usuário pode enxergar: como pagador ou como participante.
        /// </summary>
        /// <param name="userId">Usuário que consulta.</param>
        /// <param name="shareId">Identificador da divisão.</param>
        /// <returns>A divisão com o lançamento carregado, ou nulo quando o usuário não participa.</returns>
        Task<TransactionShare?> GetShareByIdAsync(Guid userId, Guid shareId);

        /// <summary>
        /// Persiste alterações de uma divisão.
        /// </summary>
        /// <param name="share">Divisão alterada.</param>
        /// <returns>True se a gravação afetou alguma linha.</returns>
        Task<bool> UpdateShareAsync(TransactionShare share);

        #endregion

        #region Methods :: GetSharedWithUserAsync(), GetFriendBalancesAsync()

        /// <summary>
        /// Lista as divisões atribuídas ao usuário por amigos.
        /// </summary>
        /// <param name="userId">Participante das divisões.</param>
        /// <param name="onlyOpen">Quando verdadeiro, traz apenas as ainda não acertadas.</param>
        /// <param name="page">Página solicitada, iniciando em 1.</param>
        /// <param name="pageSize">Itens por página.</param>
        /// <returns>Página de divisões, com o lançamento e o pagador carregados.</returns>
        Task<PagedResult<TransactionShare>> GetSharedWithUserAsync(Guid userId, bool onlyOpen, int page, int pageSize);

        /// <summary>
        /// Apura, por amigo, quanto está em aberto nos dois sentidos.
        /// </summary>
        /// <param name="userId">Usuário de referência.</param>
        /// <returns>Um registro por amigo com divisão em aberto.</returns>
        Task<IReadOnlyList<FriendBalance>> GetFriendBalancesAsync(Guid userId);

        #endregion

        #region Methods :: GetInstallmentPlansAsync(), GetInstallmentPlanByIdAsync(), CreateInstallmentPlanAsync(), DeleteInstallmentPlanAsync()

        /// <summary>
        /// Lista as compras parceladas do usuário.
        /// </summary>
        /// <param name="userId">Dono das compras.</param>
        /// <param name="onlyOpen">Quando verdadeiro, traz apenas as que ainda têm parcela prevista.</param>
        /// <returns>Compras da mais recente para a mais antiga, com as parcelas carregadas.</returns>
        Task<IReadOnlyList<InstallmentPlan>> GetInstallmentPlansAsync(Guid userId, bool onlyOpen);

        /// <summary>
        /// Obtém uma compra parcelada do usuário, com as parcelas.
        /// </summary>
        /// <param name="userId">Dono da compra.</param>
        /// <param name="installmentPlanId">Identificador da compra.</param>
        /// <returns>A compra, ou nulo se não existir ou não pertencer ao usuário.</returns>
        Task<InstallmentPlan?> GetInstallmentPlanByIdAsync(Guid userId, Guid installmentPlanId);

        /// <summary>
        /// Grava a compra parcelada e as parcelas em uma única transação de banco.
        /// </summary>
        /// <remarks>
        /// Atômico de propósito: um plano de doze parcelas gravado pela metade produziria uma
        /// dívida que não fecha com o total da compra.
        /// </remarks>
        /// <param name="plan">Compra a gravar.</param>
        /// <param name="installments">Parcelas a gravar.</param>
        /// <returns>True se a gravação afetou alguma linha.</returns>
        Task<bool> CreateInstallmentPlanAsync(InstallmentPlan plan, IReadOnlyList<Transaction> installments);

        /// <summary>
        /// Remove uma compra parcelada e, em cascata, suas parcelas.
        /// </summary>
        /// <param name="plan">Compra a remover.</param>
        /// <returns>True se a remoção afetou alguma linha.</returns>
        Task<bool> DeleteInstallmentPlanAsync(InstallmentPlan plan);

        #endregion
    }
}
