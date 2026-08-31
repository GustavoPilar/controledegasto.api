namespace ControleDeGasto.API.Domain.Enums
{
    /// <summary>
    /// De onde veio uma linha da previsão do mês.
    /// </summary>
    /// <remarks>
    /// Acompanha cada item para que o usuário saiba por que aquele valor está na previsão, e
    /// para que a tela possa levá-lo ao lugar certo: uma conta prevista se liquida no extrato,
    /// um valor fixo se altera no cadastro de fixos.
    /// </remarks>
    public enum ForecastSource
    {
        /// <summary>Definição de valor fixo mensal.</summary>
        FixedEntry = 1,

        /// <summary>Lançamento previsto (conta a pagar ou a receber).</summary>
        PendingTransaction = 2,

        /// <summary>Parcela de compra parcelada ainda em aberto.</summary>
        Installment = 3
    }
}
