using NovaBugTracker.Models;

namespace NovaBugTracker.Services.Interfaces
{
    public interface IBTNotificationService
    {
        public Task AddNotificationAsync(Notification notification);

        public Task<bool> SendEmailNotificationAsync(Notification notification, string emailSubject);
    }
}
