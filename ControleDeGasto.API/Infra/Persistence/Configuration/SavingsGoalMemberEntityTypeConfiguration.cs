using ControleDeGasto.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleDeGasto.API.Infra.Persistence.Configuration
{
    public class SavingsGoalMemberEntityTypeConfiguration : IEntityTypeConfiguration<SavingsGoalMember>
    {
        #region Constants :: ENUM_MAX_LENGTH

        private const int ENUM_MAX_LENGTH = 20;

        #endregion

        #region Methods :: Configure()

        public void Configure(EntityTypeBuilder<SavingsGoalMember> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Role)
                .IsRequired()
                .HasMaxLength(ENUM_MAX_LENGTH)
                .HasConversion<string>();

            builder.Property(x => x.JoinedAt)
                .IsRequired();

            builder.HasOne(x => x.SavingsGoal)
                .WithMany(x => x.Members)
                .HasForeignKey(x => x.SavingsGoalId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Restrict no usuário: a cascata já chega pelo cofrinho.
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.SavingsGoalId, x.UserId })
                .IsUnique();

            // Atende à listagem de cofrinhos, que hoje parte do participante.
            builder.HasIndex(x => x.UserId);

            // Um cofrinho tem exatamente um dono.
            builder.HasIndex(x => x.SavingsGoalId)
                .IsUnique()
                .HasFilter("\"Role\" = 'Owner'")
                .HasDatabaseName("IX_SavingsGoalMembers_SavingsGoalId_Owner");
        }

        #endregion
    }
}
