using ControleDeGasto.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleDeGasto.API.Infra.Persistence.Configuration
{
    public class CategoryEntityTypeConfiguration : IEntityTypeConfiguration<Category>
    {
        #region Constants :: NAME_MAX_LENGTH, COLOR_MAX_LENGTH, ICON_MAX_LENGTH, ENUM_MAX_LENGTH

        private const int NAME_MAX_LENGTH = 60;
        private const int COLOR_MAX_LENGTH = 7;
        private const int ICON_MAX_LENGTH = 40;
        private const int ENUM_MAX_LENGTH = 20;

        #endregion

        #region Methods :: Configure()

        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(NAME_MAX_LENGTH);

            builder.Property(x => x.Type)
                .IsRequired()
                .HasMaxLength(ENUM_MAX_LENGTH)
                .HasConversion<string>();

            builder.Property(x => x.Color)
                .IsRequired()
                .HasMaxLength(COLOR_MAX_LENGTH);

            builder.Property(x => x.Icon)
                .IsRequired()
                .HasMaxLength(ICON_MAX_LENGTH);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Índice parcial: o nome só precisa ser único entre as categorias vivas, senão uma
            // categoria excluída logicamente impediria o usuário de recriar o mesmo nome.
            builder.HasIndex(x => new { x.UserId, x.Type, x.Name })
                .IsUnique()
                .HasFilter("\"Active\" = true");

            // Atende à listagem de categorias, que é sempre por usuário.
            builder.HasIndex(x => new { x.UserId, x.Active });
        }

        #endregion
    }
}
