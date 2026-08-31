using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Movimento de cofrinho devolvido ao cliente, com o autor identificado.
    /// </summary>
    /// <remarks>
    /// O autor acompanha a resposta porque em cofrinho compartilhado a lista de movimentos é de
    /// todos os participantes: sem o nome, não se sabe quem depositou o quê.
    /// </remarks>
    public class ContributionResponse
    {
        #region Constructors

        /// <summary>
        /// Cria a resposta a partir do movimento.
        /// </summary>
        /// <param name="contribution">Movimento de origem.</param>
        /// <param name="currentUserId">Usuário que consulta, para marcar os próprios movimentos.</param>
        public ContributionResponse(SavingsGoalContribution contribution, Guid? currentUserId = null)
        {
            ArgumentNullException.ThrowIfNull(contribution);

            this.Id = contribution.Id;
            this.SavingsGoalId = contribution.SavingsGoalId;
            this.Amount = contribution.Amount;
            this.Kind = contribution.Kind;
            this.OccurredOn = contribution.OccurredOn;
            this.Note = contribution.Note;
            this.CreatedAt = contribution.CreatedAt;

            this.UserId = contribution.UserId;
            this.UserFullName = contribution.User?.FullName ?? string.Empty;
            this.UserName = contribution.User?.UserName ?? string.Empty;

            // A tela só oferece exclusão nos movimentos do próprio usuário, e é a API que decide
            // isso: deixar a regra apenas no cliente permitiria remover o aporte de outra pessoa.
            this.IsMine = currentUserId.HasValue && contribution.UserId == currentUserId.Value;
        }

        #endregion

        #region Properties :: Id, SavingsGoalId, Amount, Kind, OccurredOn, Note, CreatedAt, UserId, UserFullName, UserName, IsMine

        public Guid Id { get; set; }

        public Guid SavingsGoalId { get; set; }

        public decimal Amount { get; set; }

        public ContributionKind Kind { get; set; }

        public DateTime OccurredOn { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid UserId { get; set; }

        public string UserFullName { get; set; }

        public string UserName { get; set; }

        /// <summary>Verdadeiro quando o movimento é de quem consulta.</summary>
        public bool IsMine { get; set; }

        #endregion
    }
}
