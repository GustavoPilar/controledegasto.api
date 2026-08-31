using ControleDeGasto.API.Domain.Enums;

namespace ControleDeGasto.API.Domain.Entities
{
    /// <summary>
    /// Carteira de onde o dinheiro sai e para onde entra: conta corrente, dinheiro em espécie,
    /// cartão de crédito ou benefício (VR, VA, VT, VC).
    /// </summary>
    /// <remarks>
    /// O saldo não é armazenado: é o saldo inicial somado aos lançamentos liquidados e às
    /// transferências. Um total mutável perderia atualizações em lançamentos concorrentes e
    /// divergiria do extrato na primeira exclusão retroativa.
    /// </remarks>
    public class Wallet
    {
        #region Properties :: Id, UserId, Name, Kind, Color, Icon, InitialBalance, CreditLimit, StatementClosingDay, PaymentDueDay, IsDefault, Active, CreatedAt, UpdatedAt, User

        public Guid Id { get; set; }

        /// <summary>Dono da carteira.</summary>
        public Guid UserId { get; set; }

        public string Name { get; set; } = string.Empty;

        public WalletKind Kind { get; set; }

        /// <summary>Cor em hexadecimal (#RRGGBB), usada nos gráficos.</summary>
        public string Color { get; set; } = string.Empty;

        /// <summary>Nome do ícone exibido na interface.</summary>
        public string Icon { get; set; } = string.Empty;

        /// <summary>Saldo existente quando a carteira foi cadastrada.</summary>
        public decimal InitialBalance { get; set; }

        /// <summary>Limite do cartão de crédito. Nulo nas demais naturezas.</summary>
        public decimal? CreditLimit { get; set; }

        /// <summary>Dia do fechamento da fatura (1 a 31). Nulo fora do cartão de crédito.</summary>
        public int? StatementClosingDay { get; set; }

        /// <summary>Dia do vencimento da fatura (1 a 31). Nulo fora do cartão de crédito.</summary>
        public int? PaymentDueDay { get; set; }

        /// <summary>
        /// Carteira assumida quando o lançamento não informa uma. Cada usuário tem no máximo
        /// uma, garantido por índice único filtrado.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>Falso quando excluída logicamente.</summary>
        public bool Active { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public User? User { get; set; }

        #endregion
    }
}
