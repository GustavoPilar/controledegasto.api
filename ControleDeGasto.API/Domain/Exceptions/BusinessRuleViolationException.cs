namespace ControleDeGasto.API.Domain.Exceptions
{
    /// <summary>
    /// Regra de negócio violada: valor inválido, resgate maior que o saldo, categoria
    /// incompatível com o lançamento e situações equivalentes.
    /// </summary>
    public class BusinessRuleViolationException : DomainException
    {
        #region Constructors

        /// <summary>
        /// Cria a exceção com a mensagem que pode ser exibida ao usuário.
        /// </summary>
        /// <param name="message">Descrição da regra violada.</param>
        public BusinessRuleViolationException(string message) : base(message)
        {
        }

        #endregion
    }
}
