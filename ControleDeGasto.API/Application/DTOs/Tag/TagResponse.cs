using ControleDeGasto.API.Domain.Entities;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Etiqueta devolvida ao cliente, com a quantidade de lançamentos em que está aplicada.
    /// </summary>
    public class TagResponse
    {
        #region Constructors

        /// <summary>
        /// Cria a resposta a partir da etiqueta e da contagem de uso.
        /// </summary>
        /// <param name="tag">Etiqueta de origem.</param>
        /// <param name="transactionCount">Quantidade de lançamentos marcados com ela.</param>
        public TagResponse(Tag tag, int transactionCount)
        {
            ArgumentNullException.ThrowIfNull(tag);

            this.Id = tag.Id;
            this.Name = tag.Name;
            this.Color = tag.Color;
            this.TransactionCount = transactionCount;
            this.CreatedAt = tag.CreatedAt;
        }

        #endregion

        #region Properties :: Id, Name, Color, TransactionCount, CreatedAt

        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Color { get; set; }

        /// <summary>
        /// Quantidade de lançamentos marcados. Acompanha a resposta para a tela avisar o que a
        /// exclusão vai desmarcar antes de o usuário confirmar.
        /// </summary>
        public int TransactionCount { get; set; }

        public DateTime CreatedAt { get; set; }

        #endregion
    }
}
