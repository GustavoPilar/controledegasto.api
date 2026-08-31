using ControleDeGasto.API.Domain.Entities;

namespace ControleDeGasto.API.Application.Utilities
{
    /// <summary>
    /// Resolve as datas em que um valor fixo mensal acontece.
    /// </summary>
    /// <remarks>
    /// Existe porque "todo dia 31" não é uma data em fevereiro. Sem um único lugar que decida o
    /// que fazer nesses meses, a previsão e o saldo de benefício chegariam a resultados
    /// diferentes para a mesma definição.
    /// </remarks>
    public static class FixedEntryHelper
    {
        #region Methods :: ResolveDateInMonth()

        /// <summary>
        /// Data em que a definição acontece em um mês, em UTC.
        /// </summary>
        /// <remarks>
        /// Um dia maior que o mês é reduzido ao último dia dele: quem paga todo dia 31 paga no
        /// dia 28 em fevereiro, não no dia 3 de março.
        /// </remarks>
        /// <param name="dayOfMonth">Dia desejado (1 a 31).</param>
        /// <param name="reference">Qualquer data do mês de referência.</param>
        /// <returns>Data do evento no mês, à meia-noite UTC.</returns>
        public static DateTime ResolveDateInMonth(int dayOfMonth, DateTime reference)
        {
            DateTime utc = DateTimeHelper.ToUtc(reference);

            int daysInMonth = DateTime.DaysInMonth(utc.Year, utc.Month);
            int day = Math.Clamp(dayOfMonth, 1, daysInMonth);

            return new DateTime(utc.Year, utc.Month, day, 0, 0, 0, DateTimeKind.Utc);
        }

        #endregion

        #region Methods :: AppliesToMonth(), CountOccurrencesUntil()

        /// <summary>
        /// Indica se a definição vale em um mês.
        /// </summary>
        /// <param name="fixedEntry">Definição a avaliar.</param>
        /// <param name="monthStart">Primeiro instante do mês, em UTC.</param>
        /// <param name="monthEnd">Último instante do mês, em UTC.</param>
        /// <returns>True quando a vigência cobre o mês.</returns>
        public static bool AppliesToMonth(FixedEntry fixedEntry, DateTime monthStart, DateTime monthEnd)
        {
            ArgumentNullException.ThrowIfNull(fixedEntry);

            if (fixedEntry.StartsOn > monthEnd)
                return false;

            return !fixedEntry.EndsOn.HasValue || fixedEntry.EndsOn.Value >= monthStart;
        }

        /// <summary>
        /// Conta quantas vezes a definição já aconteceu até um momento.
        /// </summary>
        /// <remarks>
        /// É o que permite apurar o saldo de uma carteira de benefício sem gravar um lançamento
        /// por mês: o total creditado é a contagem de ocorrências multiplicada pelo valor.
        /// </remarks>
        /// <param name="fixedEntry">Definição a avaliar.</param>
        /// <param name="until">Momento limite, inclusive, em UTC.</param>
        /// <returns>Quantidade de ocorrências no intervalo de vigência até o limite.</returns>
        public static int CountOccurrencesUntil(FixedEntry fixedEntry, DateTime until)
        {
            ArgumentNullException.ThrowIfNull(fixedEntry);

            DateTime limit = DateTimeHelper.ToUtc(until);

            // O fim da vigência antecipa o limite: uma conta encerrada em março não continua
            // creditando em agosto.
            if (fixedEntry.EndsOn.HasValue && fixedEntry.EndsOn.Value < limit)
                limit = fixedEntry.EndsOn.Value;

            DateTime start = DateTimeHelper.StartOfMonth(fixedEntry.StartsOn);

            if (DateTimeHelper.StartOfMonth(limit) < start)
                return 0;

            int occurrences = 0;

            // Percorre mês a mês em vez de calcular pela diferença de meses: o dia efetivo muda
            // conforme o tamanho do mês, e só a data resolvida diz se a ocorrência já passou.
            for (DateTime month = start; DateTimeHelper.StartOfMonth(month) <= DateTimeHelper.StartOfMonth(limit); month = month.AddMonths(1))
            {
                DateTime occurrence = ResolveDateInMonth(fixedEntry.DayOfMonth, month);

                if (occurrence < fixedEntry.StartsOn)
                    continue;

                if (occurrence <= limit)
                    occurrences++;
            }

            return occurrences;
        }

        #endregion
    }
}
