using ControleDeGasto.API.Domain.Enums;

namespace ControleDeGasto.API.Application.Utilities
{
    /// <summary>
    /// Perguntas sobre a natureza de uma carteira.
    /// </summary>
    /// <remarks>
    /// Centraliza a lista de naturezas de benefício. Espalhar essa condição pelos serviços faria
    /// com que incluir um vale novo exigisse caçar todos os lugares que enumeram os atuais.
    /// </remarks>
    public static class WalletKindHelper
    {
        #region Fields :: BENEFIT_KINDS

        /// <summary>
        /// Naturezas cujo dinheiro é vinculado: só pode ser gasto no que o benefício aceita e
        /// não se soma ao saldo livre da conta.
        /// </summary>
        private static readonly HashSet<WalletKind> BENEFIT_KINDS =
        [
            WalletKind.MealVoucher,
            WalletKind.FoodVoucher,
            WalletKind.TransportVoucher,
            WalletKind.FuelVoucher,
            WalletKind.CultureVoucher
        ];

        #endregion

        #region Methods :: IsBenefit()

        /// <summary>
        /// Indica se a natureza é de benefício (vale).
        /// </summary>
        /// <param name="kind">Natureza a avaliar.</param>
        /// <returns>True nas carteiras de vale.</returns>
        public static bool IsBenefit(WalletKind kind)
        {
            return BENEFIT_KINDS.Contains(kind);
        }

        #endregion
    }
}
