using NovaBugTracker.Models;

namespace NovaBugTracker.Services.Interfaces
{

    public interface IBTTicketService
    {
        public Task AddNewTicketAsync(Ticket ticket);
        public Task AddTicketAttachmentAsync(TicketAttatchment ticketAttachment);
        public Task AddTicketCommentAsync(TicketComment ticketComment);
        public Task ArchiveTicketAsync(Ticket ticket);
        public Task AssignTicketAsync(int ticketId, string userId);
        public Task AssignDeveloperAsync(int ticketId, string userId);
        public Task<List<Ticket>> GetArchivedTicketsAsync(int companyId);
        public Task<TicketAttatchment> GetTicketAttachmentByIdAsync(int ticketAttachmentId);
        public Task<Ticket> GetTicketAsNoTrackingAsync(int ticketId, int companyId);
        public Task<List<Ticket>> GetAllTicketsByCompanyIdAsync(int companyId);
        public Task<Ticket> GetTicketByIdAsync(int ticketId);
        public Task<List<Ticket>> GetTicketsByUserIdAsync(string userId, int companyId);
        public Task<List<Ticket>> GetUnassignedTicketsAsync(int projectId);
        public Task RestoreTicketAsync(Ticket ticket);
        public Task UpdateTicketAsync(Ticket ticket);
    }
}