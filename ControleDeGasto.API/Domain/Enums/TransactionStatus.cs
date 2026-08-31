namespace ControleDeGasto.API.Domain.Enums
{
    /// <summary>
    /// Situação de liquidação de um lançamento.
    /// </summary>
    /// <remarks>
    /// Separa o que foi combinado do que já saiu ou entrou de fato. O atraso não é um valor
    /// deste enum porque não é um estado gravado: é <see cref="Pending"/> com vencimento no
    /// passado, e derivar em vez de gravar evita uma rotina diária só para virar a situação
    /// de linhas que ninguém consultou.
    /// </remarks>
    public enum TransactionStatus
    {
        /// <summary>Previsto: ainda não foi pago nem recebido.</summary>
        Pending = 1,

        /// <summary>Liquidado: já foi pago ou recebido.</summary>
        Settled = 2
    }
}
