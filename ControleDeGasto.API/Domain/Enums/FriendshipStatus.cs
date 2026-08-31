namespace ControleDeGasto.API.Domain.Enums
{
    /// <summary>
    /// Situação de um pedido de amizade.
    /// </summary>
    /// <remarks>
    /// O convite recusado permanece na base em vez de ser apagado: sem ele, quem foi recusado
    /// poderia reenviar o pedido indefinidamente, e o histórico de bloqueio se perderia.
    /// </remarks>
    public enum FriendshipStatus
    {
        /// <summary>Convite enviado, aguardando resposta do destinatário.</summary>
        Pending = 1,

        /// <summary>Convite aceito: os dois usuários são amigos.</summary>
        Accepted = 2,

        /// <summary>Convite recusado pelo destinatário.</summary>
        Declined = 3,

        /// <summary>Relação bloqueada: nenhum dos lados pode enviar novo convite.</summary>
        Blocked = 4
    }
}
