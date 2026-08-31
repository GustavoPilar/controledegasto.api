using ControleDeGasto.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleDeGasto.API.Infra.Persistence.Configuration
{
    public class FriendshipEntityTypeConfiguration : IEntityTypeConfiguration<Friendship>
    {
        #region Constants :: ENUM_MAX_LENGTH

        private const int ENUM_MAX_LENGTH = 20;

        #endregion

        #region Methods :: Configure()

        public void Configure(EntityTypeBuilder<Friendship> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(ENUM_MAX_LENGTH)
                .HasConversion<string>();

            builder.Property(x => x.RequestedAt)
                .IsRequired();

            builder.HasOne(x => x.Requester)
                .WithMany()
                .HasForeignKey(x => x.RequesterId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Restrict no destinatário: com cascata nos dois lados, o Postgres teria dois
            // caminhos de exclusão para a mesma linha e recusaria a criação da tabela.
            builder.HasOne(x => x.Addressee)
                .WithMany()
                .HasForeignKey(x => x.AddresseeId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // O par não se repete em um sentido. O sentido inverso é barrado pelo serviço, que
            // procura a relação nas duas ordens antes de criar o convite.
            builder.HasIndex(x => new { x.RequesterId, x.AddresseeId })
                .IsUnique();

            // Atende às duas listagens: meus amigos e convites que recebi.
            builder.HasIndex(x => new { x.AddresseeId, x.Status });
            builder.HasIndex(x => new { x.RequesterId, x.Status });

            // Ninguém é amigo de si mesmo. A checagem também existe no serviço, mas aqui ela
            // vale para qualquer caminho de escrita, inclusive script manual.
            builder.ToTable(table => table.HasCheckConstraint(
                "CK_Friendships_DifferentUsers",
                "\"RequesterId\" <> \"AddresseeId\""));
        }

        #endregion
    }
}
