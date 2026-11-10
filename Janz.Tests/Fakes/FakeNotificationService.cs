using Domain.Enums;
using Service.Helpers.DTOs.Notifications;
using Service.Services.Interfaces;

namespace Janz.Tests.Fakes
{
    // Bildirişləri yalnız yadda saxlayır: testdə kimə nə yazıldığını yoxlamaq üçün
    public class FakeNotificationService : INotificationService
    {
        public List<(string UserId, NotificationKind Kind, string Title, string Text, string? Link)> Added { get; } = new();

        public Task<NotificationListDto> GetAllAsync(string userId) => Task.FromResult(new NotificationListDto());

        public Task MarkReadAsync(string userId, int id) => Task.CompletedTask;

        public Task MarkAllReadAsync(string userId) => Task.CompletedTask;

        public Task AddAsync(string userId, NotificationKind kind, string title, string text, string? link)
        {
            Added.Add((userId, kind, title, text, link));
            return Task.CompletedTask;
        }
    }
}
