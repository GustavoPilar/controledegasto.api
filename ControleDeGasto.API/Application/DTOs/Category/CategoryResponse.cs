using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;

namespace ControleDeGasto.API.Application.DTOs
{
    /// <summary>
    /// Categoria devolvida ao cliente.
    /// </summary>
    public class CategoryResponse(Category category)
    {
        #region Properties :: Id, Name, Type, Color, Icon, IsDefault, Active, CreatedAt, UpdatedAt

        public Guid Id { get; set; } = category.Id;

        public string Name { get; set; } = category.Name;

        public TransactionType Type { get; set; } = category.Type;

        public string Color { get; set; } = category.Color;

        public string Icon { get; set; } = category.Icon;

        public bool IsDefault { get; set; } = category.IsDefault;

        public bool Active { get; set; } = category.Active;

        public DateTime CreatedAt { get; set; } = category.CreatedAt;

        public DateTime? UpdatedAt { get; set; } = category.UpdatedAt;

        #endregion
    }
}
