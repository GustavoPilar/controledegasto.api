namespace ControleDeGasto.API.Application.Utilities
{
    /// <summary>
    /// Operações com dinheiro que precisam fechar no centavo.
    /// </summary>
    public static class MoneyHelper
    {
        #region Methods :: SplitEvenly()

        /// <summary>
        /// Divide um valor em partes iguais garantindo que a soma feche com o total.
        /// </summary>
        /// <remarks>
        /// R$ 100,00 em três não dá três vezes R$ 33,33: sobra um centavo. A sobra vai para a
        /// primeira parte, como faz a maioria das faturas, e nunca é descartada — descartar
        /// produziria um parcelamento que soma menos do que a compra.
        /// </remarks>
        /// <param name="total">Valor a dividir. Deve ser positivo.</param>
        /// <param name="parts">Quantidade de partes. Deve ser maior que zero.</param>
        /// <returns>Partes cuja soma é exatamente o total.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Total não positivo ou quantidade inválida.</exception>
        public static IReadOnlyList<decimal> SplitEvenly(decimal total, int parts)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(parts, 0);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(total, 0);

            decimal rounded = Math.Round(total, 2, MidpointRounding.AwayFromZero);

            // Trabalha em centavos inteiros: dividir decimais e arredondar cada parte deixaria a
            // soma flutuar conforme a quantidade de parcelas.
            long totalCents = (long)Math.Round(rounded * 100, 0, MidpointRounding.AwayFromZero);

            long baseCents = totalCents / parts;
            long remainder = totalCents % parts;

            List<decimal> amounts = new List<decimal>(parts);

            for (int index = 0; index < parts; index++)
            {
                long cents = baseCents + (index == 0 ? remainder : 0);

                amounts.Add(cents / 100m);
            }

            return amounts;
        }

        #endregion
    }
}
