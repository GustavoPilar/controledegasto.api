using ControleDeGasto.API.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ControleDeGasto.API.Application.DTOs
{
    public class UserRequest
    {
        [Required(ErrorMessage = "Nome de usuário obrigatório.")]
        [MinLength(3, ErrorMessage = "Nome de usuário deve possuir no mínimo 3 caracteres.")]
        [MaxLength(50, ErrorMessage = "Nome de usuário deve possuir no máximo 50 caracteres.")]
        [RegularExpression(@"^[a-zA-Z0-9 _.-]+$")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha obrigatória.")]
        [MinLength(8, ErrorMessage = "Senha deve possuir no mínimo 8 caracteres.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirmação de senha obrigatória.")]
        [Compare("Password", ErrorMessage = "As senhas não se coincidem.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
