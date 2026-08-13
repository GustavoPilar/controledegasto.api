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
    /// <param name="From">Início do período, em UTC. Nulo remove o limite inferior.</param>
    /// <param name="To">Fim do período, em UTC. Nulo remove o limite superior.</param>
    /// <param name="CategoryId">Filtra por categoria. Nulo traz todas.</param>
    /// <param name="Type">Filtra por natureza. Nulo traz entradas e saídas.</param>
    /// <param name="Search">Trecho da descrição a procurar. Nulo ignora o filtro.</param>
    /// <param name="Page">Página solicitada, iniciando em 1.</param>
    /// <param name="PageSize">Quantidade de itens por página.</param>
    public sealed record TransactionQuery(
        Guid UserId,
        DateTime? From,
        DateTime? To,
        Guid? CategoryId,
        TransactionType? Type,
        string? Search,
        int Page,
        int PageSize);
}
