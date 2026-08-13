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
}
