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

        #region Properties :: Friendships, SavingsGoalMembers, TransactionShares

        public DbSet<Friendship> Friendships => this.Set<Friendship>();

        public DbSet<SavingsGoalMember> SavingsGoalMembers => this.Set<SavingsGoalMember>();

        public DbSet<TransactionShare> TransactionShares => this.Set<TransactionShare>();

        #endregion

        #region Properties :: Wallets, WalletTransfers, FixedEntries, Tags, TransactionTags, InstallmentPlans

        public DbSet<Wallet> Wallets => this.Set<Wallet>();

        public DbSet<WalletTransfer> WalletTransfers => this.Set<WalletTransfer>();

        public DbSet<FixedEntry> FixedEntries => this.Set<FixedEntry>();

        public DbSet<Tag> Tags => this.Set<Tag>();

        public DbSet<TransactionTag> TransactionTags => this.Set<TransactionTag>();

        public DbSet<InstallmentPlan> InstallmentPlans => this.Set<InstallmentPlan>();

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
