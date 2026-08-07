namespace ControleDeGasto.API.Application.Configuration
{
    public class AppSettings
    {
        /// <summary>
        /// RUL base do frontend, usada para monstar links de callback (confirmação de e-mail, reset de senha).
        /// </summary>
        public string FrontendBaseUrl { get; set; } = string.Empty;
    }
}
