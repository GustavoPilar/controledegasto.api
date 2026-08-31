using ControleDeGasto.API.Domain.Entities;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Transferência devolvida ao cliente, com os nomes das duas carteiras para a listagem
    /// não precisar de uma segunda chamada.
    /// </summary>
    public class WalletTransferResponse(WalletTransfer transfer)
    {
        #region Properties :: Id, FromWalletId, FromWalletName, FromWalletColor, ToWalletId, ToWalletName, ToWalletColor, Amount, OccurredOn, Note, CreatedAt

        public Guid Id { get; set; } = transfer.Id;

        public Guid FromWalletId { get; set; } = transfer.FromWalletId;

        public string FromWalletName { get; set; } = transfer.FromWallet?.Name ?? string.Empty;

        public string FromWalletColor { get; set; } = transfer.FromWallet?.Color ?? string.Empty;

        public Guid ToWalletId { get; set; } = transfer.ToWalletId;

        public string ToWalletName { get; set; } = transfer.ToWallet?.Name ?? string.Empty;

        public string ToWalletColor { get; set; } = transfer.ToWallet?.Color ?? string.Empty;

        public decimal Amount { get; set; } = transfer.Amount;

        public DateTime OccurredOn { get; set; } = transfer.OccurredOn;

        public string? Note { get; set; } = transfer.Note;

        public DateTime CreatedAt { get; set; } = transfer.CreatedAt;

        #endregion
    }
}
