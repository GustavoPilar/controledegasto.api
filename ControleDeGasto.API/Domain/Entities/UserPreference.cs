using ControleDeGasto.API.Domain.Enums;

namespace ControleDeGasto.API.Domain.Entities
{
    /// <summary>
    /// Preferências de interface de um usuário. Relação 1:1 com <see cref="User"/>.
    /// </summary>
    /// <remarks>
    /// Usa <see cref="UserId"/> como chave primária (shared primary key). Uma chave própria
    /// somada a um índice único em UserId seria redundante: a preferência não tem identidade
    /// fora do usuário dono.
    /// </remarks>
    public class UserPreference
    {
        #region Properties :: UserId, Appearance, CreatedAt, UpdatedAt, User

        /// <summary>Identificador do usuário dono. Também é a chave primária da tabela.</summary>
        public Guid UserId { get; set; }

        /// <summary>Tema de interface escolhido.</summary>
        public AppearanceType Appearance { get; set; } = AppearanceType.Light;

        /// <summary>Momento da criação, em UTC.</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Momento da última alteração, em UTC. Nulo enquanto nunca foi alterada.</summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>Usuário dono da preferência.</summary>
        public User? User { get; set; }

        #endregion
    }
}
