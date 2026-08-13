using ControleDeGasto.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleDeGasto.API.Infra.Persistence.Configuration
{
    public class NotificationEntityTypeConfiguration : IEntityTypeConfiguration<Notification>
    {
        #region Constants :: TITLE_MAX_LENGTH, MESSAGE_MAX_LENGTH, ENUM_MAX_LENGTH

        private const int TITLE_MAX_LENGTH = 120;
        private const int MESSAGE_MAX_LENGTH = 400;
        private const int ENUM_MAX_LENGTH = 30;

        #endregion

        #region Methods :: Configure()

        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type)
                .IsRequired()
                .HasMaxLength(ENUM_MAX_LENGTH)
                .HasConversion<string>();

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(TITLE_MAX_LENGTH);

            builder.Property(x => x.Message)
                .IsRequired()
                .HasMaxLength(MESSAGE_MAX_LENGTH);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.UserId, x.CreatedAt })
                .IsDescending(false, true);

            // Índices parciais: as duas consultas quentes só olham linhas não lidas ou sem
            // e-mail enviado, que são a minoria conforme a base cresce.
            builder.HasIndex(x => x.UserId)
                .HasFilter("\"ReadAt\" IS NULL")
                .HasDatabaseName("IX_Notifications_UserId_Unread");

            builder.HasIndex(x => x.CreatedAt)
                .HasFilter("\"EmailSentAt\" IS NULL")
                .HasDatabaseName("IX_Notifications_CreatedAt_PendingEmail");

            // Atende à verificação de duplicidade antes de criar um aviso repetido.
            builder.HasIndex(x => new { x.UserId, x.Type, x.ReferenceId, x.CreatedAt });
        }

        #endregion
    }
}
