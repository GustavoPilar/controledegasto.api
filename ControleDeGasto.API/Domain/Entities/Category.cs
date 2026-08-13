using ControleDeGasto.API.Domain.Enums;

namespace ControleDeGasto.API.Domain.Entities
{
    /// <summary>
    /// Categoria de lançamento (Alimentação, Salário, Transporte...). Pertence a um usuário.
    /// </summary>
    /// <remarks>
    /// A exclusão é lógica (<see cref="Active"/>): remover fisicamente apagaria o histórico
    /// de lançamentos já classificados, distorcendo os relatórios de meses fechados.
    /// </remarks>
    public class Category
    {
        #region Properties :: Id, UserId, Name, Type, Color, Icon, IsDefault, Active, CreatedAt, UpdatedAt, User

        public Guid Id { get; set; }

        /// <summary>Dono da categoria.</summary>
        public Guid UserId { get; set; }

        public string Name { get; set; } = string.Empty;

        /// <summary>Define se a categoria classifica entradas ou saídas.</summary>
        public TransactionType Type { get; set; }

        /// <summary>Cor em hexadecimal (#RRGGBB), usada nos gráficos.</summary>
        public string Color { get; set; } = string.Empty;

        /// <summary>Nome do ícone exibido na interface.</summary>
        public string Icon { get; set; } = string.Empty;

        /// <summary>Indica categoria criada automaticamente no cadastro da conta.</summary>
        public bool IsDefault { get; set; }

        /// <summary>Falso quando excluída logicamente.</summary>
        public bool Active { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public User? User { get; set; }

        #endregion
    }
}
