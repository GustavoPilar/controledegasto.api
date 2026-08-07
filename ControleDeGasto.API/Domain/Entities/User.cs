using Microsoft.AspNetCore.Identity;

namespace ControleDeGasto.API.Domain.Entities
{
    public class User : IdentityUser<Guid>
    {
        public string FullName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeactivatedAt { get; set; }

        public bool Active { get; set; }
    }
}
