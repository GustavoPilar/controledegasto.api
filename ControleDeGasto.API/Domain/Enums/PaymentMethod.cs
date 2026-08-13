namespace ControleDeGasto.API.Domain.Enums
{
    /// <summary>
    /// Forma de pagamento de um lançamento.
    /// </summary>
    public enum PaymentMethod
    {
        /// <summary>Dinheiro em espécie.</summary>
        Cash = 1,

        /// <summary>Cartão de débito.</summary>
        DebitCard = 2,

        /// <summary>Cartão de crédito.</summary>
        CreditCard = 3,

        /// <summary>Pix.</summary>
        Pix = 4,

        /// <summary>Transferência bancária (TED/DOC).</summary>
        BankTransfer = 5,

        /// <summary>Boleto bancário.</summary>
        Slip = 6,

        /// <summary>Outra forma não listada.</summary>
        Other = 99
    }
}
