namespace ControleDeGasto.API.Domain.Enums
{
    /// <summary>
    /// Natureza de uma carteira (conta, dinheiro, cartão ou benefício).
    /// </summary>
    /// <remarks>
    /// Os benefícios (vale-refeição, vale-alimentação, vale-transporte, vale-combustível) são
    /// carteiras como qualquer outra, e não um campo à parte do lançamento: o dinheiro do vale
    /// entra, sai e tem saldo próprio exatamente como o de uma conta. Modelar assim é o que
    /// permite responder "quanto do meu VR foi para mercado e quanto foi para restaurante"
    /// reaproveitando as categorias que já existem.
    /// </remarks>
    public enum WalletKind
    {
        /// <summary>Conta corrente.</summary>
        Checking = 1,

        /// <summary>Conta poupança ou conta de investimento.</summary>
        Savings = 2,

        /// <summary>Dinheiro em espécie.</summary>
        Cash = 3,

        /// <summary>Cartão de crédito.</summary>
        CreditCard = 4,

        /// <summary>Vale-refeição (VR).</summary>
        MealVoucher = 5,

        /// <summary>Vale-alimentação (VA).</summary>
        FoodVoucher = 6,

        /// <summary>Vale-transporte (VT).</summary>
        TransportVoucher = 7,

        /// <summary>Vale-combustível (VC).</summary>
        FuelVoucher = 8,

        /// <summary>Vale-cultura.</summary>
        CultureVoucher = 9,

        /// <summary>Outra natureza não listada.</summary>
        Other = 99
    }
}
