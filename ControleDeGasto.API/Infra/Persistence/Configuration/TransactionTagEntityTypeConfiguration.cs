using ControleDeGasto.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleDeGasto.API.Infra.Persistence.Configuration
{
    public class TransactionTagEntityTypeConfiguration : IEntityTypeConfiguration<TransactionTag>
    {
        #region Methods :: Configure()

        public void Configure(EntityTypeBuilder<TransactionTag> builder)
        {
            // Chave composta: o par já identifica o vínculo, e uma chave própria só criaria a
            // possibilidade de gravar a mesma etiqueta duas vezes no mesmo lançamento.
            builder.HasKey(x => new { x.TransactionId, x.TagId });

            builder.HasOne(x => x.Transaction)
                .WithMany(x => x.Tags)
                .HasForeignKey(x => x.TransactionId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Tag)
                .WithMany()
                .HasForeignKey(x => x.TagId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Atende ao filtro por etiqueta, que parte da etiqueta para os lançamentos — o
            // sentido oposto ao da chave primária.
            builder.HasIndex(x => x.TagId);
        }

        #endregion
    }
}
