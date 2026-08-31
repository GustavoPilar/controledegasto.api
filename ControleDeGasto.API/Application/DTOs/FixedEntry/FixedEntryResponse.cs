using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Valor fixo mensal devolvido ao cliente, com categoria e carteira embutidas.
    /// </summary>
    public class FixedEntryResponse(FixedEntry fixedEntry)
    {
        #region Properties :: Id, Kind, Description, Amount, DayOfMonth, StartsOn, EndsOn, Active, CreatedAt, UpdatedAt

        public Guid Id { get; set; } = fixedEntry.Id;

        public FixedEntryKind Kind { get; set; } = fixedEntry.Kind;

        public string Description { get; set; } = fixedEntry.Description;

        public decimal Amount { get; set; } = fixedEntry.Amount;

        public int DayOfMonth { get; set; } = fixedEntry.DayOfMonth;

        public DateTime StartsOn { get; set; } = fixedEntry.StartsOn;

        public DateTime? EndsOn { get; set; } = fixedEntry.EndsOn;

        public bool Active { get; set; } = fixedEntry.Active;

        public DateTime CreatedAt { get; set; } = fixedEntry.CreatedAt;

        public DateTime? UpdatedAt { get; set; } = fixedEntry.UpdatedAt;

        #endregion

        #region Properties :: CategoryId, CategoryName, CategoryColor, CategoryIcon, WalletId, WalletName, WalletColor, WalletKind

        public Guid? CategoryId { get; set; } = fixedEntry.CategoryId;

        public string? CategoryName { get; set; } = fixedEntry.Category?.Name;

        public string? CategoryColor { get; set; } = fixedEntry.Category?.Color;

        public string? CategoryIcon { get; set; } = fixedEntry.Category?.Icon;

        public Guid? WalletId { get; set; } = fixedEntry.WalletId;

        public string? WalletName { get; set; } = fixedEntry.Wallet?.Name;

        public string? WalletColor { get; set; } = fixedEntry.Wallet?.Color;

        public WalletKind? WalletKind { get; set; } = fixedEntry.Wallet?.Kind;

        #endregion
    }
}
