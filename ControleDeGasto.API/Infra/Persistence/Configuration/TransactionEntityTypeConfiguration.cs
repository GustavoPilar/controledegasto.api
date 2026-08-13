using ControleDeGasto.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleDeGasto.API.Infra.Persistence.Configuration
{
    public class TransactionEntityTypeConfiguration : IEntityTypeConfiguration<Transaction>
    {
        #region Constants :: DESCRIPTION_MAX_LENGTH, ENUM_MAX_LENGTH, MONEY_PRECISION, MONEY_SCALE

        private const int DESCRIPTION_MAX_LENGTH = 120;
        private const int ENUM_MAX_LENGTH = 20;
        private const int MONEY_PRECISION = 18;
        private const int MONEY_SCALE = 2;

        #endregion

        #region Methods :: Configure()

        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.HasKey(x => x.Id);

            // decimal com escala fixa, nunca double: dinheiro não tolera erro de arredondamento
            // binário.
            builder.Property(x => x.Amount)
                .IsRequired()
                .HasPrecision(MONEY_PRECISION, MONEY_SCALE);

            builder.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(DESCRIPTION_MAX_LENGTH);

            builder.Property(x => x.OccurredOn)
                .IsRequired();

            builder.Property(x => x.PaymentMethod)
                .IsRequired()
                .HasMaxLength(ENUM_MAX_LENGTH)
                .HasConversion<string>();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Restrict: apagar categoria com lançamentos deixaria o histórico sem classificação.
            // A exclusão de categoria é lógica justamente por isso.
            builder.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Índice principal de leitura: extrato e relatórios sempre filtram por usuário e
            // período, ordenando do mais recente para o mais antigo.
            builder.HasIndex(x => new { x.UserId, x.OccurredOn })
                .IsDescending(false, true);

            // Atende aos agrupamentos por categoria dos painéis.
            builder.HasIndex(x => new { x.UserId, x.CategoryId, x.OccurredOn });
        }

        #endregion
    }
}
