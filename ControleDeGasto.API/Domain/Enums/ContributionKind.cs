namespace ControleDeGasto.API.Domain.Enums
{
    /// <summary>
    /// Sentido de um movimento em cofrinho.
    /// </summary>
    /// <remarks>
    /// O valor de um aporte é sempre positivo; é este enum que define se ele soma ou subtrai
    /// do saldo. Guardar sinal no valor tornaria toda soma dependente de convenção implícita.
    /// </remarks>
    public enum ContributionKind
    {
        /// <summary>Depósito no cofrinho.</summary>
        Deposit = 1,

        /// <summary>Resgate do cofrinho.</summary>
        Withdrawal = 2
    }
}
