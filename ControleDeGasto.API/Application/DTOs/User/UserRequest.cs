using ControleDeGasto.API.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ControleDeGasto.API.Application.DTOs
{
    public class UserRequest
    {
        [Required(ErrorMessage = "Nome completo é obrigatório.")]
        [MinLength(3, ErrorMessage = "Nome completo deve possuir no mínimo 3 caracteres.")]
        [MaxLength(150, ErrorMessage = "Nome completo deve possuir no máximo 150 caracteres.")]
        [RegularExpression(@"^[a-zA-Z ]+$", ErrorMessage = "Apenas letras (a-z e/ou A-Z).")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nome de usuário é obrigatório.")]
        [MinLength(3, ErrorMessage = "Nome de usuário deve possuir no mínimo 3 caracteres.")]
        [MaxLength(50, ErrorMessage = "Nome de usuário deve possuir no máximo 50 caracteres.")]
        [RegularExpression(@"^[a-zA-Z0-9._\- ]+$", ErrorMessage = "Apenas letras (a-z e/ou A-Z), números (0-9), ponto (.), hífen (-), underline (_) e espaço.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Insira um e-mail válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirmar e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Insira um e-mail válido.")]
        [Compare("Email", ErrorMessage = "E-mail e Confimar E-mail não coincidem.")]
        public string ConfirmEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha obrigatória.")]
        [MinLength(8, ErrorMessage = "Senha deve possuir no mínimo 8 caracteres.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirmar senha é obrigatória.")]
        [MinLength(8, ErrorMessage = "Confirmar senha deve possuir no mínimo 8 caracteres")]
        [Compare("Password", ErrorMessage = "Senha e Confirmar senha não coincidem.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
