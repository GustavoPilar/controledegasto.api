using Microsoft.AspNetCore.Identity;

namespace ControleDeGasto.API.Application.Configuration
{
    public static class RoleSeender
    {
        public const string STANDARD_ROLE = "Standard";

        public static async Task SeedAsync(RoleManager<IdentityRole<Guid>> roleUser)
        {
            string[] roles = [STANDARD_ROLE];

            foreach (string role in roles)
            {
                bool exists = await roleUser.RoleExistsAsync(role);

                if (!exists)
                    await roleUser.CreateAsync(new IdentityRole<Guid>(role));
            }
        }
    }
}
