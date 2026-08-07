using ControleDeGasto.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleDeGasto.API.Infra.Persistence
{
    public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(x => x.FullName)
                .IsRequired()
                .HasColumnType("varchar(150)");

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired(false);

            builder.Property(x => x.DeactivatedAt)
                .IsRequired(false);

            builder.Property(x => x.Active)
                .IsRequired(true)
                .HasDefaultValueSql("true");

            builder.Property(x => x.UserName)
                .IsRequired();
        }
    }
}
