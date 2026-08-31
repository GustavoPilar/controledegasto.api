using ControleDeGasto.API.Domain.Enums;

namespace ControleDeGasto.API.Domain.ReadModels
{
    /// <summary>
    /// Resultado paginado genérico devolvido pelos repositórios.
    /// </summary>
    /// <typeparam name="T">Tipo dos itens da página.</typeparam>
    /// <param name="Items">Itens da página solicitada.</param>
    /// <param name="TotalCount">Total de registros que atendem ao filtro, ignorando a paginação.</param>
    public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount);

    /// <summary>
    /// Total movimentado em uma categoria dentro de um período.
    /// </summary>
    /// <remarks>
    /// Propriedades <c>init</c> em vez de construtor posicional: o EF Core precisa projetar
    /// o resultado de um GROUP BY com inicialização de membros. Com construtor, a consulta
    /// não é traduzida para SQL e a execução falha em tempo de execução.
    /// </remarks>
    public sealed record CategoryTotal
    {
        public Guid CategoryId { get; init; }

        public string CategoryName { get; init; } = string.Empty;

        public string Color { get; init; } = string.Empty;

        public string Icon { get; init; } = string.Empty;

        public TransactionType Type { get; init; }

        public decimal Total { get; init; }

        public int TransactionCount { get; init; }
    }

    /// <summary>
    /// Total movimentado em um mês, por natureza do lançamento.
    /// </summary>
    public sealed record MonthlyTotal
    {
        public int Year { get; init; }

        public int Month { get; init; }

        public TransactionType Type { get; init; }

        public decimal Total { get; init; }
    }

    /// <summary>
    /// Total movimentado por natureza do lançamento em um período.
    /// </summary>
    public sealed record TypeTotal
    {
        public TransactionType Type { get; init; }

        public decimal Total { get; init; }
    }

    /// <summary>
    /// Saldo acumulado de um cofrinho (depósitos menos resgates).
    /// </summary>
    public sealed record GoalBalance
    {
        public Guid SavingsGoalId { get; init; }

        public decimal Balance { get; init; }
    }

    /// <summary>
    /// Notificação pendente de envio por e-mail, já com os dados do destinatário.
    /// </summary>
    public sealed record PendingEmailNotification
    {
        public Guid NotificationId { get; init; }

        public Guid UserId { get; init; }

        public string Email { get; init; } = string.Empty;

        public string FullName { get; init; } = string.Empty;

        public string Title { get; init; } = string.Empty;

        public string Message { get; init; } = string.Empty;
    }

    /// <summary>
    /// Dados públicos de um usuário, usados nas telas sociais.
    /// </summary>
    /// <remarks>
    /// Projeção restrita de propósito: as telas de amizade precisam de nome e apelido para
    /// identificar a pessoa, e nada mais. Devolver a entidade inteira exporia hash de senha,
    /// selo de segurança e estado de bloqueio a qualquer amigo.
    /// </remarks>
    public sealed record UserSummary
    {
        public Guid UserId { get; init; }

        public string FullName { get; init; } = string.Empty;

        public string UserName { get; init; } = string.Empty;
    }

    /// <summary>
    /// Saldo apurado de uma carteira.
    /// </summary>
    public sealed record WalletBalance
    {
        public Guid WalletId { get; init; }

        /// <summary>Entradas liquidadas menos saídas liquidadas, sem o saldo inicial.</summary>
        public decimal MovementBalance { get; init; }

        /// <summary>Saídas ainda previstas, que vão consumir o saldo quando liquidadas.</summary>
        public decimal PendingExpense { get; init; }

        /// <summary>Entradas ainda previstas.</summary>
        public decimal PendingIncome { get; init; }
    }

    /// <summary>
    /// Soma das transferências de uma carteira, nos dois sentidos.
    /// </summary>
    public sealed record WalletTransferTotal
    {
        public Guid WalletId { get; init; }

        public decimal TransferredIn { get; init; }

        public decimal TransferredOut { get; init; }
    }

    /// <summary>
    /// Total previsto por natureza, apurado sobre os lançamentos ainda não liquidados.
    /// </summary>
    public sealed record PendingTypeTotal
    {
        public TransactionType Type { get; init; }

        public decimal Total { get; init; }

        /// <summary>Parte do total cujo vencimento já passou.</summary>
        public decimal OverdueTotal { get; init; }

        public int TransactionCount { get; init; }
    }

    /// <summary>
    /// Total movimentado em uma etiqueta dentro de um período.
    /// </summary>
    public sealed record TagTotal
    {
        public Guid TagId { get; init; }

        public string TagName { get; init; } = string.Empty;

        public string Color { get; init; } = string.Empty;

        public decimal IncomeTotal { get; init; }

        public decimal ExpenseTotal { get; init; }

        public int TransactionCount { get; init; }
    }

    /// <summary>
    /// Saldo de divisões de compra com um amigo.
    /// </summary>
    /// <remarks>
    /// Os dois sentidos vêm na mesma linha porque a tela mostra um número por amigo: quanto
    /// ele me deve menos quanto eu devo a ele.
    /// </remarks>
    public sealed record FriendBalance
    {
        public Guid FriendUserId { get; init; }

        /// <summary>Quanto o amigo deve ao usuário, em divisões ainda em aberto.</summary>
        public decimal Receivable { get; init; }

        /// <summary>Quanto o usuário deve ao amigo, em divisões ainda em aberto.</summary>
        public decimal Payable { get; init; }
    }
}
