using ControleDeGasto.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleDeGasto.API.Infra.Persistence.Configuration
{
    public class WalletEntityTypeConfiguration : IEntityTypeConfiguration<Wallet>
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

        public void Configure(EntityTypeBuilder<Wallet> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(NAME_MAX_LENGTH);

            builder.Property(x => x.Kind)
                .IsRequired()
                .HasMaxLength(ENUM_MAX_LENGTH)
                .HasConversion<string>();

            builder.Property(x => x.Color)
                .IsRequired()
                .HasMaxLength(COLOR_MAX_LENGTH);

            builder.Property(x => x.Icon)
                .IsRequired()
                .HasMaxLength(ICON_MAX_LENGTH);

            builder.Property(x => x.InitialBalance)
                .IsRequired()
                .HasPrecision(MONEY_PRECISION, MONEY_SCALE);

            builder.Property(x => x.CreditLimit)
                .HasPrecision(MONEY_PRECISION, MONEY_SCALE);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Índice único parcial: "uma carteira padrão por usuário" passa a ser garantia do
            // banco, e não só validação da aplicação.
            builder.HasIndex(x => x.UserId)
                .IsUnique()
                .HasFilter("\"IsDefault\" = true")
                .HasDatabaseName("IX_Wallets_UserId_Default");

            // Nome único entre as carteiras vivas: uma carteira excluída não deve impedir o
            // usuário de recriar o mesmo nome.
            builder.HasIndex(x => new { x.UserId, x.Name })
                .IsUnique()
                .HasFilter("\"Active\" = true");

            builder.HasIndex(x => new { x.UserId, x.Active });

            builder.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_Wallets_StatementClosingDay",
                    "\"StatementClosingDay\" IS NULL OR (\"StatementClosingDay\" BETWEEN 1 AND 31)");

                table.HasCheckConstraint(
                    "CK_Wallets_PaymentDueDay",
                    "\"PaymentDueDay\" IS NULL OR (\"PaymentDueDay\" BETWEEN 1 AND 31)");
            });
        }

        #endregion
    }
}
