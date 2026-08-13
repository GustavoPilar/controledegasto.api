namespace ControleDeGasto.API.Application.Utilities
{
    /// <summary>
    /// Normalização de datas para UTC.
    /// </summary>
    /// <remarks>
    /// O Postgres com timestamptz recusa DateTime cujo Kind não seja Utc, e o JSON do cliente
    /// chega com Kind indefinido. Centralizar a conversão aqui evita que cada serviço invente
    /// a sua própria — e que o fuso do servidor influencie o mês de competência de um lançamento.
    /// </remarks>
    public static class DateTimeHelper
    {
        #region Methods :: ToUtc(), ToUtcDate(), ToUtcEndOfDay(), StartOfMonth(), EndOfMonth()

        /// <summary>
        /// Converte um instante para UTC, tratando Kind indefinido como já sendo UTC.
        /// </summary>
        /// <param name="value">Instante a converter.</param>
        /// <returns>O mesmo instante com Kind Utc.</returns>
        public static DateTime ToUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };
        }

        /// <summary>
        /// Reduz um instante à meia-noite UTC do seu dia.
        /// </summary>
        /// <param name="value">Instante a converter.</param>
        /// <returns>Início do dia em UTC.</returns>
        public static DateTime ToUtcDate(DateTime value)
        {
            return DateTime.SpecifyKind(ToUtc(value).Date, DateTimeKind.Utc);
        }

        /// <summary>
        /// Leva um instante ao último momento do seu dia, em UTC.
        /// </summary>
        /// <remarks>
        /// Usado nos filtros de período: sem isso, um filtro "até dia 31" descartaria tudo o
        /// que foi lançado no próprio dia 31.
        /// </remarks>
        /// <param name="value">Instante a converter.</param>
        /// <returns>Fim do dia em UTC.</returns>
        public static DateTime ToUtcEndOfDay(DateTime value)
        {
            return ToUtcDate(value).AddDays(1).AddTicks(-1);
        }

        /// <summary>
        /// Primeiro instante do mês de referência, em UTC.
        /// </summary>
        /// <param name="reference">Data de referência.</param>
        /// <returns>Início do mês em UTC.</returns>
        public static DateTime StartOfMonth(DateTime reference)
        {
            DateTime utc = ToUtc(reference);

            return new DateTime(utc.Year, utc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        /// <summary>
        /// Último instante do mês de referência, em UTC.
        /// </summary>
        /// <param name="reference">Data de referência.</param>
        /// <returns>Fim do mês em UTC.</returns>
        public static DateTime EndOfMonth(DateTime reference)
        {
            return StartOfMonth(reference).AddMonths(1).AddTicks(-1);
        }

        #endregion
    }
}
