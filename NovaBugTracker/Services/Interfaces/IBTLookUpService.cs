using NovaBugTracker.Models;

namespace NovaBugTracker.Services.Interfaces
{
	public interface IBTLookUpService
	{
        public Task<List<TicketPriority>> GetTicketPrioritiesAsync();
        public Task<List<TicketStatus>> GetTicketStatusesAsync();
        public Task<List<TicketType>> GetTicketTypesAsync();
        public Task<List<ProjectPriority>> GetProjectPrioritiesAsync();
        public Task<int?> LookupNotificationTypeIdAsync(string typeName);
        public Task<int?> LookupTicketStatusIdAsync(string statusName);
    }
}
