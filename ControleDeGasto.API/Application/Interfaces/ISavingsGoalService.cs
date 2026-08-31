using ControleDeGasto.API.Application.DTOs;
using ControleDeGasto.API.Domain.Enums;

namespace ControleDeGasto.API.Application.Interfaces
{
    /// <summary>
    /// Regras de cofrinho, participantes e reserva de emergência.
    /// </summary>
    /// <remarks>
    /// Onde a documentação diz "dono", leia "participante": um cofrinho compartilhado é visível
    /// e movimentável por todos os seus participantes. As operações que mudam a configuração do
    /// cofrinho — editar, convidar, arquivar, excluir — continuam restritas ao criador.
    /// </remarks>
    public interface ISavingsGoalService
    {
        #region Methods :: GetAllAsync(), GetByIdAsync(), CreateAsync(), UpdateAsync(), SetStatusAsync(), DeleteAsync()

        /// <summary>
        /// Lista os cofrinhos do usuário com saldo e progresso calculados.
        /// </summary>
        /// <param name="userId">Dono dos cofrinhos.</param>
        /// <param name="includeArchived">Quando verdadeiro, inclui os arquivados.</param>
        /// <returns>Cofrinhos do usuário.</returns>
        Task<IReadOnlyList<SavingsGoalResponse>> GetAllAsync(Guid userId, bool includeArchived);

        /// <summary>
        /// Obtém um cofrinho do usuário.
        /// </summary>
        /// <param name="userId">Dono do cofrinho.</param>
        /// <param name="savingsGoalId">Identificador do cofrinho.</param>
        /// <returns>O cofrinho, ou nulo se não existir para esse usuário.</returns>
        Task<SavingsGoalResponse?> GetByIdAsync(Guid userId, Guid savingsGoalId);

        /// <summary>
        /// Cria um cofrinho.
        /// </summary>
        /// <param name="userId">Dono do cofrinho.</param>
        /// <param name="request">Dados do cofrinho.</param>
        /// <returns>O cofrinho criado.</returns>
        /// <exception cref="Domain.Exceptions.BusinessRuleViolationException">Nome repetido, prazo no passado ou segunda reserva de emergência.</exception>
        Task<SavingsGoalResponse> CreateAsync(Guid userId, SavingsGoalRequest request);

        /// <summary>
        /// Atualiza um cofrinho.
        /// </summary>
        /// <param name="userId">Dono do cofrinho.</param>
        /// <param name="savingsGoalId">Identificador do cofrinho.</param>
        /// <param name="request">Novos dados.</param>
        /// <returns>O cofrinho atualizado, ou nulo se não existir para esse usuário.</returns>
        /// <exception cref="Domain.Exceptions.BusinessRuleViolationException">Nome repetido ou segunda reserva de emergência.</exception>
        Task<SavingsGoalResponse?> UpdateAsync(Guid userId, Guid savingsGoalId, SavingsGoalRequest request);

        /// <summary>
        /// Altera a situação de um cofrinho (arquivar ou reativar).
        /// </summary>
        /// <param name="userId">Dono do cofrinho.</param>
        /// <param name="savingsGoalId">Identificador do cofrinho.</param>
        /// <param name="status">Nova situação.</param>
        /// <returns>O cofrinho atualizado, ou nulo se não existir para esse usuário.</returns>
        Task<SavingsGoalResponse?> SetStatusAsync(Guid userId, Guid savingsGoalId, SavingsGoalStatus status);

        /// <summary>
        /// Remove um cofrinho e seus movimentos.
        /// </summary>
        /// <param name="userId">Dono do cofrinho.</param>
        /// <param name="savingsGoalId">Identificador do cofrinho.</param>
        /// <returns>True se removeu; false se não existir para esse usuário.</returns>
        Task<bool> DeleteAsync(Guid userId, Guid savingsGoalId);

        #endregion

        #region Methods :: GetContributionsAsync(), AddContributionAsync(), DeleteContributionAsync()

        /// <summary>
        /// Lista os movimentos de um cofrinho.
        /// </summary>
        /// <param name="userId">Dono do cofrinho.</param>
        /// <param name="savingsGoalId">Identificador do cofrinho.</param>
        /// <param name="limit">Quantidade máxima de itens.</param>
        /// <returns>Movimentos do mais recente para o mais antigo, ou nulo se o cofrinho não existir.</returns>
        Task<IReadOnlyList<ContributionResponse>?> GetContributionsAsync(Guid userId, Guid savingsGoalId, int limit);

