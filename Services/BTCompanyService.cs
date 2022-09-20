using Microsoft.EntityFrameworkCore;
using NovaBugTracker.Data;
using NovaBugTracker.Models;
using NovaBugTracker.Services.Interfaces;

namespace NovaBugTracker.Services
{
    public class BTCompanyService : IBTCompanyService
    {
        private readonly ApplicationDbContext _context;

        public BTCompanyService(ApplicationDbContext applicationDbContext)
        {
            _context = applicationDbContext;
        }
        public async Task<Company> GetCompanyInfoAsync(int? companyId)
        {
            try
            {
                Company? company = new();
                if(companyId != null)
                {
                    company = await _context.Companies!
                                            .Include(c => c.Members)
                                            .Include(c => c.Projects)
                                            .Include(c => c.Invites)
                                            .FirstOrDefaultAsync(c => c.Id == companyId);
                }
                return company!;

            }
            catch (Exception)
            {throw;}
        }

        public async Task<List<BTUser>> GetMembersAsync(int? companyId)
        {
            try
            {
                List<BTUser>? members = new();
                members = (await _context.Companies!
                                         .Include(c =>c.Members)
                                         .FirstOrDefaultAsync(c => c.Id == companyId))!
                                         .Members.ToList();
                return members;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
