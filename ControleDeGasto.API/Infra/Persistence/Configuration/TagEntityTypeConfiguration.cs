using ControleDeGasto.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleDeGasto.API.Infra.Persistence.Configuration
{
    public class TagEntityTypeConfiguration : IEntityTypeConfiguration<Tag>
    {
        #region Constants :: NAME_MAX_LENGTH, COLOR_MAX_LENGTH

        private const int NAME_MAX_LENGTH = 30;
        private const int COLOR_MAX_LENGTH = 7;

        #endregion

        #region Methods :: Configure()

        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(NAME_MAX_LENGTH);

            builder.Property(x => x.Color)
                .IsRequired()
                .HasMaxLength(COLOR_MAX_LENGTH);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Etiqueta é excluída de verdade (o vínculo cai em cascata), então o nome pode ser
            // único sem filtro de ativo.
            builder.HasIndex(x => new { x.UserId, x.Name })
                .IsUnique();
        }

        #endregion
    }
}
