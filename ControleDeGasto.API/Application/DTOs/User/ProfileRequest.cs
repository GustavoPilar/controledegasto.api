using System.ComponentModel.DataAnnotations;

namespace ControleDeGasto.API.Application.DTOs
{
    public class ProfileRequest
    {
        [Required(ErrorMessage = "Nome completo é obrigatório.")]
        [MinLength(3, ErrorMessage = "Nome completo deve possuir no mínimo 3 caracteres.")]
        [MaxLength(150, ErrorMessage = "Nome completo deve possuir no máximo 150 caracteres.")]
        // Blocos separados por um único espaço impedem que o campo seja preenchido apenas com espaços,
        // que passariam em Required e MinLength por serem caracteres válidos.
        [RegularExpression(@"^[a-zA-Z]+( [a-zA-Z]+)*$", ErrorMessage = "Apenas letras (a-z e/ou A-Z).")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nome de usuário é obrigatório.")]
        [MinLength(3, ErrorMessage = "Nome de usuário deve possuir no mínimo 3 caracteres.")]
        [MaxLength(50, ErrorMessage = "Nome de usuário deve possuir no máximo 50 caracteres.")]
        // Os caracteres aceitos aqui precisam existir em Identity:AllowedUserNameCharacters, senão o UserValidator rejeita depois do DTO passar.
        [RegularExpression(@"^[a-zA-Z0-9._\-]+( [a-zA-Z0-9._\-]+)*$", ErrorMessage = "Apenas letras (a-z e/ou A-Z), números (0-9), ponto (.), hífen (-), underline (_) e espaço.")]
        public string UserName { get; set; } = string.Empty;
    }
}
