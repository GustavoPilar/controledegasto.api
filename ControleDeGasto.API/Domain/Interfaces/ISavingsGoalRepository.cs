using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.ReadModels;

namespace ControleDeGasto.API.Domain.Interfaces
{
    /// <summary>
    /// Acesso a cofrinhos e seus movimentos. Todo método recebe o dono e filtra por ele.
    /// </summary>
    public interface ISavingsGoalRepository
    {
        #region Methods :: GetAllAsync(), GetByIdAsync(), GetEmergencyReserveAsync(), ExistsByNameAsync(), CreateAsync(), UpdateAsync(), DeleteAsync()

        /// <summary>
        /// Lista os cofrinhos do usuário.
        /// </summary>
        /// <param name="userId">Dono dos cofrinhos.</param>
        /// <param name="includeArchived">Quando verdadeiro, inclui os arquivados.</param>
        /// <returns>Cofrinhos com a reserva de emergência primeiro.</returns>
        Task<IReadOnlyList<SavingsGoal>> GetAllAsync(Guid userId, bool includeArchived);

        /// <summary>
        /// Obtém um cofrinho do usuário.
        /// </summary>
        /// <param name="userId">Dono do cofrinho.</param>
        /// <param name="savingsGoalId">Identificador do cofrinho.</param>
        /// <returns>O cofrinho, ou nulo se não existir ou não pertencer ao usuário.</returns>
        Task<SavingsGoal?> GetByIdAsync(Guid userId, Guid savingsGoalId);

        /// <summary>
        /// Obtém a reserva de emergência do usuário.
        /// </summary>
        /// <param name="userId">Dono da reserva.</param>
        /// <returns>A reserva, ou nulo se ainda não foi criada.</returns>
        Task<SavingsGoal?> GetEmergencyReserveAsync(Guid userId);

        /// <summary>
        /// Verifica se já existe cofrinho com o mesmo nome.
        /// </summary>
        /// <param name="userId">Dono do cofrinho.</param>
        /// <param name="name">Nome a verificar.</param>
        /// <param name="excludeSavingsGoalId">Cofrinho a ignorar na verificação (usado na edição).</param>
        /// <returns>True se já existir.</returns>
        Task<bool> ExistsByNameAsync(Guid userId, string name, Guid? excludeSavingsGoalId);

        /// <summary>
        /// Persiste um cofrinho novo.
        /// </summary>
        /// <param name="savingsGoal">Cofrinho a gravar.</param>
        /// <returns>True se a gravação afetou alguma linha.</returns>
        Task<bool> CreateAsync(SavingsGoal savingsGoal);

        /// <summary>
        /// Persiste alterações de um cofrinho.
        /// </summary>
        /// <param name="savingsGoal">Cofrinho alterado.</param>
        /// <returns>True se a gravação afetou alguma linha.</returns>
        Task<bool> UpdateAsync(SavingsGoal savingsGoal);

        /// <summary>
        /// Remove um cofrinho e, em cascata, seus movimentos.
        /// </summary>
        /// <param name="savingsGoal">Cofrinho a remover.</param>
        /// <returns>True se a remoção afetou alguma linha.</returns>
        Task<bool> DeleteAsync(SavingsGoal savingsGoal);

        #endregion

        #region Methods :: GetBalanceAsync(), GetBalancesAsync(), GetContributionsAsync(), GetContributionByIdAsync(), CreateContributionAsync(), DeleteContributionAsync()

        /// <summary>
        /// Calcula o saldo de um cofrinho (depósitos menos resgates).
        /// </summary>
        /// <param name="userId">Dono do cofrinho.</param>
        /// <param name="savingsGoalId">Identificador do cofrinho.</param>
        /// <returns>Saldo acumulado, zero quando não há movimentos.</returns>
        Task<decimal> GetBalanceAsync(Guid userId, Guid savingsGoalId);

        /// <summary>
        /// Calcula em uma única consulta o saldo de todos os cofrinhos do usuário.
        /// </summary>
        /// <param name="userId">Dono dos cofrinhos.</param>
        /// <returns>Saldo por cofrinho que possua movimentos.</returns>
        Task<IReadOnlyList<GoalBalance>> GetBalancesAsync(Guid userId);

        /// <summary>
        /// Lista os movimentos de um cofrinho.
        /// </summary>
        /// <param name="userId">Dono do cofrinho.</param>
        /// <param name="savingsGoalId">Identificador do cofrinho.</param>
        /// <param name="limit">Quantidade máxima de itens, dos mais recentes.</param>
        /// <returns>Movimentos do mais recente para o mais antigo.</returns>
        Task<IReadOnlyList<SavingsGoalContribution>> GetContributionsAsync(Guid userId, Guid savingsGoalId, int limit);

        /// <summary>
        /// Obtém um movimento do usuário.
        /// </summary>
        /// <param name="userId">Dono do movimento.</param>
        /// <param name="contributionId">Identificador do movimento.</param>
        /// <returns>O movimento, ou nulo se não existir ou não pertencer ao usuário.</returns>
        Task<SavingsGoalContribution?> GetContributionByIdAsync(Guid userId, Guid contributionId);

        /// <summary>
        /// Persiste um movimento novo.
        /// </summary>
        /// <param name="contribution">Movimento a gravar.</param>
        /// <returns>True se a gravação afetou alguma linha.</returns>
        Task<bool> CreateContributionAsync(SavingsGoalContribution contribution);

        /// <summary>
        /// Remove um movimento.
        /// </summary>
        /// <param name="contribution">Movimento a remover.</param>
        /// <returns>True se a remoção afetou alguma linha.</returns>
        Task<bool> DeleteContributionAsync(SavingsGoalContribution contribution);

        #endregion
    }
}
