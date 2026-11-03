using Domain.Entities;
using Domain.Enums;
using Repository.Repositories.Interfaces;
using Service.Helpers.DTOs.Notifications;
using Service.Helpers.Exceptions;
using Service.Services.Interfaces;

namespace Service.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepo;

        public NotificationService(INotificationRepository notificationRepo)
        {
            _notificationRepo = notificationRepo;
        }

        public async Task<NotificationListDto> GetAllAsync(string userId)
        {
            var items = await _notificationRepo.GetByUserAsync(userId);
            return new NotificationListDto
            {
                Unread = await _notificationRepo.CountUnreadAsync(userId),
                Items = items.Select(m => new NotificationDto
                {
                    Id = m.Id,
                    Kind = m.Kind.ToString(),
                    Title = m.Title,
                    Text = m.Text,
                    Link = m.Link,
                    IsRead = m.IsRead,
                    CreatedAt = m.CreatedAt
                }).ToList()
            };
        }

        public async Task MarkReadAsync(string userId, int id)
        {
            if (!await _notificationRepo.MarkReadAsync(userId, id))
            {
                throw new NotFoundException();
            }
        }

        public async Task MarkAllReadAsync(string userId)
        {
            await _notificationRepo.MarkAllReadAsync(userId);
        }

        public async Task AddAsync(string userId, NotificationKind kind, string title, string text, string? link)
        {
            await _notificationRepo.AddAsync(new Notification
            {
                UserId = userId,
                Kind = kind,
                Title = title,
                Text = text,
                Link = link
            });
        }
    }
}
