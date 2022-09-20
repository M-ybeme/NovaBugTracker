using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NovaBugTracker.Models;

namespace NovaBugTracker.Data
{
    public class ApplicationDbContext : IdentityDbContext<BTUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<NovaBugTracker.Models.Company>? Companies { get; set; }
        public DbSet<NovaBugTracker.Models.Invite>? Invites { get; set; }
        public DbSet<NovaBugTracker.Models.Notification>? Notifications { get; set; }
        public DbSet<NovaBugTracker.Models.NotificationType>? NotificationTypes { get; set; }
        public DbSet<NovaBugTracker.Models.Project>? Projects { get; set; }
        public DbSet<NovaBugTracker.Models.ProjectPriority>? ProjectPriorities { get; set; }
        public DbSet<NovaBugTracker.Models.Ticket>? Tickets { get; set; }
        public DbSet<NovaBugTracker.Models.TicketAttatchment>? TicketAttatchments { get; set; }
        public DbSet<NovaBugTracker.Models.TicketComment>? TicketComments { get; set; }
        public DbSet<NovaBugTracker.Models.TicketHistory>? TicketHistories { get; set; }
        public DbSet<NovaBugTracker.Models.TicketPriority>? TicketPriorities { get; set; }
        public DbSet<NovaBugTracker.Models.TicketStatus>? TicketStatuses { get; set; }
        public DbSet<NovaBugTracker.Models.TicketType>? TicketTypes { get; set; }
    }
}