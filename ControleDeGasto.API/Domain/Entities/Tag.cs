namespace ControleDeGasto.API.Domain.Entities
{
    /// <summary>
    /// Etiqueta livre para agrupar lançamentos por assunto, independente da categoria.
    /// </summary>
    /// <remarks>
    /// Convive com a categoria em vez de substituí-la: a categoria é única e define se o
    /// lançamento é entrada ou saída, enquanto a etiqueta é múltipla e transversal — uma
    /// viagem tem passagem, hospedagem e comida, cada uma na sua categoria.
    /// </remarks>
    public class Tag
    {
        #region Properties :: Id, UserId, Name, Color, CreatedAt, User

        public Guid Id { get; set; }

        /// <summary>Dono da etiqueta.</summary>
        public Guid UserId { get; set; }

        public string Name { get; set; } = string.Empty;

        /// <summary>Cor em hexadecimal (#RRGGBB).</summary>
        public string Color { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public User? User { get; set; }

        #endregion
    }
}
