namespace ControleDeGasto.API.Domain.Enums
{
    /// <summary>
    /// Natureza de um lançamento financeiro.
    /// </summary>
    public enum TransactionType
    {
        /// <summary>Entrada de dinheiro (salário, rendimento, venda).</summary>
        Income = 1,

        /// <summary>Saída de dinheiro (despesa, compra, conta).</summary>
        Expense = 2
    }
}
