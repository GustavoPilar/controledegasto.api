namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Dados do cadastro de uma nova conta.
    /// </summary>
    public class RegisterRequest
    {
        #region Properties :: UserRequest, UserPreference

        /// <summary>Dados do usuário. Obrigatório.</summary>
        public UserRequest? UserRequest { get; set; }

        /// <summary>
        /// Preferências iniciais. Opcional: quando ausente, o servidor cria a preferência
        /// com o tema claro.
        /// </summary>
        public UserPreferenceRequest? UserPreference { get; set; }

        #endregion
    }
}
