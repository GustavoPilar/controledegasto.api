using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;
using ControleDeGasto.API.Domain.ReadModels;

namespace ControleDeGasto.API.Domain.Interfaces
{
    /// <summary>
    /// Acesso a cofrinhos, participantes e movimentos.
    /// </summary>
    /// <remarks>
    /// O recorte de acesso é a participação, não a posse: um cofrinho compartilhado é visível
    /// para todos os seus participantes, e o saldo é a soma dos aportes de todos eles. Onde o
    /// método recebe apenas o identificador do cofrinho, a permissão foi verificada antes.
    /// </remarks>
    public interface ISavingsGoalRepository
    {
        #region Methods :: GetAllAsync(), GetByIdAsync(), GetEmergencyReserveAsync(), ExistsByNameAsync()

        /// <summary>
        /// Lista os cofrinhos em que o usuário participa, com os participantes carregados.
        /// </summary>
        /// <param name="userId">Participante dos cofrinhos.</param>
        /// <param name="includeArchived">Quando verdadeiro, inclui os arquivados.</param>
        /// <returns>Cofrinhos com a reserva de emergência primeiro.</returns>
        Task<IReadOnlyList<SavingsGoal>> GetAllAsync(Guid userId, bool includeArchived);

        /// <summary>
        /// Obtém um cofrinho em que o usuário participa.
        /// </summary>
        /// <param name="userId">Participante do cofrinho.</param>
        /// <param name="savingsGoalId">Identificador do cofrinho.</param>
        /// <returns>O cofrinho, ou nulo se não existir ou o usuário não participar dele.</returns>
        Task<SavingsGoal?> GetByIdAsync(Guid userId, Guid savingsGoalId);

        /// <summary>
        /// Obtém a reserva de emergência do usuário.
        /// </summary>
        /// <remarks>
        /// Procura pela posse, e não pela participação: a reserva é individual por definição, e
        /// uma reserva de outra pessoa não deve entrar no cálculo do usuário só porque ele
        /// participa dela.
        /// </remarks>
        /// <param name="userId">Dono da reserva.</param>
        /// <returns>A reserva, ou nulo se ainda não foi criada.</returns>
        Task<SavingsGoal?> GetEmergencyReserveAsync(Guid userId);

        /// <summary>
        /// Verifica se o usuário já criou cofrinho com o mesmo nome.
        /// </summary>
        /// <param name="userId">Dono do cofrinho.</param>
        /// <param name="name">Nome a verificar.</param>
        /// <param name="excludeSavingsGoalId">Cofrinho a ignorar na verificação (usado na edição).</param>
        /// <returns>True se já existir.</returns>
        Task<bool> ExistsByNameAsync(Guid userId, string name, Guid? excludeSavingsGoalId);

        #endregion

        #region Methods :: CreateAsync(), UpdateAsync(), DeleteAsync()

        /// <summary>
        /// Persiste um cofrinho novo junto da participação do criador.
        /// </summary>
        /// <remarks>
        /// As duas gravações são atômicas: um cofrinho sem linha de dono ficaria invisível até
        /// para quem o criou, já que o acesso é decidido pela participação.
        /// </remarks>
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
        /// Remove um cofrinho e, em cascata, seus movimentos e participantes.
        /// </summary>
        /// <param name="savingsGoal">Cofrinho a remover.</param>
        /// <returns>True se a remoção afetou alguma linha.</returns>
        Task<bool> DeleteAsync(SavingsGoal savingsGoal);

        #endregion

        #region Methods :: GetMembersAsync(), GetMemberAsync(), AddMemberAsync(), RemoveMemberAsync(), IsMemberAsync()

        /// <summary>
        /// Lista os participantes de um cofrinho, com os usuários carregados.
        /// </summary>
        /// <param name="savingsGoalId">Identificador do cofrinho.</param>
        /// <returns>Participantes com o dono primeiro.</returns>
        Task<IReadOnlyList<SavingsGoalMember>> GetMembersAsync(Guid savingsGoalId);

        /// <summary>
        /// Obtém a participação de um usuário em um cofrinho.
        /// </summary>
        /// <param name="savingsGoalId">Identificador do cofrinho.</param>
        /// <param name="userId">Usuário desejado.</param>
        /// <returns>A participação, ou nulo quando o usuário não participa.</returns>
        Task<SavingsGoalMember?> GetMemberAsync(Guid savingsGoalId, Guid userId);

        /// <summary>
        /// Persiste uma participação nova.
        /// </summary>
        /// <param name="member">Participação a gravar.</param>
        /// <returns>True se a gravação afetou alguma linha.</returns>
        Task<bool> AddMemberAsync(SavingsGoalMember member);

        /// <summary>
        /// Remove uma participação.
        /// </summary>
        /// <param name="member">Participação a remover.</param>
        /// <returns>True se a remoção afetou alguma linha.</returns>
        Task<bool> RemoveMemberAsync(SavingsGoalMember member);

        /// <summary>
        /// Verifica se um usuário participa de um cofrinho.
        /// </summary>
        /// <param name="savingsGoalId">Identificador do cofrinho.</param>
        /// <param name="userId">Usuário a verificar.</param>
        /// <returns>True quando existe participação.</returns>
        Task<bool> IsMemberAsync(Guid savingsGoalId, Guid userId);

        #endregion

        #region Methods :: GetBalanceAsync(), GetBalancesAsync(), GetMemberBalancesAsync()

        /// <summary>
        /// Calcula o saldo de um cofrinho, somando os aportes de todos os participantes.
        /// </summary>
        /// <param name="savingsGoalId">Identificador do cofrinho.</param>
        /// <returns>Saldo acumulado, zero quando não há movimentos.</returns>
        Task<decimal> GetBalanceAsync(Guid savingsGoalId);

        /// <summary>
        /// Calcula em uma única consulta o saldo de todos os cofrinhos em que o usuário participa.
        /// </summary>
        /// <param name="userId">Participante dos cofrinhos.</param>
        /// <returns>Saldo por cofrinho que possua movimentos.</returns>
        Task<IReadOnlyList<GoalBalance>> GetBalancesAsync(Guid userId);

        /// <summary>
        /// Apura quanto cada participante aportou em um cofrinho.
        /// </summary>
        /// <param name="savingsGoalId">Identificador do cofrinho.</param>
        /// <returns>Saldo aportado por participante que possua movimentos.</returns>
        Task<IReadOnlyDictionary<Guid, decimal>> GetMemberBalancesAsync(Guid savingsGoalId);

        #endregion

        #region Methods :: GetContributionsAsync(), GetContributionByIdAsync(), CreateContributionAsync(), DeleteContributionAsync()

        /// <summary>
        /// Lista os movimentos de um cofrinho, de todos os participantes.
        /// </summary>
        /// <param name="savingsGoalId">Identificador do cofrinho.</param>
        /// <param name="limit">Quantidade máxima de itens, dos mais recentes.</param>
        /// <returns>Movimentos do mais recente para o mais antigo, com o autor carregado.</returns>
        Task<IReadOnlyList<SavingsGoalContribution>> GetContributionsAsync(Guid savingsGoalId, int limit);

        /// <summary>
        /// Obtém um movimento feito pelo usuário.
        /// </summary>
        /// <remarks>
        /// Filtra pelo autor de propósito: em um cofrinho compartilhado, cada participante
        /// remove apenas os próprios aportes.
        /// </remarks>
        /// <param name="userId">Autor do movimento.</param>
        /// <param name="contributionId">Identificador do movimento.</param>
        /// <returns>O movimento, ou nulo se não existir ou não for do usuário.</returns>
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
