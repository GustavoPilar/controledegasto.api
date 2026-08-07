using ControleDeGasto.API.Domain.Entities;

namespace ControleDeGasto.API.Application.DTOs
{
    public class UserResponse(User user)
    {
        public Guid Id { get; set; } = user.Id;

        public string FullName { get; set; } = user.FullName;

        public string Email { get; set; } = user.Email!;

        public string UserName { get; set; } = user.UserName!;

        public bool EmailConfirmed { get; set; } = user.EmailConfirmed;

        public DateTime CreatedAt { get; set; } = user.CreatedAt;

        public DateTime? UpdatedAt { get; set; } = user.UpdatedAt;

        public DateTime? DeactivatedAt { get; set; } = user.DeactivatedAt;

        public bool Active { get; set; } = user.Active;

    }
}
