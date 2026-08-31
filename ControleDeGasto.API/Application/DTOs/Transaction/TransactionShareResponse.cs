using ControleDeGasto.API.Domain.Entities;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Parte de uma compra dividida, devolvida ao cliente.
    /// </summary>
    public class TransactionShareResponse(TransactionShare share)
    {
        #region Properties :: Id, TransactionId, FriendUserId, FriendFullName, FriendUserName, Amount, SettledAt, IsSettled, CreatedAt

        public Guid Id { get; set; } = share.Id;

        public Guid TransactionId { get; set; } = share.TransactionId;

        public Guid FriendUserId { get; set; } = share.FriendUserId;

        public string FriendFullName { get; set; } = share.FriendUser?.FullName ?? string.Empty;

        public string FriendUserName { get; set; } = share.FriendUser?.UserName ?? string.Empty;

        public decimal Amount { get; set; } = share.Amount;

        public DateTime? SettledAt { get; set; } = share.SettledAt;

        /// <summary>Verdadeiro quando a parte já foi acertada.</summary>
        public bool IsSettled { get; set; } = share.SettledAt.HasValue;

        public DateTime CreatedAt { get; set; } = share.CreatedAt;

        #endregion
    }
}
