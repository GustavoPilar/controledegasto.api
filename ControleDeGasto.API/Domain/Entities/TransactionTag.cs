namespace ControleDeGasto.API.Domain.Entities
{
    /// <summary>
    /// Vínculo entre um lançamento e uma etiqueta.
    /// </summary>
    /// <remarks>
    /// Entidade própria, e não relação muitos-para-muitos implícita do EF Core, para que a
    /// tabela tenha chave composta explícita e possa ser consultada direto nos filtros por
    /// etiqueta, sem materializar os lançamentos.
    /// </remarks>
    public class TransactionTag
    {
        #region Properties :: TransactionId, TagId, Transaction, Tag

        public Guid TransactionId { get; set; }

        public Guid TagId { get; set; }

        public Transaction? Transaction { get; set; }

        public Tag? Tag { get; set; }

        #endregion
    }
}
