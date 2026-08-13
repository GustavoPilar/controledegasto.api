namespace ControleDeGasto.API.Application.Configuration
{
    /// <summary>
    /// Ajustes da rotina que avalia regras e envia os avisos por e-mail.
    /// </summary>
    public sealed class NotificationSettings
    {
        #region Properties :: Enabled, IntervalHours, StartupDelaySeconds, EmailBatchSize

        /// <summary>
        /// Liga a rotina. Desligada, a aplicação continua criando notificações pelas ações do
        /// usuário, mas não avalia regras periódicas nem envia e-mail.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>Intervalo entre execuções, em horas.</summary>
        public int IntervalHours { get; set; } = 12;

        /// <summary>
        /// Espera antes da primeira execução, em segundos. Evita competir com o restante da
        /// inicialização da aplicação.
        /// </summary>
        public int StartupDelaySeconds { get; set; } = 30;

        /// <summary>
        /// Máximo de e-mails por execução. Limita o consumo da cota do provedor e evita
        /// disparar centenas de mensagens de uma vez.
        /// </summary>
        public int EmailBatchSize { get; set; } = 50;

        #endregion
    }
}
