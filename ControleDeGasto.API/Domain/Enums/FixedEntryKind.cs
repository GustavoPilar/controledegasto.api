namespace ControleDeGasto.API.Domain.Enums
{
    /// <summary>
    /// Natureza de um valor fixo mensal.
    /// </summary>
    /// <remarks>
    /// O crédito de benefício é separado da entrada comum porque não é dinheiro livre: cair
    /// R$ 800 de vale-refeição não aumenta em R$ 800 o que se pode gastar com aluguel. Somar
    /// os dois no mesmo indicador daria um saldo previsto que o usuário não tem.
    /// </remarks>
    public enum FixedEntryKind
    {
        /// <summary>Entrada fixa em dinheiro (salário, aluguel recebido, pró-labore).</summary>
        Income = 1,

        /// <summary>Saída fixa (aluguel, plano de celular, mensalidade).</summary>
        Expense = 2,

        /// <summary>Crédito mensal de benefício em uma carteira de vale (VR, VA, VT, VC).</summary>
        BenefitCredit = 3
    }
}
