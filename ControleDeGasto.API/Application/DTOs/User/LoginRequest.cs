using System.ComponentModel.DataAnnotations;

namespace ControleDeGasto.API.Application.DTOs
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Nome de usuário obrigatório.")]
        [MinLength(3, ErrorMessage = "Nome de usuário deve possuir no mínimo 3 caracteres.")]
        [MaxLength(50, ErrorMessage = "Nome de usuário deve possuir no máximo 50 caracteres.")]
        [RegularExpression(@"^[a-zA-Z0-9 _.-]+$", ErrorMessage = "Caracteres inválidos")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha obrigatória.")]
        [MinLength(8, ErrorMessage = "Senha deve possuir no mínimo 8 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lembrar-me obrigatório")]
        public bool RememberMe { get; set; }
    }
}
