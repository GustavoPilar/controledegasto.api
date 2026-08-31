using ControleDeGasto.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleDeGasto.API.Infra.Persistence.Configuration
{
    public class FixedEntryEntityTypeConfiguration : IEntityTypeConfiguration<FixedEntry>
    {
        #region Constants :: DESCRIPTION_MAX_LENGTH, ENUM_MAX_LENGTH, MONEY_PRECISION, MONEY_SCALE

        private const int DESCRIPTION_MAX_LENGTH = 120;
        private const int ENUM_MAX_LENGTH = 20;
        private const int MONEY_PRECISION = 18;
        private const int MONEY_SCALE = 2;

        #endregion

        #region Methods :: Configure()

        public void Configure(EntityTypeBuilder<FixedEntry> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Kind)
                .IsRequired()
                .HasMaxLength(ENUM_MAX_LENGTH)
                .HasConversion<string>();

            builder.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(DESCRIPTION_MAX_LENGTH);

            builder.Property(x => x.Amount)
                .IsRequired()
                .HasPrecision(MONEY_PRECISION, MONEY_SCALE);

            builder.Property(x => x.DayOfMonth)
                .IsRequired();

            builder.Property(x => x.StartsOn)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Wallet)
                .WithMany()
                .HasForeignKey(x => x.WalletId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // A previsão do mês lê todas as definições vivas do usuário de uma vez.
            builder.HasIndex(x => new { x.UserId, x.Active, x.Kind });

            builder.ToTable(table => table.HasCheckConstraint(
                "CK_FixedEntries_DayOfMonth",
                "\"DayOfMonth\" BETWEEN 1 AND 31"));
        }

        #endregion
    }
}
