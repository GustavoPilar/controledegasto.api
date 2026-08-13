using ControleDeGasto.API.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ControleDeGasto.API.Infra.Persistence
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options)
    {
        #region Properties :: UserPreferences, Categories, Transactions, SavingsGoals, SavingsGoalContributions, Notifications

        public DbSet<UserPreference> UserPreferences => this.Set<UserPreference>();

        public DbSet<Category> Categories => this.Set<Category>();

        public DbSet<Transaction> Transactions => this.Set<Transaction>();

        public DbSet<SavingsGoal> SavingsGoals => this.Set<SavingsGoal>();

        public DbSet<SavingsGoalContribution> SavingsGoalContributions => this.Set<SavingsGoalContribution>();

        public DbSet<Notification> Notifications => this.Set<Notification>();

        #endregion

        #region Methods :: OnModelCreating()

        /// <summary>
        /// Constrói o modelo do EF Core aplicando o mapeamento do Identity e, em seguida,
        /// as configurações específicas da aplicação.
        /// </summary>
        /// <param name="builder">Construtor do modelo fornecido pelo EF Core.</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

        #endregion
    }
}
