using ControleDeGasto.API.Application.Utilities;
using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;
using ControleDeGasto.API.Domain.ReadModels;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Carteira devolvida ao cliente, com saldo atual e saldo projetado já calculados.
    /// </summary>
    /// <remarks>
    /// Os saldos são calculados aqui, e não no cliente, para que a listagem, o painel e o
    /// formulário de lançamento mostrem o mesmo número e a regra viva em um só lugar.
    /// </remarks>
    public class WalletResponse
    {
        #region Constructors

        /// <summary>
        /// Cria a resposta a partir da carteira e dos totais apurados.
        /// </summary>
        /// <param name="wallet">Carteira de origem.</param>
        /// <param name="balance">Movimento apurado da carteira, ou nulo quando não houve movimento.</param>
        /// <param name="transfers">Transferências apuradas, ou nulo quando não houve transferência.</param>
        /// <param name="credit">Crédito fixo mensal que abastece a carteira, ou nulo quando não há.</param>
        /// <param name="reference">Momento usado como "hoje" ao contar os créditos já ocorridos, em UTC.</param>
        public WalletResponse(
            Wallet wallet,
            WalletBalance? balance,
            WalletTransferTotal? transfers,
            FixedEntry? credit = null,
            DateTime? reference = null)
        {
            ArgumentNullException.ThrowIfNull(wallet);

            this.Id = wallet.Id;
            this.Name = wallet.Name;
            this.Kind = wallet.Kind;
            this.Color = wallet.Color;
            this.Icon = wallet.Icon;
            this.InitialBalance = wallet.InitialBalance;
            this.CreditLimit = wallet.CreditLimit;
            this.StatementClosingDay = wallet.StatementClosingDay;
            this.PaymentDueDay = wallet.PaymentDueDay;
            this.IsDefault = wallet.IsDefault;
            this.Active = wallet.Active;
            this.IsBenefit = WalletKindHelper.IsBenefit(wallet.Kind);
            this.CreatedAt = wallet.CreatedAt;

            DateTime now = reference ?? DateTime.UtcNow;

            decimal movement = balance?.MovementBalance ?? 0;
            decimal transferredIn = transfers?.TransferredIn ?? 0;
            decimal transferredOut = transfers?.TransferredOut ?? 0;

            this.TransferredIn = transferredIn;
            this.TransferredOut = transferredOut;

            // O crédito de benefício é reconstruído a partir da definição em vez de gravado mês
            // a mês. É o que permite a carteira de VR ter saldo correto sem uma linha nova por
            // mês; um mês em que o valor tiver sido diferente é corrigido com um lançamento.
            if (credit is not null)
            {
                this.MonthlyCredit = credit.Amount;
                this.CreditDayOfMonth = credit.DayOfMonth;

                int occurrences = FixedEntryHelper.CountOccurrencesUntil(credit, now);

                this.BenefitCreditedAmount = occurrences * credit.Amount;

                DateTime thisMonthCredit = FixedEntryHelper.ResolveDateInMonth(credit.DayOfMonth, now);

                this.NextCreditOn = thisMonthCredit > now
                    ? thisMonthCredit
                    : FixedEntryHelper.ResolveDateInMonth(credit.DayOfMonth, DateTimeHelper.StartOfMonth(now).AddMonths(1));
            }

            this.CurrentBalance = wallet.InitialBalance
                + movement
                + transferredIn
                - transferredOut
                + this.BenefitCreditedAmount;

            this.PendingIncome = balance?.PendingIncome ?? 0;
            this.PendingExpense = balance?.PendingExpense ?? 0;

            this.ProjectedBalance = this.CurrentBalance + this.PendingIncome - this.PendingExpense;

            // O limite disponível só faz sentido no cartão: nas outras naturezas não existe
            // limite, e devolver zero seria lido como "sem limite disponível".
            this.AvailableLimit = wallet.Kind == WalletKind.CreditCard && wallet.CreditLimit.HasValue
                ? wallet.CreditLimit.Value + Math.Min(0, this.ProjectedBalance)
                : null;
        }

        #endregion

        #region Properties :: Id, Name, Kind, Color, Icon, IsBenefit, IsDefault, Active, CreatedAt

        public Guid Id { get; set; }

        public string Name { get; set; }

        public WalletKind Kind { get; set; }

        public string Color { get; set; }

        public string Icon { get; set; }

        /// <summary>Verdadeiro nas carteiras de vale (VR, VA, VT, VC, vale-cultura).</summary>
        public bool IsBenefit { get; set; }

        public bool IsDefault { get; set; }

        public bool Active { get; set; }

        public DateTime CreatedAt { get; set; }

        #endregion

        #region Properties :: InitialBalance, CurrentBalance, PendingIncome, PendingExpense, ProjectedBalance, TransferredIn, TransferredOut

        public decimal InitialBalance { get; set; }

        /// <summary>
        /// Saldo inicial mais os lançamentos liquidados, as transferências e os créditos de
        /// benefício já ocorridos.
        /// </summary>
        public decimal CurrentBalance { get; set; }

        /// <summary>Entradas previstas que ainda não caíram.</summary>
        public decimal PendingIncome { get; set; }

        /// <summary>Saídas previstas que ainda não saíram.</summary>
        public decimal PendingExpense { get; set; }

        /// <summary>Saldo atual mais o previsto: o quanto sobra se tudo for liquidado.</summary>
        public decimal ProjectedBalance { get; set; }

        public decimal TransferredIn { get; set; }

        public decimal TransferredOut { get; set; }

        #endregion

        #region Properties :: MonthlyCredit, CreditDayOfMonth, BenefitCreditedAmount, NextCreditOn

        /// <summary>Valor do crédito fixo mensal. Nulo quando não há crédito cadastrado.</summary>
        public decimal? MonthlyCredit { get; set; }

        /// <summary>Dia do mês em que o crédito cai. Nulo quando não há crédito cadastrado.</summary>
        public int? CreditDayOfMonth { get; set; }

        /// <summary>Total já creditado desde o início da vigência do crédito fixo.</summary>
        public decimal BenefitCreditedAmount { get; set; }

        /// <summary>Data do próximo crédito. Nulo quando não há crédito cadastrado.</summary>
        public DateTime? NextCreditOn { get; set; }

        #endregion

        #region Properties :: CreditLimit, AvailableLimit, StatementClosingDay, PaymentDueDay

        public decimal? CreditLimit { get; set; }

        /// <summary>Limite ainda disponível. Nulo fora do cartão de crédito.</summary>
        public decimal? AvailableLimit { get; set; }

        public int? StatementClosingDay { get; set; }

        public int? PaymentDueDay { get; set; }

        #endregion
    }
}
