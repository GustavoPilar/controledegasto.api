namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Envelope de resposta paginada.
    /// </summary>
    /// <typeparam name="T">Tipo dos itens.</typeparam>
    public class PagedResponse<T>
    {
        #region Constructors

        /// <summary>
        /// Cria a resposta paginada calculando o total de páginas.
        /// </summary>
        /// <param name="items">Itens da página.</param>
        /// <param name="totalCount">Total de registros que atendem ao filtro.</param>
        /// <param name="page">Página devolvida, iniciando em 1.</param>
        /// <param name="pageSize">Itens por página.</param>
        public PagedResponse(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
        {
            this.Items = items;
            this.TotalCount = totalCount;
            this.Page = page;
            this.PageSize = pageSize;
            this.TotalPages = pageSize <= 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        }

        #endregion

        #region Properties :: Items, TotalCount, Page, PageSize, TotalPages

        public IReadOnlyList<T> Items { get; set; }

        public int TotalCount { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalPages { get; set; }

        #endregion
    }
}
