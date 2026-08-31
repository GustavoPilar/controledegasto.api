using ControleDeGasto.API.Domain.Enums;

namespace ControleDeGasto.API.Domain.Entities
{
    /// <summary>
    /// Valor que se repete todo mês: salário, aluguel, plano de celular ou o crédito de um
    /// benefício (VR, VA, VT, VC).
    /// </summary>
    /// <remarks>
    /// Deliberadamente não gera lançamento. Quem já sabe que recebe o mesmo salário e paga o
    /// mesmo aluguel não precisa de doze linhas por ano por conta fixa só para o painel exibir
    /// um número que a própria definição já contém. A previsão é calculada a partir daqui, e o
    /// usuário lança à mão apenas o que fugir do combinado.
    /// </remarks>
    public class FixedEntry
    {
        #region Properties :: Id, UserId, Kind, CategoryId, WalletId, Description, Amount, DayOfMonth, StartsOn, EndsOn, Active, CreatedAt, UpdatedAt, User, Category, Wallet

        public Guid Id { get; set; }

        /// <summary>Dono da definição.</summary>
        public Guid UserId { get; set; }

        public FixedEntryKind Kind { get; set; }

        /// <summary>
        /// Categoria usada na previsão por categoria. Nula em crédito de benefício, que não
        /// classifica gasto nenhum: apenas abastece uma carteira.
        /// </summary>
        public Guid? CategoryId { get; set; }

        /// <summary>Carteira de origem ou destino. Obrigatória no crédito de benefício.</summary>
        public Guid? WalletId { get; set; }

        public string Description { get; set; } = string.Empty;

        /// <summary>Valor mensal. Sempre positivo; o sentido vem de <see cref="Kind"/>.</summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Dia do mês em que acontece (1 a 31). Meses mais curtos usam o último dia disponível.
        /// </summary>
        public int DayOfMonth { get; set; }

        /// <summary>Primeiro mês em que a definição vale, em UTC.</summary>
        public DateTime StartsOn { get; set; }

        /// <summary>Último mês em que a definição vale, em UTC. Nulo enquanto não tem fim.</summary>
        public DateTime? EndsOn { get; set; }

        /// <summary>Falso quando pausada: deixa de entrar na previsão sem perder o histórico.</summary>
        public bool Active { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public User? User { get; set; }

        public Category? Category { get; set; }

        public Wallet? Wallet { get; set; }

        #endregion
    }
}
