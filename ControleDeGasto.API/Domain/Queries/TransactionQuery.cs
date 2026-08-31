using ControleDeGasto.API.Domain.Enums;

namespace ControleDeGasto.API.Domain.Queries
{
    /// <summary>
    /// Filtros de consulta de lançamentos.
    /// </summary>
    /// <remarks>
    /// <paramref name="UserId"/> é obrigatório e nunca vem do corpo da requisição: é resolvido
    /// a partir da identidade autenticada, para que um usuário não consiga ler dados de outro.
    /// </remarks>
    /// <param name="UserId">Dono dos lançamentos.</param>
    /// <param name="From">Início do período de competência, em UTC. Nulo remove o limite inferior.</param>
    /// <param name="To">Fim do período de competência, em UTC. Nulo remove o limite superior.</param>
    /// <param name="CategoryId">Filtra por categoria. Nulo traz todas.</param>
    /// <param name="Type">Filtra por natureza. Nulo traz entradas e saídas.</param>
    /// <param name="Search">Trecho da descrição a procurar. Nulo ignora o filtro.</param>
    /// <param name="WalletId">Filtra pela carteira que pagou ou recebeu. Nulo traz todas.</param>
    /// <param name="TagIds">Etiquetas exigidas. O lançamento entra se tiver ao menos uma delas.</param>
    /// <param name="Status">Filtra por situação de liquidação. Nulo traz previstos e liquidados.</param>
    /// <param name="PaymentMethod">Filtra pela forma de pagamento. Nulo traz todas.</param>
    /// <param name="MinAmount">Valor mínimo. Nulo remove o limite inferior.</param>
    /// <param name="MaxAmount">Valor máximo. Nulo remove o limite superior.</param>
    /// <param name="DueFrom">Início do período de vencimento, em UTC. Nulo ignora o filtro.</param>
    /// <param name="DueTo">Fim do período de vencimento, em UTC. Nulo ignora o filtro.</param>
    /// <param name="OnlyOverdue">Quando verdadeiro, traz apenas os previstos com vencimento passado.</param>
    /// <param name="OnlyShared">Quando verdadeiro, traz apenas os lançamentos divididos com amigos.</param>
    /// <param name="OnlyInstallments">Quando verdadeiro, traz apenas parcelas de compras parceladas.</param>
    /// <param name="InstallmentPlanId">Filtra as parcelas de uma compra parcelada. Nulo ignora o filtro.</param>
    /// <param name="Reference">Momento usado como "hoje" ao resolver <paramref name="OnlyOverdue"/>, em UTC.</param>
    /// <param name="SortBy">Campo de ordenação.</param>
    /// <param name="SortDescending">Sentido da ordenação.</param>
    /// <param name="Page">Página solicitada, iniciando em 1.</param>
    /// <param name="PageSize">Quantidade de itens por página.</param>
    public sealed record TransactionQuery(
        Guid UserId,
        DateTime? From,
        DateTime? To,
        Guid? CategoryId,
        TransactionType? Type,
        string? Search,
        Guid? WalletId,
        IReadOnlyList<Guid>? TagIds,
        TransactionStatus? Status,
        PaymentMethod? PaymentMethod,
        decimal? MinAmount,
        decimal? MaxAmount,
        DateTime? DueFrom,
        DateTime? DueTo,
        bool OnlyOverdue,
        bool OnlyShared,
        bool OnlyInstallments,
        Guid? InstallmentPlanId,
        DateTime Reference,
        TransactionSortField SortBy,
        bool SortDescending,
        int Page,
        int PageSize);

    /// <summary>
    /// Campos pelos quais o extrato pode ser ordenado.
    /// </summary>
    /// <remarks>
    /// Enum, e não string vinda do cliente: o nome do campo entra em uma cláusula ORDER BY, e
    /// aceitar texto livre transformaria a ordenação em superfície de injeção.
    /// </remarks>
    public enum TransactionSortField
    {
        /// <summary>Data de competência.</summary>
        OccurredOn = 1,

        /// <summary>Vencimento.</summary>
        DueDate = 2,

        /// <summary>Valor.</summary>
        Amount = 3,

        /// <summary>Descrição.</summary>
        Description = 4,

        /// <summary>Momento do cadastro.</summary>
        CreatedAt = 5
    }
}
