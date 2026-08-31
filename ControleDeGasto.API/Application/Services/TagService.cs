using ControleDeGasto.API.Application.DTOs;
using ControleDeGasto.API.Application.Interfaces;
using ControleDeGasto.API.Application.Utilities;
using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Exceptions;
using ControleDeGasto.API.Domain.Interfaces;
using ControleDeGasto.API.Domain.ReadModels;

namespace ControleDeGasto.API.Application.Services
{
    public class TagService(
        ITagRepository repository,
        ILogger<TagService> logger) : ITagService
    {
        #region Constants :: MAX_TAGS_PER_USER

        /// <summary>
        /// Teto de etiquetas por usuário. Etiqueta é filtro: passando de algumas dezenas ela
        /// deixa de organizar e passa a atrapalhar, além de inflar a tela de seleção.
        /// </summary>
        private const int MAX_TAGS_PER_USER = 50;

        #endregion

        #region Fields

        private readonly ITagRepository repository = repository;
        private readonly ILogger<TagService> logger = logger;

        #endregion

        #region Methods :: GetAllAsync(), CreateAsync(), UpdateAsync(), DeleteAsync()

        /// <inheritdoc />
        public async Task<IReadOnlyList<TagResponse>> GetAllAsync(Guid userId)
        {
            IReadOnlyList<Tag> tags = await this.repository.GetAllAsync(userId);

            if (tags.Count == 0)
                return [];

            IReadOnlyDictionary<Guid, int> usage = await this.repository.GetUsageCountAsync(userId);

            return tags
                .Select(tag => new TagResponse(tag, usage.TryGetValue(tag.Id, out int count) ? count : 0))
                .ToList();
        }

        /// <inheritdoc />
        public async Task<TagResponse> CreateAsync(Guid userId, TagRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            string name = request.Name.Trim();

            bool exists = await this.repository.ExistsByNameAsync(userId, name, null);

            if (exists)
                throw new BusinessRuleViolationException("Já existe uma etiqueta com esse nome.");

            IReadOnlyList<Tag> current = await this.repository.GetAllAsync(userId);

            if (current.Count >= MAX_TAGS_PER_USER)
                throw new BusinessRuleViolationException($"Você atingiu o limite de {MAX_TAGS_PER_USER} etiquetas.");

            Tag tag = new Tag()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = name,
                Color = request.Color.ToUpperInvariant(),
                CreatedAt = DateTime.UtcNow
            };

            bool created = await this.repository.CreateAsync(tag);

            if (!created)
                throw new BusinessRuleViolationException("Não foi possível criar a etiqueta.");

            this.logger.LogInformation("Etiqueta {TagId} criada para o usuário {UserId}.", tag.Id, userId);

            return new TagResponse(tag, 0);
        }

        /// <inheritdoc />
        public async Task<TagResponse?> UpdateAsync(Guid userId, Guid tagId, TagRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            Tag? tag = await this.repository.GetByIdAsync(userId, tagId);

            if (tag is null)
                return null;

            string name = request.Name.Trim();

            bool exists = await this.repository.ExistsByNameAsync(userId, name, tagId);

            if (exists)
                throw new BusinessRuleViolationException("Já existe uma etiqueta com esse nome.");

            tag.Name = name;
            tag.Color = request.Color.ToUpperInvariant();

            bool updated = await this.repository.UpdateAsync(tag);

            if (!updated)
                throw new BusinessRuleViolationException("Não foi possível atualizar a etiqueta.");

            IReadOnlyDictionary<Guid, int> usage = await this.repository.GetUsageCountAsync(userId);

            return new TagResponse(tag, usage.TryGetValue(tag.Id, out int count) ? count : 0);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(Guid userId, Guid tagId)
        {
            Tag? tag = await this.repository.GetByIdAsync(userId, tagId);

            if (tag is null)
                return false;

            // Exclusão física: a etiqueta não classifica o lançamento, apenas o marca. Perder a
            // marcação não distorce nenhum relatório de valores, ao contrário da categoria.
            bool deleted = await this.repository.DeleteAsync(tag);

            if (deleted)
                this.logger.LogInformation("Etiqueta {TagId} removida pelo usuário {UserId}.", tagId, userId);

            return deleted;
        }

        #endregion

        #region Methods :: GetTotalsAsync()

        /// <inheritdoc />
        public async Task<IReadOnlyList<TagTotalResponse>> GetTotalsAsync(Guid userId, DateTime? from, DateTime? to)
        {
            DateTime now = DateTime.UtcNow;

            DateTime start = from.HasValue ? DateTimeHelper.ToUtcDate(from.Value) : DateTimeHelper.StartOfMonth(now);
            DateTime end = to.HasValue ? DateTimeHelper.ToUtcEndOfDay(to.Value) : DateTimeHelper.EndOfMonth(now);

            IReadOnlyList<TagTotal> totals = await this.repository.GetTotalsAsync(userId, start, end);

            return totals
                .Select(total => new TagTotalResponse(total))
                .ToList();
        }

        #endregion
    }
}
