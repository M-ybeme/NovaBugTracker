using Microsoft.EntityFrameworkCore;
using NovaBugTracker.Data;
using NovaBugTracker.Models;
using NovaBugTracker.Services.Interfaces;

namespace NovaBugTracker.Services
{
    public class BTInviteService : IBTInviteService
    {
        private readonly ApplicationDbContext _context;

        public BTInviteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AcceptInviteAsync(Guid token, string userId, int companyId)
        {
            try
            {
                Invite? invite = await _context.Invites!.FirstOrDefaultAsync(i => i.CompanyToken == token);

                if(invite == null)
                {
                    return false;
                }

                try
                {
                    invite.IsValid = false;
                    invite.InviteeId = userId;
                    await _context.SaveChangesAsync();

                    return true;

                }
                catch (Exception)
                {

                    throw;
                }


            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddNewInviteAsync(Invite invite)
        {
            try
            {
                await _context.AddAsync(invite);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> AnyInviteAsync(Guid token, string email, int companyId)
        {
            try
            {
                bool result = await _context.Invites!.Where(i => i.CompanyId == companyId)
                                                    .AnyAsync(i => i.CompanyToken == token && i.InviteeEmail == email);
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Invite> GetInviteAsync(int inviteId, int companyId)
        {
            try
            {
                Invite? invite = await _context.Invites!
                                                .Where(i => i.CompanyId == companyId)
                                                .Include(i => i.Invitor)
                                                .Include(i => i.Project)
                                                .Include(i => i.Company)
                                                .FirstOrDefaultAsync( i=> i.Id == inviteId);
                return invite!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Invite> GetInviteAsync(Guid token, string email, int companyId)
        {
            try
            {
                Invite? invite = await _context.Invites!
                                              .Where(i => i.CompanyId == companyId && i.CompanyToken == token)
                                              .Include(i => i.Invitor)
                                              .Include(i => i.Project)
                                              .Include(i => i.Company)
                                              .FirstOrDefaultAsync(i => i.InviteeEmail == email);
                return invite!;
                                 
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> ValidateInviteCodeAsync(Guid? token)
        {
            try
            {

                if(token == null)
                {
                    return false;
                }

                bool result = false;

                Invite? invite = await _context.Invites!.FirstOrDefaultAsync(i => i.CompanyToken == token);

                if(invite != null)
                {
                    DateTime inviteDate = invite.InviteDate;

                    //Custom validation of invite based on teh date it was issued.
                    //In this case we will allow invite to be valid for 7 days.
                    bool validDate = (DateTime.Now - inviteDate).TotalDays <= 7;

                    if (validDate)
                    {
                        result = invite.IsValid;
                    }


                }
                return result;


            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
