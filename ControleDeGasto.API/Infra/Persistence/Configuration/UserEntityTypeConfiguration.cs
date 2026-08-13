using ControleDeGasto.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleDeGasto.API.Infra.Persistence.Configuration
{
    public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
    {
        #region Methods :: Configure()
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(x => x.FullName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired(false);

            builder.Property(x => x.DeactivatedAt)
                .IsRequired(false);

            builder.Property(x => x.Active)
                .IsRequired()
                .HasDefaultValueSql("false");

            builder.Property(x => x.UserName)
                .IsRequired()
                .HasMaxLength(50);
        }

        #endregion
    }
}
