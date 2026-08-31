namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Liquidação ou reabertura de um lançamento previsto.
    /// </summary>
    /// <remarks>
    /// Endpoint próprio em vez de um PUT do lançamento inteiro: marcar uma conta como paga é a
    /// ação mais frequente da tela, e exigir o corpo completo faria um clique arriscar
    /// sobrescrever campos que o usuário não pretendia tocar.
    /// </remarks>
    public class TransactionSettleRequest
    {
        #region Properties :: Settled, SettledOn

        /// <summary>Verdadeiro para liquidar; falso para voltar a previsto.</summary>
        public bool Settled { get; set; } = true;

        /// <summary>
        /// Data do pagamento ou recebimento. Nulo assume hoje. Ignorado ao reabrir.
        /// </summary>
        public DateTime? SettledOn { get; set; }

        #endregion
    }
}
