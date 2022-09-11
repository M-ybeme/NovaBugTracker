using Microsoft.EntityFrameworkCore;
using NovaBugTracker.Data;
using NovaBugTracker.Models;
using NovaBugTracker.Services.Interfaces;

namespace NovaBugTracker.Services
{
    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;

        public BTTicketService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddNewTicketAsync(Ticket ticket)
        {
            try
            {
                await _context.AddAsync(ticket);
                await _context.SaveChangesAsync();
            }
            catch { throw; }
        }

        public async Task AddTicketAttachmentAsync(TicketAttatchment ticketAttachment)
        {
            try
            {
                await _context.AddAsync(ticketAttachment);
                await _context.SaveChangesAsync();
            }
            catch { throw; }
        }

        public async Task AddTicketCommentAsync(TicketComment ticketComment)
        {
            try
            {
                await _context.AddAsync(ticketComment);
                await _context.SaveChangesAsync();
            }
            catch { throw; }
        }

        public async Task ArchiveTicketAsync(Ticket ticket)
        {
            try
            {
                ticket.Archived = true;
                await UpdateTicketAsync(ticket);
            }
            catch { throw; }
        }

        public async Task AssignTicketAsync(int ticketId, string userId)
        {
            try
            {
                Ticket ticket = await GetTicketByIdAsync(ticketId);
                ticket.DeveloperUserId = userId;

                await UpdateTicketAsync(ticket);
            }
            catch { throw; }
        }

        public async Task AssignDeveloperAsync(int ticketId, string userId)
        {
            await AssignTicketAsync(ticketId, userId);
        }

        public async Task<List<Ticket>> GetAllTicketsByCompanyIdAsync(int companyId)
        {
            try
            {
                return await _context.Tickets!
                    .Where(t => t.Project!.CompanyId == companyId && !t.Archived)
                    .Include(t => t.Project).ThenInclude(p => p!.Members)
                    .Include(t => t.Project).ThenInclude(p => p!.ProjectPriority)
                    .Include(t => t.TicketPriority)
                    .Include(t => t.TicketType)
                    .Include(t => t.TicketStatus)
                    .Include(t => t.DeveloperUser)
                    .Include(t => t.SubmitterUser)
                    .Include(t => t.Comments).ThenInclude(c => c.User)
                    .Include(t => t.Attatchment).ThenInclude(a => a.User)
                    .Include(t => t.History).ThenInclude(h => h.User)
                    .ToListAsync();
            }
            catch { throw; }
        }

        public async Task<List<Ticket>> GetArchivedTicketsAsync(int companyId)
        {
            try
            {
                return await _context.Tickets!
                    .Where(t => t.Project!.CompanyId == companyId && t.Archived)
                    .Include(t => t.Project).ThenInclude(p => p!.Members)
                    .Include(t => t.Project).ThenInclude(p => p!.ProjectPriority)
                    .Include(t => t.TicketPriority)
                    .Include(t => t.TicketType)
                    .Include(t => t.TicketStatus)
                    .Include(t => t.DeveloperUser)
                    .Include(t => t.SubmitterUser)
                    .Include(t => t.Comments).ThenInclude(c => c.User)
                    .Include(t => t.Attatchment).ThenInclude(a => a.User)
                    .Include(t => t.History).ThenInclude(h => h.User)
                    .ToListAsync();
            }
            catch { throw; }
        }

        public async Task<Ticket> GetTicketAsNoTrackingAsync(int ticketId)
        {
            try
            {
                Ticket? ticket = await _context.Tickets!
                    .AsNoTracking()
                    .Where(t => t.Id == ticketId)
                    .Include(t => t.Project)
                    .Include(t => t.TicketPriority)
                    .Include(t => t.TicketType)
                    .Include(t => t.TicketStatus)
                    .Include(t => t.DeveloperUser)
                    .Include(t => t.SubmitterUser)
                    .Include(t => t.Comments).ThenInclude(c => c.User)
                    .Include(t => t.Attatchment).ThenInclude(a => a.User)
                    .Include(t => t.History).ThenInclude(h => h.User)
                    .FirstOrDefaultAsync();

                return ticket!;
            }
            catch { throw; }
        }

        public async Task<TicketAttatchment> GetTicketAttachmentByIdAsync(int ticketAttachmentId)
        {
            try
            {
                TicketAttatchment? attachment = await _context.TicketAttatchments!
                                                              .Where(a => a.Id == ticketAttachmentId)
                                                              .Include(a => a.Ticket)
                                                              .Include(a => a.User)
                                                              .FirstOrDefaultAsync();
                return attachment!;
            }
            catch { throw; }
        }

        public async Task<Ticket> GetTicketByIdAsync(int ticketId)
        {
            try
            {
                Ticket? ticket = await _context.Tickets!.Where(t => t.Id == ticketId)
                    .Include(t => t.Project).ThenInclude(p => p!.Members)
                    .Include(t => t.Project).ThenInclude(p => p!.ProjectPriority)
                    .Include(t => t.TicketPriority)
                    .Include(t => t.TicketType)
                    .Include(t => t.TicketStatus)
                    .Include(t => t.DeveloperUser)
                    .Include(t => t.SubmitterUser)
                    .Include(t => t.Comments).ThenInclude(c => c.User)
                    .Include(t => t.Attatchment).ThenInclude(a => a.User)
                    .Include(t => t.History).ThenInclude(h => h.User)
                    .FirstOrDefaultAsync();

                return ticket!;
            }
            catch { throw; }
        }

        public async Task<List<Ticket>> GetTicketsByUserIdAsync(string userId, int companyId)
        {
            try
            {
                return await _context.Tickets!
                    .Where(t => (t.DeveloperUserId == userId || t.SubmitterUserId == userId)
                                    && t.Project!.CompanyId == companyId
                                    && !t.Archived)
                    .Include(t => t.Project).ThenInclude(p => p!.Members)
                    .Include(t => t.Project).ThenInclude(p => p!.ProjectPriority)
                    .Include(t => t.TicketPriority)
                    .Include(t => t.TicketType)
                    .Include(t => t.TicketStatus)
                    .Include(t => t.DeveloperUser)
                    .Include(t => t.SubmitterUser)
                    .Include(t => t.Comments).ThenInclude(c => c.User)
                    .Include(t => t.Attatchment).ThenInclude(a => a.User)
                    .Include(t => t.History).ThenInclude(h => h.User)
                    .ToListAsync();
            }
            catch { throw; }
        }
        public async Task<List<Ticket>> GetUnassignedTicketsAsync(int companyId)
        {
            try
            {
                return await _context.Tickets!
                    .Where(t => t.Project!.CompanyId == companyId && t.DeveloperUserId == null && !t.Archived)
                    .Include(t => t.TicketPriority)
                    .Include(t => t.TicketType)
                    .Include(t => t.TicketStatus)
                    .Include(t => t.DeveloperUser)
                    .Include(t => t.SubmitterUser)
                    .Include(t => t.Comments).ThenInclude(c => c.User)
                    .Include(t => t.Attatchment).ThenInclude(a => a.User)
                    .Include(t => t.History).ThenInclude(h => h.User)
                    .Include(t => t.Project).ThenInclude(p => p!.ProjectPriority)
                    .ToListAsync();
            }
            catch { throw; }
        }

        public async Task RestoreTicketAsync(Ticket ticket)
        {
            try
            {
                ticket.Archived = false;

                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch { throw; }
        }

        public async Task UpdateTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch { throw; }
        }
    }
}

