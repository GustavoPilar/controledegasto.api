namespace ControleDeGasto.API.Domain.Enums
{
    /// <summary>
    /// Situação de um cofrinho.
    /// </summary>
    public enum SavingsGoalStatus
    {
        /// <summary>Em andamento, aceita novos aportes.</summary>
        Active = 1,

        /// <summary>Meta alcançada.</summary>
        Completed = 2,

        /// <summary>Arquivado pelo usuário: não aparece nos painéis nem gera notificação.</summary>
        Archived = 3
    }
}
