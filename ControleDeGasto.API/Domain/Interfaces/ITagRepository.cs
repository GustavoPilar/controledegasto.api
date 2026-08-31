using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.ReadModels;

namespace ControleDeGasto.API.Domain.Interfaces
{
    /// <summary>
    /// Acesso às etiquetas e aos vínculos com lançamentos.
    /// </summary>
    public interface ITagRepository
    {
        #region Methods :: GetAllAsync(), GetByIdAsync(), FilterOwnedAsync(), ExistsByNameAsync()

        /// <summary>
        /// Lista as etiquetas do usuário.
        /// </summary>
        /// <param name="userId">Dono das etiquetas.</param>
        /// <returns>Etiquetas em ordem alfabética.</returns>
        Task<IReadOnlyList<Tag>> GetAllAsync(Guid userId);

        /// <summary>
        /// Obtém uma etiqueta do usuário.
        /// </summary>
        /// <param name="userId">Dono da etiqueta.</param>
        /// <param name="tagId">Identificador da etiqueta.</param>
        /// <returns>A etiqueta, ou nulo se não existir ou não pertencer ao usuário.</returns>
        Task<Tag?> GetByIdAsync(Guid userId, Guid tagId);

        /// <summary>
        /// Filtra, entre os identificadores informados, os que pertencem ao usuário.
        /// </summary>
        /// <remarks>
        /// Uma consulta para o conjunto todo: marcar um lançamento com cinco etiquetas não deve
        /// custar cinco verificações de posse.
        /// </remarks>
        /// <param name="userId">Dono das etiquetas.</param>
        /// <param name="tagIds">Identificadores a verificar.</param>
        /// <returns>Subconjunto que pertence ao usuário.</returns>
        Task<IReadOnlyList<Guid>> FilterOwnedAsync(Guid userId, IReadOnlyList<Guid> tagIds);

        /// <summary>
        /// Verifica se já existe etiqueta com o mesmo nome.
        /// </summary>
        /// <param name="userId">Dono da etiqueta.</param>
        /// <param name="name">Nome a verificar.</param>
        /// <param name="excludeTagId">Etiqueta a ignorar na verificação (usada na edição).</param>
        /// <returns>True se já existir.</returns>
        Task<bool> ExistsByNameAsync(Guid userId, string name, Guid? excludeTagId);

        #endregion

        #region Methods :: CreateAsync(), UpdateAsync(), DeleteAsync(), GetUsageCountAsync()

        /// <summary>
        /// Persiste uma etiqueta nova.
        /// </summary>
        /// <param name="tag">Etiqueta a gravar.</param>
        /// <returns>True se a gravação afetou alguma linha.</returns>
        Task<bool> CreateAsync(Tag tag);

        /// <summary>
        /// Persiste alterações de uma etiqueta.
        /// </summary>
        /// <param name="tag">Etiqueta alterada.</param>
        /// <returns>True se a gravação afetou alguma linha.</returns>
        Task<bool> UpdateAsync(Tag tag);

        /// <summary>
        /// Remove uma etiqueta e, em cascata, seus vínculos.
        /// </summary>
        /// <param name="tag">Etiqueta a remover.</param>
        /// <returns>True se a remoção afetou alguma linha.</returns>
        Task<bool> DeleteAsync(Tag tag);

        /// <summary>
        /// Conta em quantos lançamentos cada etiqueta do usuário está aplicada.
        /// </summary>
        /// <param name="userId">Dono das etiquetas.</param>
        /// <returns>Quantidade de usos por identificador de etiqueta.</returns>
        Task<IReadOnlyDictionary<Guid, int>> GetUsageCountAsync(Guid userId);

        #endregion

        #region Methods :: GetTotalsAsync()

        /// <summary>
        /// Soma, por etiqueta, o que foi movimentado em um período.
        /// </summary>
        /// <param name="userId">Dono dos lançamentos.</param>
        /// <param name="from">Início do período, em UTC.</param>
        /// <param name="to">Fim do período, em UTC.</param>
        /// <returns>Totais por etiqueta, da que mais movimentou para a que menos movimentou.</returns>
        Task<IReadOnlyList<TagTotal>> GetTotalsAsync(Guid userId, DateTime from, DateTime to);

        #endregion
    }
}