        /// <summary>
        /// Registra um depósito ou resgate em um cofrinho.
        /// </summary>
        /// <param name="userId">Dono do cofrinho.</param>
        /// <param name="savingsGoalId">Identificador do cofrinho.</param>
        /// <param name="request">Dados do movimento.</param>
        /// <returns>O cofrinho com o saldo atualizado, ou nulo se não existir para esse usuário.</returns>
        /// <exception cref="Domain.Exceptions.BusinessRuleViolationException">Resgate maior que o saldo ou cofrinho arquivado.</exception>
        Task<SavingsGoalResponse?> AddContributionAsync(Guid userId, Guid savingsGoalId, ContributionRequest request);

        /// <summary>
        /// Remove um movimento e reavalia a situação do cofrinho.
        /// </summary>
        /// <param name="userId">Dono do movimento.</param>
        /// <param name="contributionId">Identificador do movimento.</param>
        /// <returns>O cofrinho com o saldo atualizado, ou nulo se o movimento não existir para esse usuário.</returns>
        /// <exception cref="Domain.Exceptions.BusinessRuleViolationException">Remoção deixaria o saldo negativo.</exception>
        Task<SavingsGoalResponse?> DeleteContributionAsync(Guid userId, Guid contributionId);

        #endregion

        #region Methods :: GetMembersAsync(), AddMemberAsync(), RemoveMemberAsync(), LeaveAsync()

        /// <summary>
        /// Lista os participantes de um cofrinho, com quanto cada um aportou.
        /// </summary>
        /// <param name="userId">Participante que consulta.</param>
        /// <param name="savingsGoalId">Identificador do cofrinho.</param>
        /// <returns>Participantes, ou nulo quando o usuário não participa do cofrinho.</returns>
        Task<IReadOnlyList<SavingsGoalMemberResponse>?> GetMembersAsync(Guid userId, Guid savingsGoalId);

        /// <summary>
        /// Adiciona um amigo como participante de um cofrinho.
        /// </summary>
        /// <param name="userId">Criador do cofrinho.</param>
        /// <param name="savingsGoalId">Identificador do cofrinho.</param>
        /// <param name="request">Amigo a adicionar.</param>
        /// <returns>O cofrinho atualizado, ou nulo quando o usuário não participa do cofrinho.</returns>
        /// <exception cref="Domain.Exceptions.BusinessRuleViolationException">Não é o criador, o convidado não é amigo, já participa, ou é a reserva de emergência.</exception>
        Task<SavingsGoalResponse?> AddMemberAsync(Guid userId, Guid savingsGoalId, SavingsGoalMemberRequest request);

        /// <summary>
        /// Remove um participante de um cofrinho.
        /// </summary>
        /// <param name="userId">Criador do cofrinho.</param>
        /// <param name="savingsGoalId">Identificador do cofrinho.</param>
        /// <param name="memberUserId">Participante a remover.</param>
        /// <returns>O cofrinho atualizado, ou nulo quando o cofrinho ou o participante não existem.</returns>
        /// <exception cref="Domain.Exceptions.BusinessRuleViolationException">Não é o criador, tentou remover o dono, ou o participante tem aportes.</exception>
        Task<SavingsGoalResponse?> RemoveMemberAsync(Guid userId, Guid savingsGoalId, Guid memberUserId);

        /// <summary>
        /// Sai de um cofrinho compartilhado.
        /// </summary>
        /// <remarks>
        /// Separado de <see cref="RemoveMemberAsync"/> porque a permissão é oposta: remover é
        /// coisa do criador, sair é direito de quem foi convidado.
        /// </remarks>
        /// <param name="userId">Participante que está saindo.</param>
        /// <param name="savingsGoalId">Identificador do cofrinho.</param>
        /// <returns>True se saiu; false quando o usuário não participa do cofrinho.</returns>
        /// <exception cref="Domain.Exceptions.BusinessRuleViolationException">O criador não pode sair, ou o participante tem aportes.</exception>
        Task<bool> LeaveAsync(Guid userId, Guid savingsGoalId);

        #endregion
    }
}
