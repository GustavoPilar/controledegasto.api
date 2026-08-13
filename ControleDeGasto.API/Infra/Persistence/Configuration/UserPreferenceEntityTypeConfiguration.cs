using ControleDeGasto.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleDeGasto.API.Infra.Persistence.Configuration
{
    public class UserPreferenceEntityTypeConfiguration : IEntityTypeConfiguration<UserPreference>
    {
        #region Constants :: APPEARANCE_MAX_LENGTH

        /// <summary>Comprimento máximo da coluna de aparência. Cabe o maior nome do enum com folga.</summary>
        private const int APPEARANCE_MAX_LENGTH = 20;

        #endregion

        #region Methods :: Configure()

        public void Configure(EntityTypeBuilder<UserPreference> builder)
        {
            // Shared primary key: a preferência é identificada pelo usuário dono.
            builder.HasKey(x => x.UserId);

            // Persistido como texto em vez de inteiro para o dado ser legível em consultas
            // diretas ao banco e não depender da ordem dos membros do enum.
            // Sem HasDefaultValue de propósito: o padrão (Light) é decidido no domínio/serviço,
            // e um default no banco duplicaria essa regra em um segundo lugar.
            builder.Property(x => x.Appearance)
                .IsRequired()
                .HasMaxLength(APPEARANCE_MAX_LENGTH)
                .HasConversion<string>();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            // Relação 1:1 com o usuário: a preferência não existe sem o dono, por isso o
            // Cascade remove a preferência junto com o usuário.
            builder.HasOne(x => x.User)
                .WithOne(x => x.UserPreference)
                .HasForeignKey<UserPreference>(x => x.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }

        #endregion
    }
}
