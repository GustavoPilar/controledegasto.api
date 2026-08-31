namespace ControleDeGasto.API.Domain.Enums
{
    /// <summary>
    /// Papel de um participante em um cofrinho compartilhado.
    /// </summary>
    public enum SavingsGoalMemberRole
    {
        /// <summary>Criador do cofrinho. Pode editar, convidar, remover participantes e excluir.</summary>
        Owner = 1,

        /// <summary>Participante convidado. Pode ver o saldo e registrar os próprios aportes.</summary>
        Member = 2
    }
}
