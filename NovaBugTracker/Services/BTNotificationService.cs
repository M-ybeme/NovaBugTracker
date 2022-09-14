using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using NovaBugTracker.Data;
using NovaBugTracker.Models;
using NovaBugTracker.Services.Interfaces;

namespace NovaBugTracker.Services
{
    public class BTNotificationService : IBTNotificationService
    {

        public readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailService;

        public BTNotificationService(ApplicationDbContext context, IEmailSender emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task AddNotificationAsync(Notification notification)
        {
            try
            {
                await _context.AddAsync(notification);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> SendEmailNotificationAsync(Notification notification, string emailSubject)
        {
            try
            {
                BTUser? btUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == notification.RecipientId);

                string userEmail = btUser!.Email;

                await _emailService.SendEmailAsync(userEmail, emailSubject, notification.Message);

                return true;

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
