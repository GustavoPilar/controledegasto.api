using ControleDeGasto.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleDeGasto.API.Infra.Persistence.Configuration
{
    public class WalletTransferEntityTypeConfiguration : IEntityTypeConfiguration<WalletTransfer>
    {
        #region Constants :: NOTE_MAX_LENGTH, MONEY_PRECISION, MONEY_SCALE

        private const int NOTE_MAX_LENGTH = 120;
        private const int MONEY_PRECISION = 18;
        private const int MONEY_SCALE = 2;

        #endregion

        #region Methods :: Configure()

        public void Configure(EntityTypeBuilder<WalletTransfer> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Amount)
                .IsRequired()
                .HasPrecision(MONEY_PRECISION, MONEY_SCALE);

            builder.Property(x => x.OccurredOn)
                .IsRequired();

            builder.Property(x => x.Note)
                .HasMaxLength(NOTE_MAX_LENGTH);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Restrict nas duas carteiras: apagar uma carteira com transferências deixaria a
            // outra ponta com um saldo que ninguém consegue explicar. A exclusão de carteira
            // é lógica justamente por isso.
            builder.HasOne(x => x.FromWallet)
                .WithMany()
                .HasForeignKey(x => x.FromWalletId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ToWallet)
                .WithMany()
                .HasForeignKey(x => x.ToWalletId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.UserId, x.OccurredOn })
                .IsDescending(false, true);

            builder.HasIndex(x => x.FromWalletId);
            builder.HasIndex(x => x.ToWalletId);

            builder.ToTable(table => table.HasCheckConstraint(
                "CK_WalletTransfers_DifferentWallets",
                "\"FromWalletId\" <> \"ToWalletId\""));
        }

        #endregion
    }
}
