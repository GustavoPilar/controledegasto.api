using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Movimento de cofrinho devolvido ao cliente.
    /// </summary>
    public class ContributionResponse(SavingsGoalContribution contribution)
    {
        #region Properties :: Id, SavingsGoalId, Amount, Kind, OccurredOn, Note, CreatedAt

        public Guid Id { get; set; } = contribution.Id;

        public Guid SavingsGoalId { get; set; } = contribution.SavingsGoalId;

        public decimal Amount { get; set; } = contribution.Amount;

        public ContributionKind Kind { get; set; } = contribution.Kind;

        public DateTime OccurredOn { get; set; } = contribution.OccurredOn;

        public string? Note { get; set; } = contribution.Note;

        public DateTime CreatedAt { get; set; } = contribution.CreatedAt;

        #endregion
    }
}
