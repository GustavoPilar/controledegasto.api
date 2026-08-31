using ControleDeGasto.API.Domain.Entities;

namespace ControleDeGasto.API.Domain.Interfaces
{
    /// <summary>
    /// Acesso às definições de valores fixos mensais. Todo método recebe o dono e filtra por ele.
    /// </summary>
    public interface IFixedEntryRepository
    {
        #region Methods :: GetAllAsync(), GetActiveForMonthAsync(), GetByIdAsync(), ExistsByDescriptionAsync()

        /// <summary>
        /// Lista as definições do usuário, com categoria e carteira carregadas.
        /// </summary>
        /// <param name="userId">Dono das definições.</param>
        /// <param name="includeInactive">Quando verdadeiro, inclui as pausadas.</param>
        /// <returns>Definições agrupadas por natureza e ordenadas pelo dia do mês.</returns>
        Task<IReadOnlyList<FixedEntry>> GetAllAsync(Guid userId, bool includeInactive);

        /// <summary>
        /// Lista as definições ativas que valem em um mês.
        /// </summary>
        /// <remarks>
        /// A vigência é comparada no banco em vez de no cliente: uma conta encerrada em março
        /// não pode aparecer na previsão de agosto, e trazer tudo para filtrar em memória
        /// cresceria com o histórico de definições encerradas.
        /// </remarks>
        /// <param name="userId">Dono das definições.</param>
        /// <param name="monthStart">Primeiro instante do mês de referência, em UTC.</param>
        /// <param name="monthEnd">Último instante do mês de referência, em UTC.</param>
        /// <returns>Definições vigentes no mês.</returns>
        Task<IReadOnlyList<FixedEntry>> GetActiveForMonthAsync(Guid userId, DateTime monthStart, DateTime monthEnd);

        /// <summary>
        /// Obtém uma definição do usuário.
        /// </summary>
        /// <param name="userId">Dono da definição.</param>
        /// <param name="fixedEntryId">Identificador da definição.</param>
        /// <returns>A definição, ou nulo se não existir ou não pertencer ao usuário.</returns>
        Task<FixedEntry?> GetByIdAsync(Guid userId, Guid fixedEntryId);

        /// <summary>
        /// Verifica se já existe definição com a mesma descrição e natureza.
        /// </summary>
        /// <param name="userId">Dono da definição.</param>
        /// <param name="kind">Natureza a considerar.</param>
        /// <param name="description">Descrição a verificar.</param>
        /// <param name="excludeFixedEntryId">Definição a ignorar na verificação (usada na edição).</param>
        /// <returns>True se já existir.</returns>
        Task<bool> ExistsByDescriptionAsync(Guid userId, Enums.FixedEntryKind kind, string description, Guid? excludeFixedEntryId);

        #endregion

        #region Methods :: CreateAsync(), UpdateAsync(), DeleteAsync()

        /// <summary>
        /// Persiste uma definição nova.
        /// </summary>
        /// <param name="fixedEntry">Definição a gravar.</param>
        /// <returns>True se a gravação afetou alguma linha.</returns>
        Task<bool> CreateAsync(FixedEntry fixedEntry);

        /// <summary>
        /// Persiste alterações de uma definição.
        /// </summary>
        /// <param name="fixedEntry">Definição alterada.</param>
        /// <returns>True se a gravação afetou alguma linha.</returns>
        Task<bool> UpdateAsync(FixedEntry fixedEntry);

        /// <summary>
        /// Remove uma definição.
        /// </summary>
        /// <param name="fixedEntry">Definição a remover.</param>
        /// <returns>True se a remoção afetou alguma linha.</returns>
        Task<bool> DeleteAsync(FixedEntry fixedEntry);

        #endregion
    }
}
