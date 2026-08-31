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

            // Default no banco além do default da entidade: as linhas já existentes recebem
            // "liquidado" na migração, e não NULL, que quebraria a conversão para o enum.
            //
            // O sentinela é o zero, que não é membro do enum: assim o default do banco só entra
            // quando a situação realmente não foi informada, e nunca sobrescreve um "previsto"
            // enviado pela aplicação.
            builder.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(ENUM_MAX_LENGTH)
                .HasConversion<string>()
                .HasDefaultValue(Domain.Enums.TransactionStatus.Settled)
                .HasSentinel(default(Domain.Enums.TransactionStatus));

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

            // Mesmo motivo da categoria: o saldo histórico da carteira depende dos lançamentos.
            builder.HasOne(x => x.Wallet)
                .WithMany()
                .HasForeignKey(x => x.WalletId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Cascata a partir do plano: cancelar a compra parcelada leva as parcelas junto,
            // que é exatamente o que "cancelar o parcelamento" significa.
            builder.HasOne(x => x.InstallmentPlan)
                .WithMany(x => x.Installments)
                .HasForeignKey(x => x.InstallmentPlanId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            // Índice principal de leitura: extrato e relatórios sempre filtram por usuário e
            // período, ordenando do mais recente para o mais antigo.
            builder.HasIndex(x => new { x.UserId, x.OccurredOn })
                .IsDescending(false, true);

            // Atende aos agrupamentos por categoria dos painéis.
            builder.HasIndex(x => new { x.UserId, x.CategoryId, x.OccurredOn });

            // Atende ao saldo por carteira e ao extrato de uma carteira.
            builder.HasIndex(x => new { x.UserId, x.WalletId, x.OccurredOn });

            // Índice parcial das contas em aberto: é a consulta do bloco "a pagar" e do aviso
            // de vencimento, e as linhas pendentes são a minoria conforme a base cresce.
            builder.HasIndex(x => new { x.UserId, x.DueDate })
                .HasFilter("\"Status\" = 'Pending'")
                .HasDatabaseName("IX_Transactions_UserId_DueDate_Pending");

            builder.HasIndex(x => x.InstallmentPlanId);

            builder.ToTable(table => table.HasCheckConstraint(
                "CK_Transactions_InstallmentNumber",
                "\"InstallmentNumber\" IS NULL OR \"InstallmentNumber\" >= 1"));
        }

        #endregion
    }
}
