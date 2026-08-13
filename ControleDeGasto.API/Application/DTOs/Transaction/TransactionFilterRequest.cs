using ControleDeGasto.API.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Filtros de listagem de lançamentos, recebidos na query string.
    /// </summary>
    public class TransactionFilterRequest
    {
        #region Constants :: DEFAULT_PAGE, DEFAULT_PAGE_SIZE, MAX_PAGE_SIZE

        public const int DEFAULT_PAGE = 1;
        public const int DEFAULT_PAGE_SIZE = 20;

        /// <summary>
        /// Teto de itens por página. Impede que um cliente peça a base inteira em uma
        /// requisição e derrube o tempo de resposta.
        /// </summary>
        public const int MAX_PAGE_SIZE = 100;

        #endregion

        #region Properties :: From, To, CategoryId, Type, Search, Page, PageSize

        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        public Guid? CategoryId { get; set; }

        public TransactionType? Type { get; set; }

        [StringLength(120, ErrorMessage = "A busca deve ter no máximo 120 caracteres.")]
        public string? Search { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "A página deve ser maior que zero.")]
        public int Page { get; set; } = DEFAULT_PAGE;

        [Range(1, MAX_PAGE_SIZE, ErrorMessage = "A quantidade por página deve estar entre 1 e 100.")]
        public int PageSize { get; set; } = DEFAULT_PAGE_SIZE;

        #endregion
    }
}
