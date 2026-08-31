using ControleDeGasto.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleDeGasto.API.Infra.Persistence.Configuration
{
    public class TransactionShareEntityTypeConfiguration : IEntityTypeConfiguration<TransactionShare>
    {
        #region Constants :: MONEY_PRECISION, MONEY_SCALE

        private const int MONEY_PRECISION = 18;
        private const int MONEY_SCALE = 2;

        #endregion

        #region Methods :: Configure()

        public void Configure(EntityTypeBuilder<TransactionShare> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Amount)
                .IsRequired()
                .HasPrecision(MONEY_PRECISION, MONEY_SCALE);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasOne(x => x.Transaction)
                .WithMany(x => x.Shares)
                .HasForeignKey(x => x.TransactionId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Restrict no amigo: a cascata já chega pelo lançamento, e dois caminhos de exclusão
            // para a mesma linha são recusados pelo banco.
            builder.HasOne(x => x.FriendUser)
                .WithMany()
                .HasForeignKey(x => x.FriendUserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // O mesmo amigo entra uma vez por lançamento: duas partes para a mesma pessoa são
            // uma parte só, com o valor somado.
            builder.HasIndex(x => new { x.TransactionId, x.FriendUserId })
                .IsUnique();

            // Atende à tela "dividido comigo", que parte do amigo para os lançamentos.
            builder.HasIndex(x => new { x.FriendUserId, x.SettledAt });
        }

        #endregion
    }
}
