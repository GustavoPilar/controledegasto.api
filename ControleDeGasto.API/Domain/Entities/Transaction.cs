using ControleDeGasto.API.Domain.Enums;

namespace ControleDeGasto.API.Domain.Entities
{
    /// <summary>
    /// Lançamento financeiro: uma entrada ou saída de dinheiro em uma data.
    /// </summary>
    /// <remarks>
    /// A natureza (entrada/saída) não é gravada aqui: ela vem de <see cref="Category"/>.
    /// Duplicar o dado abriria espaço para lançamento marcado como entrada apontando para
    /// categoria de saída, e nenhum relatório conseguiria decidir qual dos dois está certo.
    /// </remarks>
    public class Transaction
    {
        #region Properties :: Id, UserId, CategoryId, Amount, Description, OccurredOn, PaymentMethod, CreatedAt, UpdatedAt, User, Category

        public Guid Id { get; set; }

        /// <summary>Dono do lançamento.</summary>
        public Guid UserId { get; set; }

        public Guid CategoryId { get; set; }

        /// <summary>Valor sempre positivo. O sentido do dinheiro vem do tipo da categoria.</summary>
        public decimal Amount { get; set; }

        public string Description { get; set; } = string.Empty;

        /// <summary>Data de competência do lançamento, em UTC.</summary>
        public DateTime OccurredOn { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public User? User { get; set; }

        public Category? Category { get; set; }

        #endregion
    }
}
