namespace ControleDeGasto.API.Domain.Enums
{
    /// <summary>
    /// Temas de interface disponíveis para o usuário.
    /// </summary>
    /// <remarks>
    /// Modelado como enum, e não como tabela de domínio, porque o conjunto é fechado e só muda
    /// com deploy: uma tabela exigiria seed por ambiente, join em toda leitura e um endpoint
    /// público só para a tela de cadastro obter os identificadores. Os rótulos exibidos
    /// ("Claro", "Escuro", "Sistema") são responsabilidade da camada de apresentação.
    /// </remarks>
    public enum AppearanceType
    {
        /// <summary>Tema claro. Padrão para contas novas.</summary>
        Light = 1,

        /// <summary>Tema escuro.</summary>
        Dark = 2,

        /// <summary>Acompanha a preferência declarada pelo sistema operacional do usuário.</summary>
        System = 3
    }
}
