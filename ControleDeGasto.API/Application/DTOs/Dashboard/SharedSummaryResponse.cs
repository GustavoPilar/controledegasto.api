namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Resumo das divisões de compra em aberto com amigos.
    /// </summary>
    public class SharedSummaryResponse
    {
        #region Properties :: Receivable, Payable, NetBalance, FriendCount, OpenShareCount

        /// <summary>Total que amigos devem ao usuário.</summary>
        public decimal Receivable { get; set; }

        /// <summary>Total que o usuário deve a amigos.</summary>
        public decimal Payable { get; set; }

        /// <summary>Positivo quando há mais a receber do que a pagar.</summary>
        public decimal NetBalance { get; set; }

        /// <summary>Quantidade de amigos com pendência em qualquer sentido.</summary>
        public int FriendCount { get; set; }

        /// <summary>Quantidade de divisões atribuídas ao usuário e ainda não acertadas.</summary>
        public int OpenShareCount { get; set; }

        #endregion
    }
}
