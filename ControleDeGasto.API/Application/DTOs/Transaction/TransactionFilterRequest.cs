using ControleDeGasto.API.Domain.Enums;
using ControleDeGasto.API.Domain.Queries;
using System.ComponentModel.DataAnnotations;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Filtros de listagem de lançamentos, recebidos na query string.
    /// </summary>
    public class TransactionFilterRequest
    {
        #region Constants :: DEFAULT_PAGE, DEFAULT_PAGE_SIZE, MAX_PAGE_SIZE, MAX_TAG_FILTERS

        public const int DEFAULT_PAGE = 1;
        public const int DEFAULT_PAGE_SIZE = 20;

        /// <summary>
        /// Teto de itens por página. Impede que um cliente peça a base inteira em uma
        /// requisição e derrube o tempo de resposta.
        /// </summary>
        public const int MAX_PAGE_SIZE = 100;

        /// <summary>Teto de etiquetas no filtro, para limitar o tamanho da cláusula gerada.</summary>
        public const int MAX_TAG_FILTERS = 10;

        #endregion

        #region Properties :: From, To, CategoryId, Type, Search

        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        public Guid? CategoryId { get; set; }

        public TransactionType? Type { get; set; }

        [StringLength(120, ErrorMessage = "A busca deve ter no máximo 120 caracteres.")]
        public string? Search { get; set; }

        #endregion

        #region Properties :: WalletId, TagIds, Status, PaymentMethod, MinAmount, MaxAmount

        public Guid? WalletId { get; set; }

        /// <summary>Etiquetas exigidas. O lançamento entra se tiver ao menos uma delas.</summary>
        [MaxLength(MAX_TAG_FILTERS, ErrorMessage = "São permitidas no máximo 10 etiquetas no filtro.")]
        public IReadOnlyList<Guid>? TagIds { get; set; }

        public TransactionStatus? Status { get; set; }

        public PaymentMethod? PaymentMethod { get; set; }

        [Range(0, 999_999_999.99, ErrorMessage = "O valor mínimo informado é inválido.")]
        public decimal? MinAmount { get; set; }

        [Range(0, 999_999_999.99, ErrorMessage = "O valor máximo informado é inválido.")]
        public decimal? MaxAmount { get; set; }

        #endregion

        #region Properties :: DueFrom, DueTo, OnlyOverdue, OnlyShared, OnlyInstallments, InstallmentPlanId

        public DateTime? DueFrom { get; set; }

        public DateTime? DueTo { get; set; }

        /// <summary>Traz apenas as contas previstas com vencimento já passado.</summary>
        public bool OnlyOverdue { get; set; }

        /// <summary>Traz apenas os lançamentos divididos com amigos.</summary>
        public bool OnlyShared { get; set; }

        /// <summary>Traz apenas as parcelas de compras parceladas.</summary>
        public bool OnlyInstallments { get; set; }

        public Guid? InstallmentPlanId { get; set; }

        #endregion

        #region Properties :: SortBy, SortDescending, Page, PageSize

        [EnumDataType(typeof(TransactionSortField), ErrorMessage = "Campo de ordenação inválido.")]
        public TransactionSortField SortBy { get; set; } = TransactionSortField.OccurredOn;

        public bool SortDescending { get; set; } = true;

        [Range(1, int.MaxValue, ErrorMessage = "A página deve ser maior que zero.")]
        public int Page { get; set; } = DEFAULT_PAGE;

        [Range(1, MAX_PAGE_SIZE, ErrorMessage = "A quantidade por página deve estar entre 1 e 100.")]
        public int PageSize { get; set; } = DEFAULT_PAGE_SIZE;

        #endregion
    }
}
