using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Domain.Enums;
using ControleDeGasto.API.Domain.Interfaces;
using ControleDeGasto.API.Domain.ReadModels;
using ControleDeGasto.API.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControleDeGasto.API.Infra.Repositories
{
    public class NotificationRepository(
        AppDbContext context) : INotificationRepository
    {
        #region Fields

        private readonly AppDbContext context = context;

        #endregion

        #region Methods :: GetPagedAsync(), GetUnreadCountAsync(), ExistsRecentAsync()

        /// <inheritdoc />
        public async Task<PagedResult<Notification>> GetPagedAsync(Guid userId, bool onlyUnread, int page, int pageSize)
        {
            IQueryable<Notification> query = this.context.Notifications
                .AsNoTracking()
                .Where(x => x.UserId == userId);

            if (onlyUnread)
                query = query.Where(x => x.ReadAt == null);

            int totalCount = await query.CountAsync();

            List<Notification> items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Notification>(items, totalCount);
        }

        /// <inheritdoc />
        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await this.context.Notifications
                .AsNoTracking()
                .CountAsync(x => x.UserId == userId && x.ReadAt == null);
        }

        /// <inheritdoc />
        public async Task<bool> ExistsRecentAsync(Guid userId, NotificationType type, Guid? referenceId, DateTime since)
        {
            return await this.context.Notifications
                .AsNoTracking()
                .AnyAsync(x => x.UserId == userId
                    && x.Type == type
                    && x.ReferenceId == referenceId
                    && x.CreatedAt >= since);
        }

        #endregion

        #region Methods :: CreateAsync(), CreateRangeAsync(), MarkAsReadAsync(), MarkAllAsReadAsync()

        /// <inheritdoc />
        public async Task<bool> CreateAsync(Notification notification)
        {
            ArgumentNullException.ThrowIfNull(notification);

            await this.context.Notifications.AddAsync(notification);

            return await this.context.SaveChangesAsync() > 0;
        }

        /// <inheritdoc />
        public async Task<int> CreateRangeAsync(IEnumerable<Notification> notifications)
        {
            ArgumentNullException.ThrowIfNull(notifications);

            await this.context.Notifications.AddRangeAsync(notifications);

            return await this.context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId, DateTime readAt)
        {
            // ExecuteUpdate: um UPDATE direto, sem carregar a entidade para depois gravar.
            int affected = await this.context.Notifications
                .Where(x => x.Id == notificationId && x.UserId == userId && x.ReadAt == null)
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ReadAt, readAt));

            return affected > 0;
        }

        /// <inheritdoc />
        public async Task<int> MarkAllAsReadAsync(Guid userId, DateTime readAt)
        {
            return await this.context.Notifications
                .Where(x => x.UserId == userId && x.ReadAt == null)
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ReadAt, readAt));
        }

        #endregion

        #region Methods :: GetPendingEmailAsync(), MarkEmailSentAsync()

        /// <inheritdoc />
        public async Task<IReadOnlyList<PendingEmailNotification>> GetPendingEmailAsync(int maxCount)
        {
            // Projeção com os dados do destinatário: evita carregar a entidade User inteira
            // apenas para ler e-mail e nome.
            return await this.context.Notifications
                .AsNoTracking()
                .Where(x => x.EmailSentAt == null && x.User!.Active && x.User.EmailConfirmed)
                .OrderBy(x => x.CreatedAt)
                .Take(maxCount)
                .Select(x => new PendingEmailNotification
                {
                    NotificationId = x.Id,
                    UserId = x.UserId,
                    Email = x.User!.Email!,
                    FullName = x.User.FullName,
                    Title = x.Title,
                    Message = x.Message
                })
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<bool> MarkEmailSentAsync(Guid notificationId, DateTime sentAt)
        {
            int affected = await this.context.Notifications
                .Where(x => x.Id == notificationId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.EmailSentAt, sentAt));

            return affected > 0;
        }

        #endregion
    }
}
