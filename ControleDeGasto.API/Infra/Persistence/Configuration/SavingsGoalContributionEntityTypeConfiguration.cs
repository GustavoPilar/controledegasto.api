using ControleDeGasto.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleDeGasto.API.Infra.Persistence.Configuration
{
    public class SavingsGoalContributionEntityTypeConfiguration : IEntityTypeConfiguration<SavingsGoalContribution>
    {
        #region Constants :: NOTE_MAX_LENGTH, ENUM_MAX_LENGTH, MONEY_PRECISION, MONEY_SCALE

        private const int NOTE_MAX_LENGTH = 120;
        private const int ENUM_MAX_LENGTH = 20;
        private const int MONEY_PRECISION = 18;
        private const int MONEY_SCALE = 2;

        #endregion

        #region Methods :: Configure()

        public void Configure(EntityTypeBuilder<SavingsGoalContribution> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Amount)
                .IsRequired()
                .HasPrecision(MONEY_PRECISION, MONEY_SCALE);

            builder.Property(x => x.Kind)
                .IsRequired()
                .HasMaxLength(ENUM_MAX_LENGTH)
                .HasConversion<string>();

            builder.Property(x => x.OccurredOn)
                .IsRequired();

            builder.Property(x => x.Note)
                .HasMaxLength(NOTE_MAX_LENGTH);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasOne(x => x.SavingsGoal)
                .WithMany(x => x.Contributions)
                .HasForeignKey(x => x.SavingsGoalId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Sem cascata a partir do usuário: a exclusão já chega em cascata pelo cofrinho, e
            // dois caminhos de cascata para a mesma linha são rejeitados por vários bancos.
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Atende ao cálculo de saldo, que agrupa por cofrinho.
            builder.HasIndex(x => new { x.SavingsGoalId, x.OccurredOn })
                .IsDescending(false, true);

            builder.HasIndex(x => new { x.UserId, x.OccurredOn })
                .IsDescending(false, true);
        }

        #endregion
    }
}
