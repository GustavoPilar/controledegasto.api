using ControleDeGasto.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleDeGasto.API.Infra.Persistence.Configuration
{
    public class SavingsGoalEntityTypeConfiguration : IEntityTypeConfiguration<SavingsGoal>
    {
        #region Constants :: NAME_MAX_LENGTH, COLOR_MAX_LENGTH, ICON_MAX_LENGTH, ENUM_MAX_LENGTH, MONEY_PRECISION, MONEY_SCALE

        private const int NAME_MAX_LENGTH = 60;
        private const int COLOR_MAX_LENGTH = 7;
        private const int ICON_MAX_LENGTH = 40;
        private const int ENUM_MAX_LENGTH = 20;
        private const int MONEY_PRECISION = 18;
        private const int MONEY_SCALE = 2;

        #endregion

        #region Methods :: Configure()

        public void Configure(EntityTypeBuilder<SavingsGoal> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(NAME_MAX_LENGTH);

            builder.Property(x => x.TargetAmount)
                .IsRequired()
                .HasPrecision(MONEY_PRECISION, MONEY_SCALE);

            builder.Property(x => x.Color)
                .IsRequired()
                .HasMaxLength(COLOR_MAX_LENGTH);

            builder.Property(x => x.Icon)
                .IsRequired()
                .HasMaxLength(ICON_MAX_LENGTH);

            builder.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(ENUM_MAX_LENGTH)
                .HasConversion<string>();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Índice único parcial: a regra "uma reserva de emergência por usuário" fica
            // garantida pelo banco, e não apenas por validação na aplicação.
            builder.HasIndex(x => x.UserId)
                .IsUnique()
                .HasFilter("\"IsEmergencyReserve\" = true")
                .HasDatabaseName("IX_SavingsGoals_UserId_EmergencyReserve");

            builder.HasIndex(x => new { x.UserId, x.Status });

            builder.HasIndex(x => new { x.UserId, x.Name })
                .IsUnique();
        }

        #endregion
    }
}
