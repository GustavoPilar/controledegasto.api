namespace ControleDeGasto.API.Domain.Exceptions
{
    /// <summary>
    /// Base das violações de regra de negócio. Representa erro do chamador (HTTP 400),
    /// não falha de infraestrutura.
    /// </summary>
    /// <remarks>
    /// Existe para os controllers distinguirem "requisição inválida" de "erro no servidor"
    /// sem capturar <see cref="Exception"/> genérica.
    /// </remarks>
    public class DomainException : Exception
    {
        #region Constructors

        /// <summary>
        /// Cria a exceção com a mensagem que pode ser exibida ao usuário.
        /// </summary>
        /// <param name="message">Descrição da regra violada.</param>
        public DomainException(string message) : base(message)
        {
        }

        #endregion
    }
}
